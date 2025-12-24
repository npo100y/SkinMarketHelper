using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SkinMarketHelper.DAL.Entities;
using SkinMarketHelper.DAL.Interfaces;

namespace SkinMarketHelper.DAL
{
    public class SkinMarketRepository : ISkinMarketRepository, IDisposable
    {
        private readonly SkinMarketDbContext _context;

        public SkinMarketRepository(SkinMarketDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<Games> GetAllGames()
        {
            return _context.Games
                .OrderBy(g => g.Name)
                .ToList();
        }

        public IQueryable<MarketListings> GetActiveMarketListings()
        {
            return _context.MarketListings
                .Where(ml => ml.Status == "Active");
        }

        public MarketListings GetMarketListingById(int id)
        {
            return _context.MarketListings
                .Include(ml => ml.UserInventoryItems)
                .Include(ml => ml.Users)
                .Include(ml => ml.Users1)
                .SingleOrDefault(ml => ml.MarketListingID == id);
        }

        public Users GetUserById(int userId)
        {
            return _context.Users.SingleOrDefault(u => u.UserID == userId);
        }

        public Users GetUserBySteamId(string steamId64)
        {
            return _context.Users.SingleOrDefault(u => u.SteamID64 == steamId64);
        }

        public List<UserInventoryItems> GetUserInventory(int userId)
        {
            return _context.UserInventoryItems
                .Include(ii => ii.Items)
                .Where(ii => ii.UserID == userId)
                .ToList();
        }

        public List<ShoppingCartItems> GetUserCart(int userId)
        {
            return _context.ShoppingCartItems
                .Include(ci => ci.MarketListings)
                .Where(ci => ci.UserID == userId)
                .ToList();
        }

        public ShoppingCartItems GetCartItem(int userId, int listingId)
        {
            return _context.ShoppingCartItems
                .SingleOrDefault(ci =>
                    ci.UserID == userId &&
                    ci.MarketListingID == listingId);
        }

        public void AddCartItem(ShoppingCartItems item)
        {
            _context.ShoppingCartItems.Add(item);
        }

        public void RemoveCartItem(ShoppingCartItems item)
        {
            _context.ShoppingCartItems.Remove(item);
        }

        public void AddMarketListing(MarketListings listing)
        {
            _context.MarketListings.Add(listing);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
