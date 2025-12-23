using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkinMarketHelper.Models
{
    public partial class UserInventoryItem
    {
        public int InventoryItemId { get; set; }
        public int UserId { get; set; }
        public int ItemId { get; set; }

        public string AssetId { get; set; }
        public bool IsAvailableForTrade { get; set; }
        public DateTime AcquiredAt { get; set; }

        public User User { get; set; }
        public Item Item { get; set; }

        public bool IsOnSale { get; set; }
        public decimal? ActiveListingPrice { get; set; }
    }
}
