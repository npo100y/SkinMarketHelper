using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SkinMarketHelper.DAL;
using SkinMarketHelper.Models;

namespace SkinMarketHelper.Services
{
    public class AdminService
    {
        public IList<User> GetAllUsers()
        {
            using (var context = new SkinMarketDbContext())
            {
                var entities = context.Users
                    .OrderBy(u => u.Username)
                    .ToList();

                return entities
                    .Select(u => u.ToModel())
                    .ToList();
            }
        }

        public IList<MarketListing> GetAllListings()
        {
            using (var context = new SkinMarketDbContext())
            {
                var entities = context.MarketListings
                    .Include(ml => ml.UserInventoryItems.Items.Games)
                    .Include(ml => ml.Users1)
                    .Include(ml => ml.Users)
                    .OrderByDescending(ml => ml.ListedAt)
                    .ToList();

                return entities
                    .Select(ml => ml.ToModel())
                    .ToList();
            }
        }
        public bool UpdateUserRole(int userId, string newRole, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(newRole))
            {
                errorMessage = "Роль не задана.";
                return false;
            }

            newRole = newRole.Trim();

            if (!string.Equals(newRole, "User", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(newRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Недопустимое значение роли. Используйте User или Admin.";
                return false;
            }

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var user = context.Users.SingleOrDefault(u => u.UserID == userId);
                    if (user == null)
                    {
                        errorMessage = "Пользователь не найден.";
                        return false;
                    }

                    user.Role = newRole;
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при изменении роли: " + ex.Message;
                return false;
            }
        }
        public IDictionary<int, int> GetActiveListingCountsBySeller()
        {
            using (var context = new SkinMarketDbContext())
            {
                return context.MarketListings
                    .Where(ml => ml.Status == "Active")
                    .GroupBy(ml => ml.SellerUserID)
                    .Select(g => new { SellerUserId = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.SellerUserId, x => x.Count);
            }
        }

        public bool CancelListing(int listingId, out string errorMessage)
        {
            errorMessage = null;

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var listing = context.MarketListings
                        .Include(ml => ml.UserInventoryItems)
                        .SingleOrDefault(ml => ml.MarketListingID == listingId);

                    if (listing == null)
                    {
                        errorMessage = "Лот не найден.";
                        return false;
                    }

                    if (!string.Equals(listing.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = "Отменить можно только активный лот.";
                        return false;
                    }

                    listing.Status = "Cancelled";
                    listing.UpdatedAt = DateTime.Now;

                    var cartItems = context.ShoppingCartItems
                        .Where(ci => ci.MarketListingID == listingId)
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
    }
}
