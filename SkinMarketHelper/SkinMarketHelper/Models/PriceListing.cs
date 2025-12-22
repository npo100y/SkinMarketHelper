using System;

namespace SkinMarketHelper.Models
{
    public class PriceListing
    {
        public int PriceListingId { get; set; }
        public int ItemId { get; set; }
        public int MarketplaceId { get; set; }

        public decimal Price { get; set; }
        public decimal? FloatValue { get; set; }
        public string CurrencyCode { get; set; }
        public string ListingUrl { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Item Item { get; set; }
        public Marketplace Marketplace { get; set; }
    }
}
