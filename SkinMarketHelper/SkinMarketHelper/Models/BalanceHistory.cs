using System;

namespace SkinMarketHelper.Models
{
    public class BalanceHistory
    {
        public int BalanceHistoryId { get; set; }
        public int UserId { get; set; }

        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
    }
}
