using System.Collections.Generic;

namespace SkinMarketHelper.Models
{
    public class AdminSummaryReportData
    {
        public int TotalUsers { get; set; }
        public int ActiveListings { get; set; }
        public int SoldListings { get; set; }

        public decimal TotalTurnover { get; set; }
        public decimal SellerRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public IList<AdminSummaryTopGameEntry> TopGames { get; set; } = new List<AdminSummaryTopGameEntry>();
    }

    public class AdminSummaryTopGameEntry
    {
        public string GameName { get; set; }
        public int SoldCount { get; set; }
        public decimal SoldSum { get; set; }
    }
}
