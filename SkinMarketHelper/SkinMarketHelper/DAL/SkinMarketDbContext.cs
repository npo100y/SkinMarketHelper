using System.Data.Entity;
using SkinMarketHelper.Models;

namespace SkinMarketHelper.DAL
{
    public class SkinMarketDbContext : DbContext
    {
        public SkinMarketDbContext()
            : base("name=SkinMarketDbContext")
        {
            Database.SetInitializer<SkinMarketDbContext>(null);
        }

        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<BalanceHistory> BalanceHistory { get; set; }
        public virtual DbSet<UserInventoryItem> UserInventoryItems { get; set; }
        public virtual DbSet<Marketplace> Marketplaces { get; set; }
        public virtual DbSet<PriceListing> PriceListings { get; set; }
        public virtual DbSet<MarketListing> MarketListings { get; set; }
        public virtual DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Game>()
                .ToTable("Games")
                .HasKey(g => g.GameId);

            modelBuilder.Entity<Game>()
                .Property(g => g.GameId)
                .HasColumnName("GameID");

            modelBuilder.Entity<Game>()
                .Property(g => g.AppId)
                .HasColumnName("AppID");


            modelBuilder.Entity<Item>()
                .ToTable("Items")
                .HasKey(i => i.ItemId);

            modelBuilder.Entity<Item>()
                .Property(i => i.ItemId)
                .HasColumnName("ItemID");

            modelBuilder.Entity<Item>()
                .Property(i => i.GameId)
                .HasColumnName("GameID");

            modelBuilder.Entity<Item>()
                .HasRequired(i => i.Game)
                .WithMany(g => g.Items)
                .HasForeignKey(i => i.GameId);


            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .Property(u => u.UserId)
                .HasColumnName("UserID");


            modelBuilder.Entity<BalanceHistory>()
                .ToTable("BalanceHistory")
                .HasKey(b => b.BalanceHistoryId);

            modelBuilder.Entity<BalanceHistory>()
                .Property(b => b.BalanceHistoryId)
                .HasColumnName("BalanceHistoryID");

            modelBuilder.Entity<BalanceHistory>()
                .Property(b => b.UserId)
                .HasColumnName("UserID");

            modelBuilder.Entity<BalanceHistory>()
                .HasRequired(b => b.User)
                .WithMany(u => u.BalanceHistory)
                .HasForeignKey(b => b.UserId);


            modelBuilder.Entity<UserInventoryItem>()
                .ToTable("UserInventoryItems")
                .HasKey(ii => ii.InventoryItemId);

            modelBuilder.Entity<UserInventoryItem>()
                .Property(ii => ii.InventoryItemId)
                .HasColumnName("InventoryItemID");

            modelBuilder.Entity<UserInventoryItem>()
                .Property(ii => ii.UserId)
                .HasColumnName("UserID");

            modelBuilder.Entity<UserInventoryItem>()
                .Property(ii => ii.ItemId)
                .HasColumnName("ItemID");

            modelBuilder.Entity<UserInventoryItem>()
                .HasRequired(ii => ii.User)
                .WithMany(u => u.InventoryItems)
                .HasForeignKey(ii => ii.UserId);

            modelBuilder.Entity<UserInventoryItem>()
                .HasRequired(ii => ii.Item)
                .WithMany(i => i.UserInventoryItems)
                .HasForeignKey(ii => ii.ItemId);


            modelBuilder.Entity<Marketplace>()
                .ToTable("Marketplaces")
                .HasKey(m => m.MarketplaceId);

            modelBuilder.Entity<Marketplace>()
                .Property(m => m.MarketplaceId)
                .HasColumnName("MarketplaceID");


            modelBuilder.Entity<PriceListing>()
                .ToTable("PriceListings")
                .HasKey(pl => pl.PriceListingId);

            modelBuilder.Entity<PriceListing>()
                .Property(pl => pl.PriceListingId)
                .HasColumnName("PriceListingID");

            modelBuilder.Entity<PriceListing>()
                .Property(pl => pl.ItemId)
                .HasColumnName("ItemID");

            modelBuilder.Entity<PriceListing>()
                .Property(pl => pl.MarketplaceId)
                .HasColumnName("MarketplaceID");

            modelBuilder.Entity<PriceListing>()
                .HasRequired(pl => pl.Item)
                .WithMany(i => i.PriceListings)
                .HasForeignKey(pl => pl.ItemId);

            modelBuilder.Entity<PriceListing>()
                .HasRequired(pl => pl.Marketplace)
                .WithMany(m => m.PriceListings)
                .HasForeignKey(pl => pl.MarketplaceId);


            modelBuilder.Entity<MarketListing>()
                .ToTable("MarketListings")
                .HasKey(ml => ml.MarketListingId);

            modelBuilder.Entity<MarketListing>()
                .Property(ml => ml.MarketListingId)
                .HasColumnName("MarketListingID");

            modelBuilder.Entity<MarketListing>()
                .Property(ml => ml.InventoryItemId)
                .HasColumnName("InventoryItemID");

            modelBuilder.Entity<MarketListing>()
                .Property(ml => ml.SellerUserId)
                .HasColumnName("SellerUserID");

            modelBuilder.Entity<MarketListing>()
                .Property(ml => ml.BuyerUserId)
                .HasColumnName("BuyerUserID");


            modelBuilder.Entity<MarketListing>()
                .HasRequired(ml => ml.InventoryItem)
                .WithMany()
                .HasForeignKey(ml => ml.InventoryItemId);

            modelBuilder.Entity<MarketListing>()
                .HasRequired(ml => ml.Seller)
                .WithMany(u => u.SellListings)
                .HasForeignKey(ml => ml.SellerUserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MarketListing>()
                .HasOptional(ml => ml.Buyer)
                .WithMany(u => u.BuyListings)
                .HasForeignKey(ml => ml.BuyerUserId)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<ShoppingCartItem>()
                .ToTable("ShoppingCartItems")
                .HasKey(ci => ci.CartItemId);

            modelBuilder.Entity<ShoppingCartItem>()
                .Property(ci => ci.CartItemId)
                .HasColumnName("CartItemID");

            modelBuilder.Entity<ShoppingCartItem>()
                .Property(ci => ci.UserId)
                .HasColumnName("UserID");

            modelBuilder.Entity<ShoppingCartItem>()
                .Property(ci => ci.MarketListingId)
                .HasColumnName("MarketListingID");

            modelBuilder.Entity<ShoppingCartItem>()
                .HasRequired(ci => ci.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(ci => ci.UserId);

            modelBuilder.Entity<ShoppingCartItem>()
                .HasRequired(ci => ci.MarketListing)
                .WithMany()
                .HasForeignKey(ci => ci.MarketListingId);
        }
    }
}
