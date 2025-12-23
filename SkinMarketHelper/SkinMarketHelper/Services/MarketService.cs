using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using SkinMarketHelper.DAL;
using SkinMarketHelper.Models;

namespace SkinMarketHelper.Services
{
    public class MarketService
    {
        public const decimal MinListingPrice = 0.5m;
        public List<Game> GetGames()
        {
            using (var context = new SkinMarketDbContext())
            {
                var repo = new SkinMarketRepository(context);
                return repo.GetAllGames();
            }
        }
        public List<MarketListing> GetCatalog(int? gameId, string searchText, string sortBy)
        {
            using (var context = new SkinMarketDbContext())
            {
                var repo = new SkinMarketRepository(context);
                var query = repo.GetActiveMarketListings();

                if (gameId.HasValue)
                {
                    query = query.Where(ml => ml.InventoryItem.Item.GameId == gameId.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchText = searchText.Trim();
                    query = query.Where(ml =>
                        ml.InventoryItem.Item.Name.Contains(searchText));
                }

                switch (sortBy)
                {
                    case "price_asc":
                        query = query.OrderBy(ml => ml.Price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(ml => ml.Price);
                        break;
                    default:
                        query = query.OrderBy(ml => ml.MarketListingId);
                        break;
                }

                return query.ToList();
            }
        }
        public bool AddToCart(int userId, int listingId, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var repo = new SkinMarketRepository(context);

                    var user = repo.GetUserById(userId);
                    if (user == null)
                    {
                        errorMessage = "Пользователь не найден.";
                        return false;
                    }

                    var listing = repo.GetMarketListingById(listingId);
                    if (listing == null || listing.Status != "Active")
                    {
                        errorMessage = "Лот недоступен для покупки.";
                        return false;
                    }

                    var existing = repo.GetCartItem(userId, listingId);
                    if (existing != null)
                    {
                        errorMessage = "Этот товар уже в корзине.";
                        return false;
                    }

                    var cartItem = new ShoppingCartItem
                    {
                        UserId = userId,
                        MarketListingId = listingId,
                        AddedAt = DateTime.Now
                    };

                    repo.AddCartItem(cartItem);
                    repo.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при добавлении в корзину: " + ex.Message;
                return false;
            }
        }
        public bool RemoveFromCart(int userId, int listingId, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var repo = new SkinMarketRepository(context);

                    var item = repo.GetCartItem(userId, listingId);
                    if (item == null)
                    {
                        errorMessage = "Товар не найден в корзине.";
                        return false;
                    }

                    repo.RemoveCartItem(item);
                    repo.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при удалении из корзины: " + ex.Message;
                return false;
            }
        }

        public bool CancelListingByOwner(int sellerUserId, int inventoryItemId, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var listing = context.MarketListings
                        .SingleOrDefault(ml =>
                            ml.InventoryItemId == inventoryItemId &&
                            ml.SellerUserId == sellerUserId &&
                            ml.Status == "Active");

                    if (listing == null)
                    {
                        errorMessage = "Активный лот для этого предмета не найден.";
                        return false;
                    }

                    listing.Status = "Cancelled";
                    listing.UpdatedAt = DateTime.Now;

                    var cartItems = context.ShoppingCartItems
                        .Where(ci => ci.MarketListingId == listing.MarketListingId)
                        .ToList();
                    context.ShoppingCartItems.RemoveRange(cartItems);

                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при снятии предмета с продажи: " + ex.Message;
                return false;
            }
        }

        public bool BuyListing(int buyerUserId, int listingId, out string errorMessage)
        {
            errorMessage = null;


            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var repo = new SkinMarketRepository(context);

                    var buyer = repo.GetUserById(buyerUserId);
                    if (buyer == null)
                    {
                        errorMessage = "Покупатель не найден.";
                        return false;
                    }

                    var listing = repo.GetMarketListingById(listingId);

                    if (listing.SellerUserId == buyerUserId)
                    {
                        errorMessage = "Нельзя купить собственный лот.";
                        return false;
                    }

                    if (listing == null || listing.Status != "Active")
                    {
                        errorMessage = "Лот недоступен.";
                        return false;
                    }

                    var seller = repo.GetUserById(listing.SellerUserId);
                    if (seller == null)
                    {
                        errorMessage = "Продавец не найден.";
                        return false;
                    }

                    if (buyer.Balance < listing.Price)
                    {
                        errorMessage = "Недостаточно средств на балансе.";
                        return false;
                    }

                    var sellerAmount = Math.Round(listing.Price * 0.95m, 2);
                    var commission = listing.Price - sellerAmount;

                    buyer.Balance -= listing.Price;
                    seller.Balance += sellerAmount;

                    context.BalanceHistory.Add(new BalanceHistory
                    {
                        UserId = buyer.UserId,
                        Amount = -listing.Price,
                        Type = "Покупка",
                        Description = $"Покупка лота #{listing.MarketListingId}",
                        CreatedAt = DateTime.Now
                    });

                    context.BalanceHistory.Add(new BalanceHistory
                    {
                        UserId = seller.UserId,
                        Amount = sellerAmount,
                        Type = "Продажа",
                        Description = $"Продажа лота #{listing.MarketListingId} (комиссия сервиса 5%)",
                        CreatedAt = DateTime.Now
                    });

                    var inventoryItem = listing.InventoryItem;
                    if (inventoryItem != null)
                    {
                        inventoryItem.UserId = buyerUserId;
                        inventoryItem.AcquiredAt = DateTime.Now;
                    }

                    listing.BuyerUserId = buyerUserId;
                    listing.Status = "Sold";
                    listing.SoldAt = DateTime.Now;
                    listing.UpdatedAt = DateTime.Now;

                    var cartItems = context.ShoppingCartItems
                        .Where(ci => ci.MarketListingId == listingId)
                        .ToList();
                    context.ShoppingCartItems.RemoveRange(cartItems);

                    repo.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при покупке лота: " + ex.Message;
                return false;
            }
        }
        public IList<PriceComparisonEntry> GetExternalPriceComparisons(
            int? gameId,
            string typeFilter,
            string rarityFilter,
            decimal? minPrice,
            decimal? maxPrice,
            string searchText)
        {
            using (var context = new SkinMarketDbContext())
            {
                var query = context.PriceListings
                    .Include(pl => pl.Item.Game)
                    .Include(pl => pl.Marketplace)
                    .AsQueryable();

                if (gameId.HasValue)
                    query = query.Where(pl => pl.Item.GameId == gameId.Value);

                if (!string.IsNullOrWhiteSpace(typeFilter) && typeFilter != "Все")
                    query = query.Where(pl => pl.Item.Type == typeFilter);

                if (!string.IsNullOrWhiteSpace(rarityFilter) && rarityFilter != "Все")
                    query = query.Where(pl => pl.Item.Rarity == rarityFilter);

                if (minPrice.HasValue)
                    query = query.Where(pl => pl.Price >= minPrice.Value);

                if (maxPrice.HasValue)
                    query = query.Where(pl => pl.Price <= maxPrice.Value);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchText = searchText.Trim();
                    query = query.Where(pl =>
                        pl.Item.Name.Contains(searchText) ||
                        pl.Item.SteamMarketHashName.Contains(searchText));
                }

                var list = query.ToList();

                var result = list
                    .GroupBy(pl => pl.ItemId)
                    .Select(g =>
                    {
                        var best = g.OrderBy(pl => pl.Price).First();
                        return new PriceComparisonEntry
                        {
                            Item = best.Item,
                            BestPrice = best.Price,
                            CurrencyCode = best.CurrencyCode,
                            BestMarketplaceName = best.Marketplace.Name,
                            BestMarketplaceUrl = best.ListingUrl,
                            UpdatedAt = best.UpdatedAt
                        };
                    })
                    .OrderBy(e => e.BestPrice)
                    .ToList();

                return result;
            }
        }

        public IList<string> GetItemTypes(int? gameId = null)
        {
            using (var context = new SkinMarketDbContext())
            {
                var query = context.Items.AsQueryable();

                if (gameId.HasValue)
                    query = query.Where(i => i.GameId == gameId.Value);

                return query
                    .Select(i => i.Type)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();
            }
        }

        public IList<string> GetItemRarities(int? gameId = null)
        {
            using (var context = new SkinMarketDbContext())
            {
                var query = context.Items.AsQueryable();

                if (gameId.HasValue)
                    query = query.Where(i => i.GameId == gameId.Value);

                return query
                    .Select(i => i.Rarity)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();
            }
        }

        public IList<int> GetActiveInventoryItemIdsWithActiveListings(IEnumerable<int> inventoryItemIds)
        {
            using (var context = new SkinMarketDbContext())
            {
                var ids = inventoryItemIds.ToList();
                if (!ids.Any()) return new List<int>();

                return context.MarketListings
                    .Where(ml => ids.Contains(ml.InventoryItemId) && ml.Status == "Active")
                    .Select(ml => ml.InventoryItemId)
                    .Distinct()
                    .ToList();
            }
        }

        public IDictionary<int, decimal> GetActiveListingPricesForInventoryItems(IEnumerable<int> inventoryItemIds)
        {
            using (var context = new SkinMarketDbContext())
            {
                var ids = inventoryItemIds?.ToList() ?? new List<int>();
                if (!ids.Any())
                    return new Dictionary<int, decimal>();

                return context.MarketListings
                    .Where(ml => ids.Contains(ml.InventoryItemId) && ml.Status == "Active")
                    .Select(ml => new { ml.InventoryItemId, ml.Price })
                    .ToList()
                    .GroupBy(x => x.InventoryItemId)
                    .ToDictionary(g => g.Key, g => g.First().Price);
            }
        }


        public decimal? GetBestInternalListingPriceForItem(int itemId)
        {
            using (var context = new SkinMarketDbContext())
            {
                var best = context.MarketListings
                    .Include(ml => ml.InventoryItem)
                    .Where(ml => ml.Status == "Active" && ml.InventoryItem.ItemId == itemId)
                    .OrderBy(ml => ml.Price)
                    .Select(ml => (decimal?)ml.Price)
                    .FirstOrDefault();

                return best;
            }
        }

        public decimal? GetBestExternalPriceForItem(int itemId, out string marketplaceName)
        {
            using (var context = new SkinMarketDbContext())
            {
                var best = context.PriceListings
                    .Include(pl => pl.Marketplace)
                    .Where(pl => pl.ItemId == itemId)
                    .OrderBy(pl => pl.Price)
                    .FirstOrDefault();

                if (best == null)
                {
                    marketplaceName = null;
                    return null;
                }

                marketplaceName = best.Marketplace?.Name;
                return best.Price;
            }
        }
        public bool CreateListingFromInventoryItem(int sellerUserId, int inventoryItemId, decimal price, out string errorMessage)
        {
            errorMessage = null;

            if (price < MinListingPrice)
            {
                errorMessage = $"Минимальная цена выставления — {MinListingPrice:F2} ₽.";
                return false;
            }

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var repo = new SkinMarketRepository(context);

                    var seller = repo.GetUserById(sellerUserId);
                    if (seller == null)
                    {
                        errorMessage = "Продавец не найден.";
                        return false;
                    }

                    var inventoryItem = context.UserInventoryItems
                        .SingleOrDefault(ii => ii.InventoryItemId == inventoryItemId && ii.UserId == sellerUserId);

                    if (inventoryItem == null)
                    {
                        errorMessage = "Предмет инвентаря не найден у текущего пользователя.";
                        return false;
                    }

                    if (!inventoryItem.IsAvailableForTrade)
                    {
                        errorMessage = "Предмет недоступен для обмена.";
                        return false;
                    }

                    var existingListing = context.MarketListings
                        .SingleOrDefault(ml => ml.InventoryItemId == inventoryItemId);

                    if (existingListing == null)
                    {
                        var newListing = new MarketListing
                        {
                            InventoryItemId = inventoryItemId,
                            SellerUserId = sellerUserId,
                            BuyerUserId = null,
                            Price = price,
                            ListedAt = DateTime.Now,
                            SoldAt = null,
                            Status = "Active",
                            UpdatedAt = DateTime.Now
                        };

                        repo.AddMarketListing(newListing);
                    }
                    else
                    {
                        if (existingListing.Status == "Active")
                        {
                            errorMessage = "Этот предмет уже выставлен на продажу.";
                            return false;
                        }

                        existingListing.SellerUserId = sellerUserId;
                        existingListing.BuyerUserId = null;
                        existingListing.Price = price;
                        existingListing.ListedAt = DateTime.Now;
                        existingListing.SoldAt = null;
                        existingListing.Status = "Active";
                        existingListing.UpdatedAt = DateTime.Now;
                    }

                    repo.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при создании лота: " + ex.Message;
                return false;
            }
        }

    }
}
