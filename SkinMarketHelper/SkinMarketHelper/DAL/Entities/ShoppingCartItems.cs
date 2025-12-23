namespace SkinMarketHelper.DAL.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ShoppingCartItems
    {
        [Key]
        public int CartItemID { get; set; }

        public int UserID { get; set; }

        public int MarketListingID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? AddedAt { get; set; }

        public virtual MarketListings MarketListings { get; set; }

        public virtual Users Users { get; set; }
    }
}
