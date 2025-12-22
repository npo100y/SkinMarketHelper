using System;

namespace SkinMarketHelper.Models
{
    public partial class MarketListing
    {
        public int MarketListingId { get; set; }

        public int InventoryItemId { get; set; }
        public int SellerUserId { get; set; }
        public int? BuyerUserId { get; set; }

        public decimal Price { get; set; }
        public DateTime ListedAt { get; set; }
        public DateTime? SoldAt { get; set; }
        public string Status { get; set; }
        public DateTime UpdatedAt { get; set; }

        public UserInventoryItem InventoryItem { get; set; }
        public User Seller { get; set; }
        public User Buyer { get; set; }
    }
}
