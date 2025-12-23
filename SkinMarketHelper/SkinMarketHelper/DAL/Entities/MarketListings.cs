namespace SkinMarketHelper.DAL.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MarketListings
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MarketListings()
        {
            ShoppingCartItems = new HashSet<ShoppingCartItems>();
        }

        [Key]
        public int MarketListingID { get; set; }

        public int InventoryItemID { get; set; }

        public int SellerUserID { get; set; }

        public int? BuyerUserID { get; set; }

        public decimal Price { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ListedAt { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? SoldAt { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }

        public virtual Users Users { get; set; }

        public virtual UserInventoryItems UserInventoryItems { get; set; }

        public virtual Users Users1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShoppingCartItems> ShoppingCartItems { get; set; }
    }
}
