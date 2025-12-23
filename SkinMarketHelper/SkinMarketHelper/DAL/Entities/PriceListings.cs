namespace SkinMarketHelper.DAL.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PriceListings
    {
        [Key]
        public int PriceListingID { get; set; }

        public int ItemID { get; set; }

        public int MarketplaceID { get; set; }

        public decimal Price { get; set; }

        public decimal? FloatValue { get; set; }

        [StringLength(3)]
        public string CurrencyCode { get; set; }

        [StringLength(500)]
        public string ListingUrl { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }

        public virtual Items Items { get; set; }

        public virtual Marketplaces Marketplaces { get; set; }
    }
}
