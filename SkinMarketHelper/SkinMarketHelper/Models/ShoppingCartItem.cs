using System;

namespace SkinMarketHelper.Models
{
    public class ShoppingCartItem
    {
        public int CartItemId { get; set; }
        public int UserId { get; set; }
        public int MarketListingId { get; set; }

        public DateTime AddedAt { get; set; }

        public User User { get; set; }
        public MarketListing MarketListing { get; set; }
    }
}
