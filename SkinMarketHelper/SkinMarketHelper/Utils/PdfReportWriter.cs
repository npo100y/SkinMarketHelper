using iTextSharp.text;
using iTextSharp.text.pdf;
using SkinMarketHelper.DAL;
using SkinMarketHelper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkinMarketHelper.Utils
{
    public static class PdfReportWriter
    {
        public static void WriteUserBalanceHistory(User user, IList<BalanceHistory> operations, string filePath)
        {
            var document = new Document(PageSize.A4, 40, 40, 40, 40);

            try
            {
                var baseFont = BaseFont.CreateFont(
                    @"C:\Windows\Fonts\arial.ttf",
                    BaseFont.IDENTITY_H,
                    BaseFont.NOT_EMBEDDED);

                var fontRegular = new Font(baseFont, 10, Font.NORMAL);
                var fontBold = new Font(baseFont, 12, Font.BOLD);

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    PdfWriter.GetInstance(document, stream);
                    document.Open();

                    var header = new Paragraph($"Отчёт по истории операций: {user.Username}", fontBold)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 10
                    };

                    var dateStr = new Paragraph("Дата формирования: " + DateTime.Now.ToString("g"), fontRegular)
                    {
                        Alignment = Element.ALIGN_RIGHT,
                        SpacingAfter = 10
                    };

                    var userInfo = new Paragraph(
                        $"Пользователь: {user.Username} (ID: {user.UserId})\n" +
                        $"Роль: {user.Role}\n" +
                        $"Текущий баланс: {user.Balance:F2} ₽",
                        fontRegular)
                    {
                        SpacingAfter = 15
                    };

                    document.Add(header);
                    document.Add(dateStr);
                    document.Add(userInfo);

                    PdfPTable table = new PdfPTable(4) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 2f, 2f, 1f, 5f });

                    AddCell(table, "Дата", fontBold, true);
                    AddCell(table, "Тип", fontBold, true);
                    AddCell(table, "Сумма", fontBold, true);
                    AddCell(table, "Описание", fontBold, true);

                    if (operations != null)
                    {
                        foreach (var op in operations)
                        {
                            AddCell(table, op.CreatedAt.ToString("g"), fontRegular);
                            AddCell(table, op.Type, fontRegular);
                            AddCell(table, $"{op.Amount:F2} ₽", fontRegular);
                            AddCell(table, op.Description ?? string.Empty, fontRegular);
                        }
                    }

                    document.Add(table);
                    document.Close();
                }
            }
            catch (DocumentException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public static void WriteAdminSummary(SkinMarketDbContext context, int totalUsers, int activeListings, int soldListings, decimal totalTurnover, decimal sellerRevenue, decimal totalCommission, string filePath)
        {
            var topGames = context.MarketListings
                        .Where(ml => ml.Status == "Sold")
                        .GroupBy(ml => ml.UserInventoryItems.Items.Games.Name)
                        .Select(g => new
                        {
                            GameName = g.Key,
                            Count = g.Count(),
                            Sum = g.Sum(x => x.Price)
                        })
                        .OrderByDescending(g => g.Sum)
                        .Take(5)
                        .ToList();

            var document = new Document(PageSize.A4, 40, 40, 40, 40);

            var baseFont = BaseFont.CreateFont(
                @"C:\Windows\Fonts\arial.ttf",
                BaseFont.IDENTITY_H,
                BaseFont.NOT_EMBEDDED);

            var fontRegular = new Font(baseFont, 10, Font.NORMAL);
            var fontBold = new Font(baseFont, 12, Font.BOLD);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                PdfWriter.GetInstance(document, stream);
                document.Open();

                var header = new Paragraph("Отчёт по площадке SkinMarketHelper", fontBold)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };

                var dateStr = new Paragraph("Дата формирования: " + DateTime.Now.ToString("g"), fontRegular)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingAfter = 10
                };

                document.Add(header);
                document.Add(dateStr);

                var summary = new Paragraph(
                    $"Всего пользователей: {totalUsers}\n" +
                    $"Активных лотов: {activeListings}\n" +
                    $"Проданных лотов: {soldListings}\n" +
                    $"Оборот (продажи): {totalTurnover:F2} ₽\n" +
                    $"Доход продавцов: {sellerRevenue:F2} ₽\n" +
                    $"Комиссия сервиса (5%): {totalCommission:F2} ₽",
                    fontRegular)
                {
                    SpacingAfter = 15
                };
                document.Add(summary);

                if (topGames.Any())
                {
                    var topHeader = new Paragraph("Топ-игры по обороту:", fontBold)
                    {
                        SpacingAfter = 8
                    };
                    document.Add(topHeader);

                    PdfPTable table = new PdfPTable(3) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 4f, 2f, 2f });

                    AddCell(table, "Игра", fontBold, true);
                    AddCell(table, "Проданных лотов", fontBold, true);
                    AddCell(table, "Оборот, ₽", fontBold, true);

                    foreach (var g in topGames)
                    {
                        AddCell(table, g.GameName, fontRegular);
                        AddCell(table, g.Count.ToString(), fontRegular);
                        AddCell(table, g.Sum.ToString("F2"), fontRegular);
                    }

                    document.Add(table);
                }

                document.Close();
            }
        }

        private static void AddCell(PdfPTable table, string text, Font font, bool isHeader = false)
        {
            var cell = new PdfPCell(new Phrase(text, font))
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
