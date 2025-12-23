namespace SkinMarketHelper.DAL.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Users()
        {
            BalanceHistory = new HashSet<BalanceHistory>();
            MarketListings = new HashSet<MarketListings>();
            MarketListings1 = new HashSet<MarketListings>();
            ShoppingCartItems = new HashSet<ShoppingCartItems>();
            UserInventoryItems = new HashSet<UserInventoryItems>();
        }

        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(20)]
        public string SteamID64 { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [StringLength(500)]
        public string TradeUrl { get; set; }

        [StringLength(500)]
        public string AvatarUrl { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? LastInventorySync { get; set; }

        public decimal? Balance { get; set; }

        [StringLength(20)]
        public string Role { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BalanceHistory> BalanceHistory { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MarketListings> MarketListings { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MarketListings> MarketListings1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShoppingCartItems> ShoppingCartItems { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserInventoryItems> UserInventoryItems { get; set; }
    }
}
