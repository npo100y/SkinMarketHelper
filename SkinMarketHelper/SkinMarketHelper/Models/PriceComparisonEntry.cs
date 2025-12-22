namespace SkinMarketHelper.Models
{
    public class PriceComparisonEntry
    {
        public Item Item { get; set; }

        public decimal BestPrice { get; set; }
        public string CurrencyCode { get; set; }

        public string BestMarketplaceName { get; set; }
        public string BestMarketplaceUrl { get; set; }

        public System.DateTime? UpdatedAt { get; set; }
    }
}
