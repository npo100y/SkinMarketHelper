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
                return repo.GetUserById(userId);
            }
        }
        public List<UserInventoryItem> GetUserInventory(int userId)
        {
            using (var context = new SkinMarketDbContext())
            {
                var repo = new SkinMarketRepository(context);
                return repo.GetUserInventory(userId);
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
                    var user = context.Users
                        .FirstOrDefault(u => u.SteamId64 == login || u.Username == login);

                    if (user == null)
                    {
                        errorMessage = "Пользователь не найден.";
                        return null;
                    }

                    return user;
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
                return repo.GetUserCart(userId);
            }
        }
        public IList<BalanceHistory> GetUserBalanceHistory(int userId)
        {
            using (var context = new SkinMarketDbContext())
            {
                return context.BalanceHistory
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();
            }
        }

        public bool WithdrawBalance(int userId, decimal amount, out string errorMessage)
        {
            errorMessage = null;

            if (amount <= 0)
            {
                errorMessage = "Сумма вывода должна быть больше нуля.";
                return false;
            }

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var user = context.Users.SingleOrDefault(u => u.UserId == userId);
                    if (user == null)
                    {
                        errorMessage = "Пользователь не найден.";
                        return false;
                    }

                    if (user.Balance < amount)
                    {
                        errorMessage = "Недостаточно средств для вывода.";
                        return false;
                    }

                    user.Balance -= amount;

                    context.BalanceHistory.Add(new BalanceHistory
                    {
                        UserId = userId,
                        Amount = -amount,
                        Type = "Вывод",
                        Description = "Вывод средств (демонстрационный сценарий)",
                        CreatedAt = DateTime.Now
                    });

                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка при выводе средств: " + ex.Message;
                return false;
            }
        }
        public bool TopUpBalance(int userId, decimal amount, out string errorMessage)
        {
            errorMessage = null;

            if (amount <= 0)
            {
                errorMessage = "Сумма пополнения должна быть больше нуля.";
                return false;
            }

            try
            {
                using (var context = new SkinMarketDbContext())
                {
                    var user = context.Users.SingleOrDefault(u => u.UserId == userId);
                    if (user == null)
                    {
                        errorMessage = "Пользователь не найден.";
                        return false;
                    }

                    user.Balance += amount;

                    context.BalanceHistory.Add(new BalanceHistory
                    {
                        UserId = userId,
                        Amount = amount,
                        Type = "Пополнение",
                        Description = "Пополнение через демонстрационный сценарий",
                        CreatedAt = DateTime.Now
                    });

                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Ошибка пополнения баланса: " + ex.Message;
                return false;
            }
        }
    }
}
