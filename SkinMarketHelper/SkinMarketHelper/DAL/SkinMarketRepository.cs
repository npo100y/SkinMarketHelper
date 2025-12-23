using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SkinMarketHelper.Models;

namespace SkinMarketHelper.DAL
{
    public class SkinMarketRepository
    {
        private readonly SkinMarketDbContext _context;

        public SkinMarketRepository(SkinMarketDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<Game> GetAllGames()
        {
            return _context.Games
                .OrderBy(g => g.Name)
                .ToList();
        }

        public IQueryable<MarketListing> GetActiveMarketListings()
        {
            return _context.MarketListings
                .Include(ml => ml.InventoryItem.Item.Game)
                .Include(ml => ml.Seller)
                .Where(ml => ml.Status == "Active");
        }

        public MarketListing GetMarketListingById(int listingId)
        {
            return _context.MarketListings
                .Include(ml => ml.InventoryItem.Item.Game)
                .Include(ml => ml.Seller)
                .Include(ml => ml.Buyer)
                .SingleOrDefault(ml => ml.MarketListingId == listingId);
        }

        public User GetUserById(int userId)
        {
            return _context.Users.SingleOrDefault(u => u.UserId == userId);
        }

        public User GetUserBySteamId(string steamId64)
        {
            return _context.Users.SingleOrDefault(u => u.SteamId64 == steamId64);
        }

        public List<UserInventoryItem> GetUserInventory(int userId)
        {
            return _context.UserInventoryItems
                .Include(ii => ii.Item.Game)
                .Where(ii => ii.UserId == userId)
                .ToList();
        }

        public List<ShoppingCartItem> GetUserCart(int userId)
        {
            return _context.ShoppingCartItems
                .Include(ci => ci.MarketListing.InventoryItem.Item.Game)
                .Include(ci => ci.MarketListing.Seller)
                .Where(ci => ci.UserId == userId)
                .ToList();
        }

        public ShoppingCartItem GetCartItem(int userId, int listingId)
        {
            return _context.ShoppingCartItems
                .FirstOrDefault(ci => ci.UserId == userId && ci.MarketListingId == listingId);
        }

        public void AddCartItem(ShoppingCartItem item)
        {
            _context.ShoppingCartItems.Add(item);
        }

        public void RemoveCartItem(ShoppingCartItem item)
        {
            _context.ShoppingCartItems.Remove(item);
        }

        public void AddMarketListing(MarketListing listing)
        {
            _context.MarketListings.Add(listing);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
