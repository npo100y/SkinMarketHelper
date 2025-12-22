using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SkinMarketHelper.Models
{
    public class User : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public string SteamId64 { get; set; }
        public string Username { get; set; }
        public string TradeUrl { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime? LastInventorySync { get; set; }
        public string Role { get; set; }

        private decimal _balance;

        public ICollection<BalanceHistory> BalanceHistory { get; set; }
        public ICollection<UserInventoryItem> InventoryItems { get; set; }
        public ICollection<MarketListing> SellListings { get; set; }
        public ICollection<MarketListing> BuyListings { get; set; }
        public ICollection<ShoppingCartItem> CartItems { get; set; }

        public decimal Balance
        {
            get => _balance;
            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    OnPropertyChanged(nameof(Balance));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
