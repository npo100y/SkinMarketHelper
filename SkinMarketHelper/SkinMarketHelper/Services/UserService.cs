using System;
using System.Collections.Generic;
using System.Linq;
using SkinMarketHelper.DAL;
using SkinMarketHelper.Models;

namespace SkinMarketHelper.Services
{
    public class UserService
    {
        public User GetUser(int userId)
        {
            using (var context = new SkinMarketDbContext())
            {
                var repo = new SkinMarketRepository(context);
                var entity = repo.GetUserById(userId);
                return entity?.ToModel();
            }
        }

        public List<UserInventoryItem> GetUserInventory(int userId)
        {
            using (var context = new SkinMarketDbContext())
            {
                var repo = new SkinMarketRepository(context);
                var entities = repo.GetUserInventory(userId);
                return entities
                    .Select(ii => ii.ToModel(includeOwner: false, includeItem: true))
                    .ToList();
            }
        }

        public User Login(string login, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(login))
            {
                errorMessage = "Введите SteamID64 или имя пользователя.";
                return null;
            }

            login = login.Trim();

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    // Ищем либо по SteamID64, либо по Username
                    var userEntity = context.Users
                        .FirstOrDefault(u => u.SteamID64 == login || u.Username == login);

                    if (userEntity == null)
                    {
                        errorMessage = "Пользователь не найден.";
                        return null;
                    }

                    return userEntity.ToModel();
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при обращении к базе данных: " + ex.Message;
                return null;
            }
        }

        public List<ShoppingCartItem> GetUserCart(int userId)
        {
            using (var context = new SkinMarketDbContext())
            {
                var repo = new SkinMarketRepository(context);
                var entities = repo.GetUserCart(userId);
                return entities
                    .Select(ci => ci.ToModel())
                    .ToList();
            }
        }

        public IList<BalanceHistory> GetUserBalanceHistory(int userId)
        {
            using (var context = new SkinMarketDbContext())
            {
                var entities = context.BalanceHistory
                    .Where(b => b.UserID == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();

                return entities
                    .Select(b => b.ToModel())
                    .ToList();
            }
        }

        public bool WithdrawBalance(int userId, decimal amount, out string errorMessage)
        {
            errorMessage = null;

            if (amount <= 0)
            {
                errorMessage = "Сумма должна быть положительной.";
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

                    if ((user.Balance ?? 0m) < amount)
                    {
                        errorMessage = "Недостаточно средств на балансе.";
                        return false;
                    }

                    user.Balance = (user.Balance ?? 0m) - amount;

                    context.BalanceHistory.Add(new SkinMarketHelper.DAL.Entities.BalanceHistory
                    {
                        UserID = userId,
                        Amount = -amount,
                        Type = "Вывод",
                        Description = $"Вывод средств {amount:0.00}",
                        CreatedAt = DateTime.Now
                    });

                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при обращении к базе данных: " + ex.Message;
                return false;
            }
        }

        public bool TopUpBalance(int userId, decimal amount, out string errorMessage)
        {
            errorMessage = null;

            if (amount <= 0)
            {
                errorMessage = "Сумма должна быть положительной.";
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

                    user.Balance = (user.Balance ?? 0m) + amount;

                    context.BalanceHistory.Add(new SkinMarketHelper.DAL.Entities.BalanceHistory
                    {
                        UserID = userId,
                        Amount = amount,
                        Type = "Пополнение",
                        Description = $"Пополнение баланса на {amount:0.00}",
                        CreatedAt = DateTime.Now
                    });

                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при обращении к базе данных: " + ex.Message;
                return false;
            }
        }
    }
}
