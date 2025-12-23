using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

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
        private int _activeListingsCount;

        public ICollection<BalanceHistory> BalanceHistory { get; set; }
        public ICollection<UserInventoryItem> InventoryItems { get; set; }
        public ICollection<MarketListing> SellListings { get; set; }
        public ICollection<MarketListing> BuyListings { get; set; }
        public ICollection<ShoppingCartItem> CartItems { get; set; }

        [NotMapped]
        public int ActiveListingsCount
        {
            get => _activeListingsCount;
            set
            {
                if (_activeListingsCount != value)
                {
                    _activeListingsCount = value;
                    OnPropertyChanged(nameof(ActiveListingsCount));
                }
            }
        }

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
