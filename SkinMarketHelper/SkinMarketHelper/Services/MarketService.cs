using SkinMarketHelper.DAL;
using SkinMarketHelper.DAL.Entities;
using SkinMarketHelper.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

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
                var entities = repo.GetAllGames();
                return entities.Select(g => g.ToModel()).ToList();
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
                    query = query.Where(ml => ml.UserInventoryItems.Items.GameID == gameId.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchText = searchText.Trim();
                    query = query.Where(ml =>
                        ml.UserInventoryItems.Items.Name.Contains(searchText));
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
                        query = query.OrderBy(ml => ml.MarketListingID);
                        break;
                }

                var entities = query.ToList();
                return entities.Select(ml => ml.ToModel()).ToList();
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
                        errorMessage = "Этот лот уже есть в корзине.";
                        return false;
                    }

                    var cartItemEntity = new SkinMarketHelper.DAL.Entities.ShoppingCartItems
                    {
                        UserID = userId,
                        MarketListingID = listingId,
                        AddedAt = DateTime.Now
                    };

                    repo.AddCartItem(cartItemEntity);
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
                            ml.InventoryItemID == inventoryItemId &&
                            ml.SellerUserID == sellerUserId &&
                            ml.Status == "Active");

                    if (listing == null)
                    {
                        errorMessage = "Активный лот не найден.";
                        return false;
                    }

                    listing.Status = "Cancelled";
                    listing.UpdatedAt = DateTime.Now;

                    var cartItems = context.ShoppingCartItems
                        .Where(ci => ci.MarketListingID == listing.MarketListingID)
                        .ToList();

                    context.ShoppingCartItems.RemoveRange(cartItems);
                    context.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при отмене лота: " + ex.Message;
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

                    if (listing.SellerUserID == buyerUserId)
                    {
                        errorMessage = "Нельзя купить собственный лот.";
                        return false;
                    }

                    if (listing == null || listing.Status != "Active")
                    {
                        errorMessage = "Лот недоступен.";
                        return false;
                    }

                    var seller = repo.GetUserById(listing.SellerUserID);
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

                    context.BalanceHistory.Add(new SkinMarketHelper.DAL.Entities.BalanceHistory
                    {
                        UserID = buyer.UserID,
                        Amount = -listing.Price,
                        Type = "Покупка",
                        Description = $"Покупка лота #{listing.MarketListingID}",
                        CreatedAt = DateTime.Now
                    });

                    context.BalanceHistory.Add(new SkinMarketHelper.DAL.Entities.BalanceHistory
                    {
                        UserID = seller.UserID,
                        Amount = sellerAmount,
                        Type = "Продажа",
                        Description = $"Продажа лота #{listing.MarketListingID} (комиссия сервиса 5%)",
                        CreatedAt = DateTime.Now
                    });

                    var inventoryItem = listing.UserInventoryItems;
                    if (inventoryItem != null)
                    {
                        inventoryItem.UserID = buyerUserId;
                        inventoryItem.AcquiredAt = DateTime.Now;
                    }

                    listing.BuyerUserID = buyerUserId;
                    listing.Status = "Sold";
                    listing.SoldAt = DateTime.Now;
                    listing.UpdatedAt = DateTime.Now;

                    var cartItems = context.ShoppingCartItems
                        .Where(ci => ci.MarketListingID == listingId)
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
                    .Include(pl => pl.Items.Games)
                    .Include(pl => pl.Marketplaces)
                    .AsQueryable();

                if (gameId.HasValue)
                    query = query.Where(pl => pl.Items.GameID == gameId.Value);

                if (!string.IsNullOrWhiteSpace(typeFilter) && typeFilter != "Все")
                    query = query.Where(pl => pl.Items.Type == typeFilter);

                if (!string.IsNullOrWhiteSpace(rarityFilter) && rarityFilter != "Все")
                    query = query.Where(pl => pl.Items.Rarity == rarityFilter);

                if (minPrice.HasValue)
                    query = query.Where(pl => pl.Price >= minPrice.Value);

                if (maxPrice.HasValue)
                    query = query.Where(pl => pl.Price <= maxPrice.Value);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchText = searchText.Trim();
                    query = query.Where(pl =>
                        pl.Items.Name.Contains(searchText) ||
                        pl.Items.SteamMarketHashName.Contains(searchText));
                }

                var list = query
                    .Where(pl => pl.Items != null)
                    .ToList();

                var result = list
                    .GroupBy(pl => pl.ItemID)
                    .Select(g =>
                    {
                        var best = g.OrderBy(pl => pl.Price).First();
                        var itemEntity = best.Items;
                        var gameEntity = itemEntity?.Games;

                        var itemModel = new Item
                        {
                            ItemId = itemEntity.ItemID,
                            GameId = itemEntity.GameID,
                            Type = itemEntity.Type,
                            Name = itemEntity.Name,
                            Description = itemEntity.Description,
                            Rarity = itemEntity.Rarity,
                            IconUrl = itemEntity.IconUrl,
                            SteamMarketHashName = itemEntity.SteamMarketHashName,
                            Game = gameEntity == null ? null : new Game
                                {
                                    GameId = gameEntity.GameID,
                                    AppId = gameEntity.AppID,
                                    Name = gameEntity.Name,
                                    LogoUrl = gameEntity.LogoUrl
                                }
                        };

                        return new PriceComparisonEntry
                        {
                            Item = itemModel,
                            BestPrice = best.Price,
                            CurrencyCode = best.CurrencyCode,
                            BestMarketplaceName = best.Marketplaces?.Name,
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
                {
                    query = query.Where(i => i.GameID == gameId.Value);
                }

                return query
                    .Select(i => i.Type)
                    .Where(t => t != null)
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
                    query = query.Where(i => i.GameID == gameId.Value);

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
                    .Where(ml => ids.Contains(ml.InventoryItemID) && ml.Status == "Active")
                    .Select(ml => ml.InventoryItemID)
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
                    .Where(ml => ids.Contains(ml.InventoryItemID) && ml.Status == "Active")
                    .Select(ml => new { ml.InventoryItemID, ml.Price })
                    .ToList()
                    .GroupBy(x => x.InventoryItemID)
                    .ToDictionary(g => g.Key, g => g.First().Price);
            }
        }


        public decimal? GetBestInternalListingPriceForItem(int itemId)
        {
            using (var context = new SkinMarketDbContext())
            {
                return context.MarketListings
                    .Where(ml => ml.Status == "Active" &&
                                 ml.UserInventoryItems.ItemID == itemId)
                    .OrderBy(ml => ml.Price)
                    .Select(ml => (decimal?)ml.Price)
                    .FirstOrDefault();
            }
        }


        public decimal? GetBestExternalPriceForItem(int itemId, out string marketplaceName)
        {
            marketplaceName = null;

            using (var context = new SkinMarketDbContext())
            {
                var best = context.PriceListings
                    .Include(pl => pl.Marketplaces)
                    .Where(pl => pl.ItemID == itemId)
                    .OrderBy(pl => pl.Price)
                    .FirstOrDefault();

                if (best == null)
                    return null;

                marketplaceName = best.Marketplaces?.Name;
                return best.Price;
            }
        }

        public bool CreateListingFromInventoryItem(int sellerUserId, int inventoryItemId, decimal price, out string errorMessage)
        {
            errorMessage = null;

            if (price <= 0)
            {
                errorMessage = "Цена должна быть положительной.";
                return false;
            }

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var repo = new SkinMarketRepository(context);

                    var inventoryItem = context.UserInventoryItems
                        .SingleOrDefault(ii =>
                            ii.InventoryItemID == inventoryItemId &&
                            ii.UserID == sellerUserId);

                    if (inventoryItem == null)
                    {
                        errorMessage = "Предмет инвентаря не найден у данного пользователя.";
                        return false;
                    }

                    var listingEntity = context.MarketListings
                        .SingleOrDefault(ml => ml.InventoryItemID == inventoryItemId);

                    if (listingEntity != null)
                    {
                        if (listingEntity.Status == "Active")
                        {
                            errorMessage = "Для этого предмета уже существует активный лот.";
                            return false;
                        }

                        listingEntity.SellerUserID = sellerUserId;
                        listingEntity.Price = price;
                        listingEntity.Status = "Active";
                        listingEntity.ListedAt = DateTime.Now;
                        listingEntity.UpdatedAt = DateTime.Now;
                        listingEntity.BuyerUserID = null;
                        listingEntity.SoldAt = null;
                    }
                    else
                    {
                        listingEntity = new MarketListings
                        {
                            InventoryItemID = inventoryItemId,
                            SellerUserID = sellerUserId,
                            Price = price,
                            ListedAt = DateTime.Now,
                            Status = "Active",
                            UpdatedAt = DateTime.Now
                        };

                        repo.AddMarketListing(listingEntity);
                    }

                    repo.SaveChanges();
                    return true;
                }
            }
            catch (DbUpdateException ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                var inner = ex.InnerException;
                while (inner != null)
                {
                    sb.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }

                errorMessage = "Ошибка при создании лота (ошибка обновления БД): " + sb;
                return false;
            }
            catch (SqlException ex)
            {
                errorMessage = "Ошибка при подключении/работе с базой данных: " + ex.Message;
                return false;
            }
            catch (EntityException ex)
            {
                errorMessage = "Ошибка уровня Entity Framework при обращении к базе данных: " + ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = "Неизвестная ошибка при создании лота: " + ex.Message;
                return false;
            }
        }

    }
}
