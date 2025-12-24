using iTextSharp.text;
using iTextSharp.text.pdf;
using SkinMarketHelper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkinMarketHelper.Utils
{
    public static class PdfReportWriter
    {
        public static void WriteUserBalanceHistory(
            User user,
            IList<BalanceHistory> operations,
            string filePath)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (operations == null) throw new ArgumentNullException(nameof(operations));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            var document = new Document(PageSize.A4, 40, 40, 40, 40);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                PdfWriter.GetInstance(document, fs);
                document.Open();

                var baseFont = BaseFont.CreateFont(
                    @"C:\Windows\Fonts\arial.ttf",
                    BaseFont.IDENTITY_H,
                    BaseFont.NOT_EMBEDDED);

                var titleFont = new Font(baseFont, 14, Font.BOLD);
                var headerFont = new Font(baseFont, 10, Font.BOLD);
                var regularFont = new Font(baseFont, 10, Font.NORMAL);

                var title = new Paragraph(
                    $"Отчёт по движениям баланса пользователя {user.Username} (ID: {user.UserId})",
                    titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(title);

                var currentBalance = user.Balance;
                var info = new Paragraph(
                    $"Текущий баланс: {currentBalance:F2} ₽",
                    regularFont)
                {
                    SpacingAfter = 10
                };
                document.Add(info);

                var table = new PdfPTable(4)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 2, 2, 2, 6 });

                AddCell(table, "Дата", headerFont, isHeader: true);
                AddCell(table, "Тип", headerFont, isHeader: true);
                AddCell(table, "Сумма", headerFont, isHeader: true);
                AddCell(table, "Описание", headerFont, isHeader: true);

                foreach (var op in operations.OrderBy(o => o.CreatedAt))
                {
                    AddCell(table, op.CreatedAt.ToString("dd.MM.yyyy HH:mm"), regularFont);
                    AddCell(table, op.Type, regularFont);
                    AddCell(table, $"{op.Amount:F2} ₽", regularFont);
                    AddCell(table, op.Description, regularFont);
                }

                document.Add(table);
                document.Close();
            }
        }

        public static void WriteAdminSummary(
            AdminSummaryReportData data,
            string filePath)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

            var document = new Document(PageSize.A4, 40, 40, 40, 40);

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                PdfWriter.GetInstance(document, fs);
                document.Open();

                var baseFont = BaseFont.CreateFont(
                    @"C:\Windows\Fonts\arial.ttf",
                    BaseFont.IDENTITY_H,
                    BaseFont.NOT_EMBEDDED);

                var titleFont = new Font(baseFont, 14, Font.BOLD);
                var headerFont = new Font(baseFont, 10, Font.BOLD);
                var regularFont = new Font(baseFont, 10, Font.NORMAL);

                var title = new Paragraph("Сводный отчёт по площадке", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(title);

                var summary = new Paragraph(
                    $"Всего пользователей: {data.TotalUsers}\n" +
                    $"Активных лотов: {data.ActiveListings}\n" +
                    $"Проданных лотов: {data.SoldListings}\n" +
                    $"Общий оборот: {data.TotalTurnover:F2} ₽\n" +
                    $"Выручка продавцов: {data.SellerRevenue:F2} ₽\n" +
                    $"Комиссия площадки: {data.TotalCommission:F2} ₽",
                    regularFont)
                {
                    SpacingAfter = 20
                };
                document.Add(summary);

                var table = new PdfPTable(3)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 5, 2, 3 });

                AddCell(table, "Игра", headerFont, isHeader: true);
                AddCell(table, "Продано лотов", headerFont, isHeader: true);
                AddCell(table, "Сумма продаж, ₽", headerFont, isHeader: true);

                foreach (var g in data.TopGames)
                {
                    AddCell(table, g.GameName, regularFont);
                    AddCell(table, g.SoldCount.ToString(), regularFont);
                    AddCell(table, $"{g.SoldSum:F2}", regularFont);
                }

                document.Add(table);
                document.Close();
            }
        }

        private static void AddCell(
            PdfPTable table,
            string text,
            Font font,
            bool isHeader = false)
        {
            var cell = new PdfPCell(new Phrase(text ?? string.Empty, font))
            {
                Padding = 5,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };

            if (isHeader)
            {
                cell.BackgroundColor = new BaseColor(220, 220, 220);
            }

            table.AddCell(cell);
        }
    }
}
