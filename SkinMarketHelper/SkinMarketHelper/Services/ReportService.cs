using iTextSharp.text;
using iTextSharp.text.pdf;
using SkinMarketHelper.DAL;
using SkinMarketHelper.Models;
using SkinMarketHelper.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SkinMarketHelper.Services
{
    public class ReportService
    {
        public bool ExportUserBalanceHistoryToPdf(User user, IList<BalanceHistory> operations, string filePath, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                if (user == null)
                {
                    errorMessage = "Пользователь не задан.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    errorMessage = "Не указан путь к файлу.";
                    return false;
                }

                PdfReportWriter.WriteUserBalanceHistory(user, operations, filePath);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при формировании PDF-отчёта: " + ex.Message;
                return false;
            }
        }

        public bool ExportAdminSummaryToPdf(string filePath, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var totalUsers = context.Users.Count();
                    var activeListings = context.MarketListings.Count(ml => ml.Status == "Active");
                    var soldListings = context.MarketListings.Count(ml => ml.Status == "Sold");

                    var totalTurnover = context.MarketListings
                        .Where(ml => ml.Status == "Sold")
                        .Select(ml => (decimal?)ml.Price)
                        .Sum() ?? 0m;

                    var totalCommission = Math.Round(totalTurnover * 0.05m, 2);
                    var sellerRevenue = totalTurnover - totalCommission;

                    PdfReportWriter.WriteAdminSummary(context, totalUsers, activeListings, soldListings, totalTurnover, sellerRevenue, totalCommission, filePath);

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при формировании отчёта администратора: " + ex.Message;
                return false;
            }
        }
    }
}
