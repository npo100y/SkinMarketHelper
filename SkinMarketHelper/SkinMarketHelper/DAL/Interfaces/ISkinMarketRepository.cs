using System.Linq;
using System.Collections.Generic;
using SkinMarketHelper.DAL.Entities;
using SkinMarketHelper.DAL;

namespace SkinMarketHelper.DAL.Interfaces
{
    public interface ISkinMarketRepository
    {
        List<Games> GetAllGames();
        IQueryable<MarketListings> GetActiveMarketListings();
        MarketListings GetMarketListingById(int id);

        Users GetUserById(int userId);
        Users GetUserBySteamId(string steamId64);

        List<UserInventoryItems> GetUserInventory(int userId);

        List<ShoppingCartItems> GetUserCart(int userId);
        ShoppingCartItems GetCartItem(int userId, int listingId);

        void AddCartItem(ShoppingCartItems item);
        void RemoveCartItem(ShoppingCartItems item);

        void AddMarketListing(MarketListings listing);

        void SaveChanges();
    }
}
