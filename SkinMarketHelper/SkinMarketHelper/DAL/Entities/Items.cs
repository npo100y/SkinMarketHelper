namespace SkinMarketHelper.DAL.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Items
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Items()
        {
            PriceListings = new HashSet<PriceListings>();
            UserInventoryItems = new HashSet<UserInventoryItems>();
        }

        [Key]
        public int ItemID { get; set; }

        public int GameID { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Rarity { get; set; }

        [StringLength(500)]
        public string IconUrl { get; set; }

        [Required]
        [StringLength(300)]
        public string SteamMarketHashName { get; set; }

        public virtual Games Games { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PriceListings> PriceListings { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserInventoryItems> UserInventoryItems { get; set; }
    }
}
