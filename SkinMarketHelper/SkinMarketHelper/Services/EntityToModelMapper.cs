using System;
using SkinMarketHelper.DAL.Entities;

using ModelUser = SkinMarketHelper.Models.User;
using ModelGame = SkinMarketHelper.Models.Game;
using ModelItem = SkinMarketHelper.Models.Item;
using ModelUserInventoryItem = SkinMarketHelper.Models.UserInventoryItem;
using ModelMarketListing = SkinMarketHelper.Models.MarketListing;
using ModelShoppingCartItem = SkinMarketHelper.Models.ShoppingCartItem;
using ModelBalanceHistory = SkinMarketHelper.Models.BalanceHistory;
using ModelMarketplace = SkinMarketHelper.Models.Marketplace;
using ModelPriceListing = SkinMarketHelper.Models.PriceListing;

namespace SkinMarketHelper.Services
{
    internal static class EntityToModelMapper
    {
        public static ModelUser ToModel(this Users entity)
        {
            if (entity == null) return null;

            return new ModelUser
            {
                UserId = entity.UserID,
                SteamId64 = entity.SteamID64,
                Username = entity.Username,
                TradeUrl = entity.TradeUrl,
                AvatarUrl = entity.AvatarUrl,
                LastInventorySync = entity.LastInventorySync,
                Role = entity.Role,
                Balance = entity.Balance ?? 0m
                // ActiveListingsCount будет проставляться отдельно в AdminPanelViewModel
            };
        }

        public static ModelGame ToModel(this Games entity)
        {
            if (entity == null) return null;

            return new ModelGame
            {
                GameId = entity.GameID,
                AppId = entity.AppID,
                Name = entity.Name,
                LogoUrl = entity.LogoUrl
            };
        }

        public static ModelItem ToModel(this Items entity, bool includeGame = true)
        {
            if (entity == null) return null;

            var result = new ModelItem
            {
                ItemId = entity.ItemID,
                GameId = entity.GameID,
                Type = entity.Type,
                Name = entity.Name,
                Description = entity.Description,
                Rarity = entity.Rarity,
                IconUrl = entity.IconUrl,
                SteamMarketHashName = entity.SteamMarketHashName
            };

            if (includeGame && entity.Games != null)
            {
                result.Game = entity.Games.ToModel();
            }

            return result;
        }

        public static ModelUserInventoryItem ToModel(
            this UserInventoryItems entity,
            bool includeOwner = false,
            bool includeItem = true)
        {
            if (entity == null) return null;

            var result = new ModelUserInventoryItem
            {
                InventoryItemId = entity.InventoryItemID,
                UserId = entity.UserID,
                ItemId = entity.ItemID,
                AssetId = entity.AssetID,
                IsAvailableForTrade = entity.IsAvailableForTrade ?? false,
                AcquiredAt = entity.AcquiredAt ?? DateTime.MinValue
            };

            if (includeOwner && entity.Users != null)
            {
                result.User = entity.Users.ToModel();
            }

            if (includeItem && entity.Items != null)
            {
                result.Item = entity.Items.ToModel();
            }

            return result;
        }

        public static ModelMarketListing ToModel(
            this MarketListings entity,
            bool includeInventoryItem = true,
            bool includeSeller = true,
            bool includeBuyer = true)
        {
            if (entity == null) return null;

            var result = new ModelMarketListing
            {
                MarketListingId = entity.MarketListingID,
                InventoryItemId = entity.InventoryItemID,
                SellerUserId = entity.SellerUserID,
                BuyerUserId = entity.BuyerUserID,
                Price = entity.Price,
                ListedAt = entity.ListedAt ?? DateTime.MinValue,
                SoldAt = entity.SoldAt,
                Status = entity.Status,
                UpdatedAt = entity.UpdatedAt ?? DateTime.MinValue
            };

            if (includeInventoryItem && entity.UserInventoryItems != null)
            {
                result.InventoryItem = entity.UserInventoryItems
                    .ToModel(includeOwner: includeSeller || includeBuyer, includeItem: true);
            }

            if (includeSeller && entity.Users1 != null)
            {
                result.Seller = entity.Users1.ToModel();
            }

            if (includeBuyer && entity.Users != null)
            {
                result.Buyer = entity.Users.ToModel();
            }

            return result;
        }

        public static ModelShoppingCartItem ToModel(this ShoppingCartItems entity)
        {
            if (entity == null) return null;

            return new ModelShoppingCartItem
            {
                CartItemId = entity.CartItemID,
                UserId = entity.UserID,
                MarketListingId = entity.MarketListingID,
                AddedAt = entity.AddedAt ?? DateTime.MinValue,
                User = entity.Users?.ToModel(),
                MarketListing = entity.MarketListings?.ToModel()
            };
        }

        public static ModelBalanceHistory ToModel(this BalanceHistory entity)
        {
            if (entity == null) return null;

            return new ModelBalanceHistory
            {
                BalanceHistoryId = entity.BalanceHistoryID,
                UserId = entity.UserID,
                Amount = entity.Amount,
                Type = entity.Type,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt ?? DateTime.MinValue
            };
        }

        public static ModelMarketplace ToModel(this Marketplaces entity)
        {
            if (entity == null) return null;

            return new ModelMarketplace
            {
                MarketplaceId = entity.MarketplaceID,
                Name = entity.Name,
                WebsiteUrl = entity.WebsiteUrl,
                LogoUrl = entity.LogoUrl,
                IsEnabled = entity.IsEnabled ?? false,
                AddedAt = entity.AddedAt ?? DateTime.MinValue
            };
        }

        public static ModelPriceListing ToModel(
            this PriceListings entity,
            bool includeItem = true,
            bool includeMarketplace = true)
        {
            if (entity == null) return null;

            var result = new ModelPriceListing
            {
                PriceListingId = entity.PriceListingID,
                ItemId = entity.ItemID,
                MarketplaceId = entity.MarketplaceID,
                Price = entity.Price,
                FloatValue = entity.FloatValue,
                CurrencyCode = entity.CurrencyCode,
                ListingUrl = entity.ListingUrl,
                UpdatedAt = entity.UpdatedAt ?? DateTime.MinValue
            };

            if (includeItem && entity.Items != null)
            {
                result.Item = entity.Items.ToModel();
            }

            if (includeMarketplace && entity.Marketplaces != null)
            {
                result.Marketplace = entity.Marketplaces.ToModel();
            }

            return result;
        }
    }
}
