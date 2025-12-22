using System;
using System.Collections.ObjectModel;
using System.Linq;
using SkinMarketHelper.Commands;
using SkinMarketHelper.Models;
using SkinMarketHelper.Services;

namespace SkinMarketHelper.ViewModels
{
    public class CartViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private readonly UserService _userService;
        private readonly MarketService _marketService;

        public ObservableCollection<ShoppingCartItem> CartItems { get; } = new ObservableCollection<ShoppingCartItem>();

        private ShoppingCartItem _selectedCartItem;
        private string _statusMessage;
        private decimal _totalPrice;

        public ShoppingCartItem SelectedCartItem
        {
            get => _selectedCartItem;
            set
            {
                if (SetProperty(ref _selectedCartItem, value))
                {
                    RemoveFromCartCommand?.RaiseCanExecuteChanged();
                    BuySelectedCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand RemoveFromCartCommand { get; }
        public RelayCommand BuySelectedCommand { get; }
        public RelayCommand BuyAllCommand { get; }

        public CartViewModel(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _userService = new UserService();
            _marketService = new MarketService();

            RefreshCommand = new RelayCommand(_ => Refresh());
            RemoveFromCartCommand = new RelayCommand(_ => RemoveFromCart(), _ => SelectedCartItem != null);
            BuySelectedCommand = new RelayCommand(_ => BuySelected(), _ => SelectedCartItem != null);
            BuyAllCommand = new RelayCommand(_ => BuyAll(), _ => CartItems.Any());

            RefreshCart();
        }

        private void Refresh()
        {
            RefreshCart();
            StatusMessage = "Корзина обновлена.";
        }

        private void RefreshCart()
        {
            CartItems.Clear();

            try
            {
                var items = _userService.GetUserCart(_currentUser.UserId);

                foreach (var item in items)
                    CartItems.Add(item);

                TotalPrice = CartItems.Sum(ci => ci.MarketListing.Price);

                if (!CartItems.Any())
                {
                    StatusMessage = "Корзина пуста.";
                }
                else
                {
                    StatusMessage = $"Товаров в корзине: {CartItems.Count}, сумма: {TotalPrice:F2} ₽.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки корзины: " + ex.Message;
            }

            RemoveFromCartCommand?.RaiseCanExecuteChanged();
            BuySelectedCommand?.RaiseCanExecuteChanged();
            BuyAllCommand?.RaiseCanExecuteChanged();
        }

        private void RemoveFromCart()
        {
            if (SelectedCartItem == null)
                return;

            if (_marketService.RemoveFromCart(_currentUser.UserId,
                                              SelectedCartItem.MarketListingId,
                                              out var errorMessage))
            {
                StatusMessage = "Товар удалён из корзины.";
                RefreshCart();
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }

        private void BuySelected()
        {
            if (SelectedCartItem == null)
                return;

            if (_marketService.BuyListing(_currentUser.UserId,
                                          SelectedCartItem.MarketListingId,
                                          out var errorMessage))
            {
                StatusMessage = "Покупка успешно выполнена.";
                _currentUser.Balance -= SelectedCartItem.MarketListing.Price;
                RefreshCart();
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }

        private void BuyAll()
        {
            if (!CartItems.Any())
            {
                StatusMessage = "Корзина пуста.";
                return;
            }

            int successCount = 0;
            int failCount = 0;

            var items = CartItems.ToList();

            foreach (var ci in items)
            {
                if (_marketService.BuyListing(_currentUser.UserId,
                                              ci.MarketListingId,
                                              out var errorMessage))
                {
                    successCount++;
                    _currentUser.Balance -= ci.MarketListing.Price;
                }
                else
                {
                    failCount++;
                }
            }

            RefreshCart();
            StatusMessage = $"Успешно куплено: {successCount}, ошибок: {failCount}.";
        }
    }
}
