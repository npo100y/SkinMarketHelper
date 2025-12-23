namespace SkinMarketHelper.DAL.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UserInventoryItems
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserInventoryItems()
        {
            MarketListings = new HashSet<MarketListings>();
        }

        [Key]
        public int InventoryItemID { get; set; }

        public int UserID { get; set; }

        public int ItemID { get; set; }

        [Required]
        [StringLength(100)]
        public string AssetID { get; set; }

        public bool? IsAvailableForTrade { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? AcquiredAt { get; set; }

        public virtual Items Items { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MarketListings> MarketListings { get; set; }

        public virtual Users Users { get; set; }
    }
}
