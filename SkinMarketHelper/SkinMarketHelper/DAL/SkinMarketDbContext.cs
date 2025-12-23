using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using SkinMarketHelper.DAL.Entities;

namespace SkinMarketHelper.DAL
{
    public partial class SkinMarketDbContext : DbContext
    {
        public SkinMarketDbContext()
            : base("name=SkinMarketDbContext")
        {
        }

        public virtual DbSet<BalanceHistory> BalanceHistory { get; set; }
        public virtual DbSet<Games> Games { get; set; }
        public virtual DbSet<Items> Items { get; set; }
        public virtual DbSet<MarketListings> MarketListings { get; set; }
        public virtual DbSet<Marketplaces> Marketplaces { get; set; }
        public virtual DbSet<PriceListings> PriceListings { get; set; }
        public virtual DbSet<ShoppingCartItems> ShoppingCartItems { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<UserInventoryItems> UserInventoryItems { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Games>()
                .HasMany(e => e.Items)
                .WithRequired(e => e.Games)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Items>()
                .HasMany(e => e.UserInventoryItems)
                .WithRequired(e => e.Items)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MarketListings>()
                .HasMany(e => e.ShoppingCartItems)
                .WithRequired(e => e.MarketListings)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Marketplaces>()
                .HasMany(e => e.PriceListings)
                .WithRequired(e => e.Marketplaces)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PriceListings>()
                .Property(e => e.FloatValue)
                .HasPrecision(8, 6);

            modelBuilder.Entity<UserInventoryItems>()
                .HasMany(e => e.MarketListings)
                .WithRequired(e => e.UserInventoryItems)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.MarketListings)
                .WithOptional(e => e.Users)
                .HasForeignKey(e => e.BuyerUserID);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.MarketListings1)
                .WithRequired(e => e.Users1)
                .HasForeignKey(e => e.SellerUserID)
                .WillCascadeOnDelete(false);
        }
    }
}
