using System;
using System.Collections.ObjectModel;
using System.Linq;
using SkinMarketHelper.Commands;
using SkinMarketHelper.Models;
using SkinMarketHelper.Services;

namespace SkinMarketHelper.ViewModels
{
    public class InventoryViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private readonly UserService _userService;
        private readonly MarketService _marketService;

        public ObservableCollection<UserInventoryItem> InventoryItems { get; } = new ObservableCollection<UserInventoryItem>();
        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        private UserInventoryItem _selectedInventoryItem;
        private Game _selectedGame;
        private string _listingPriceText;
        private string _statusMessage;
        private string _expectedPayoutText;
        private string _internalBestPriceText;
        private string _externalBestPriceText;

        public UserInventoryItem SelectedInventoryItem
        {
            get => _selectedInventoryItem;
            set
            {
                if (SetProperty(ref _selectedInventoryItem, value))
                {
                    ListingPriceText = string.Empty;
                    ExpectedPayoutText = string.Empty;
                    UpdatePriceHints();
                    RemoveFromSaleCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        public string ExpectedPayoutText
        {
            get => _expectedPayoutText;
            set => SetProperty(ref _expectedPayoutText, value);
        }
        public string InternalBestPriceText
        {
            get => _internalBestPriceText;
            set => SetProperty(ref _internalBestPriceText, value);
        }
        public string ExternalBestPriceText
        {
            get => _externalBestPriceText;
            set => SetProperty(ref _externalBestPriceText, value);
        }
        public Game SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (SetProperty(ref _selectedGame, value))
                {
                    RefreshInventory();
                }
            }
        }
        public string ListingPriceText
        {
            get => _listingPriceText;
            set
            {
                if (SetProperty(ref _listingPriceText, value))
                {
                    if (!string.IsNullOrWhiteSpace(_listingPriceText) &&
                        decimal.TryParse(_listingPriceText.Replace(',', '.'),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var price) && price > 0)
                    {
                        var sellerAmount = Math.Round(price * 0.95m, 2);
                        ExpectedPayoutText = $"{sellerAmount:F2} ₽ (с учётом комиссии 5%)";
                    }
                    else
                    {
                        ExpectedPayoutText = string.Empty;
                    }
                }
            }
        }


        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand CreateListingCommand { get; }
        public RelayCommand RemoveFromSaleCommand { get; }

        public InventoryViewModel(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _userService = new UserService();
            _marketService = new MarketService();

            RefreshCommand = new RelayCommand(_ => Refresh());
            CreateListingCommand = new RelayCommand(_ => CreateListing(), _ => SelectedInventoryItem != null);
            RemoveFromSaleCommand = new RelayCommand(_ => RemoveFromSale(), _ => SelectedInventoryItem != null && SelectedInventoryItem.IsOnSale);

            LoadGames();
            RefreshInventory();
        }

        private void LoadGames()
        {
            Games.Clear();
            try
            {
                var games = _marketService.GetGames();
                foreach (var g in games)
                    Games.Add(g);
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки списка игр: " + ex.Message;
            }
        }

        private void UpdatePriceHints()
        {
            InternalBestPriceText = string.Empty;
            ExternalBestPriceText = string.Empty;

            if (SelectedInventoryItem?.Item == null)
                return;

            var itemId = SelectedInventoryItem.ItemId;

            try
            {
                var internalBest = _marketService.GetBestInternalListingPriceForItem(itemId);
                if (internalBest.HasValue)
                    InternalBestPriceText = $"{internalBest.Value:F2} ₽ – минимальная цена этого предмета на нашей площадке.";
                else
                    InternalBestPriceText = "Для этого предмета нет активных лотов на нашей площадке.";

                string marketplaceName;
                var externalBest = _marketService.GetBestExternalPriceForItem(itemId, out marketplaceName);
                if (externalBest.HasValue)
                {
                    if (!string.IsNullOrWhiteSpace(marketplaceName))
                        ExternalBestPriceText = $"{externalBest.Value:F2} ({marketplaceName}) – минимальная цена на внешних площадках.";
                    else
                        ExternalBestPriceText = $"{externalBest.Value:F2} – минимальная цена на внешних площадках.";
                }
                else
                {
                    ExternalBestPriceText = "Нет данных по внешним площадкам для этого предмета.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка при загрузке статистики цен: " + ex.Message;
            }
        }

        private void RemoveFromSale()
        {
            if (SelectedInventoryItem == null || !SelectedInventoryItem.IsOnSale)
            {
                StatusMessage = "Предмет не находится в продаже.";
                return;
            }

            if (_marketService.CancelListingByOwner(_currentUser.UserId,
                                                    SelectedInventoryItem.InventoryItemId,
                                                    out var errorMessage))
            {
                StatusMessage = "Предмет снят с продажи.";
                RefreshInventory();
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }


        private void Refresh()
        {
            RefreshInventory();
            StatusMessage = "Инвентарь обновлён.";
        }

        private void RefreshInventory()
        {
            InventoryItems.Clear();

            try
            {
                var items = _userService.GetUserInventory(_currentUser.UserId);

                if (SelectedGame != null)
                {
                    items = items
                        .Where(ii => ii.Item.GameId == SelectedGame.GameId)
                        .ToList();
                }

                var ids = items.Select(ii => ii.InventoryItemId).ToList();
                var onSaleIds = _marketService
                    .GetActiveInventoryItemIdsWithActiveListings(ids)
                    .ToHashSet();

                foreach (var item in items)
                {
                    item.IsOnSale = onSaleIds.Contains(item.InventoryItemId);
                    InventoryItems.Add(item);
                }

                if (!InventoryItems.Any())
                {
                    StatusMessage = "В инвентаре нет предметов по текущему фильтру.";
                }
                else
                {
                    StatusMessage = $"Предметов в инвентаре: {InventoryItems.Count}.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки инвентаря: " + ex.Message;
            }
        }

        private void CreateListing()
        {
            StatusMessage = null;

            if (SelectedInventoryItem == null)
            {
                StatusMessage = "Выберите предмет.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ListingPriceText) ||
                !decimal.TryParse(ListingPriceText.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                                  System.Globalization.CultureInfo.InvariantCulture, out var price))
            {
                StatusMessage = "Введите корректную цену.";
                return;
            }

            if (price <= 0)
            {
                StatusMessage = "Цена должна быть больше нуля.";
                return;
            }

            if (_marketService.CreateListingFromInventoryItem(_currentUser.UserId,
                                                             SelectedInventoryItem.InventoryItemId,
                                                             price,
                                                             out var errorMessage))
            {
                StatusMessage = "Лот успешно создан и добавлен в общий каталог.";
                RefreshInventory();
                ListingPriceText = string.Empty;
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }
    }
}
