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
        public bool ExportUserBalanceHistoryToPdf(
            User user,
            IList<BalanceHistory> operations,
            string filePath,
            out string errorMessage)
        {
            errorMessage = null;

            try
            {
                if (user == null)
                {
                    errorMessage = "Пользователь не задан.";
                    return false;
                }

                if (operations == null || operations.Count == 0)
                {
                    errorMessage = "Нет операций для формирования отчёта.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    errorMessage = "Путь к файлу не задан.";
                    return false;
                }

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                PdfReportWriter.WriteUserBalanceHistory(user, operations, filePath);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при формировании отчёта: " + ex.Message;
                return false;
            }
        }
        public bool ExportAdminSummaryToPdf(string filePath, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    errorMessage = "Путь к файлу не задан.";
                    return false;
                }

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                AdminSummaryReportData data;

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

                    data = new AdminSummaryReportData
                    {
                        TotalUsers = totalUsers,
                        ActiveListings = activeListings,
                        SoldListings = soldListings,
                        TotalTurnover = totalTurnover,
                        SellerRevenue = sellerRevenue,
                        TotalCommission = totalCommission,
                        TopGames = topGames
                            .Select(g => new AdminSummaryTopGameEntry
                            {
                                GameName = g.GameName,
                                SoldCount = g.Count,
                                SoldSum = g.Sum
                            })
                            .ToList()
                    };
                }

                PdfReportWriter.WriteAdminSummary(data, filePath);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при формировании отчёта администратора: " + ex.Message;
                return false;
            }
        }
    }
}
