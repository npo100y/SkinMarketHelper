using System;
using System.Collections.Generic;

namespace SkinMarketHelper.Models
{
    public class Marketplace
    {
        public int MarketplaceId { get; set; }
        public string Name { get; set; }
        public string WebsiteUrl { get; set; }
        public string LogoUrl { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime AddedAt { get; set; }

        public ICollection<PriceListing> PriceListings { get; set; }
    }
}
