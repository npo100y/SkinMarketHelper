namespace SkinMarketHelper.DAL.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Marketplaces
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Marketplaces()
        {
            PriceListings = new HashSet<PriceListings>();
        }

        [Key]
        public int MarketplaceID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string WebsiteUrl { get; set; }

        [StringLength(500)]
        public string LogoUrl { get; set; }

        public bool? IsEnabled { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? AddedAt { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PriceListings> PriceListings { get; set; }
    }
}
