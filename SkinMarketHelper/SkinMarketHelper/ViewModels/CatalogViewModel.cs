using System;
using System.Collections.ObjectModel;
using System.Linq;
using SkinMarketHelper.Commands;
using SkinMarketHelper.Models;
using SkinMarketHelper.Services;

namespace SkinMarketHelper.ViewModels
{
    public class CatalogViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private readonly MarketService _marketService;
        private readonly UserService _userService;

        private Game _selectedGame;
        private SortOption _selectedSortOption;
        private string _searchText;
        private MarketListing _selectedListing;
        private string _statusMessage;

        private string _selectedType;
        private string _selectedRarity;
        private string _minPriceText;
        private string _maxPriceText;

        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();
        public ObservableCollection<SortOption> SortOptions { get; } = new ObservableCollection<SortOption>();
        public ObservableCollection<MarketListing> Listings { get; } = new ObservableCollection<MarketListing>();

        public ObservableCollection<string> Types { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Rarities { get; } = new ObservableCollection<string>();

        public Game SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (SetProperty(ref _selectedGame, value))
                {
                    RefreshListings();
                    LoadTypeAndRarityFilters();
                }
            }
        }

        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetProperty(ref _selectedType, value))
                    RefreshListings();
            }
        }

        public string SelectedRarity
        {
            get => _selectedRarity;
            set
            {
                if (SetProperty(ref _selectedRarity, value))
                    RefreshListings();
            }
        }

        public string MinPriceText
        {
            get => _minPriceText;
            set => SetProperty(ref _minPriceText, value);
        }

        public string MaxPriceText
        {
            get => _maxPriceText;
            set => SetProperty(ref _maxPriceText, value);
        }

        public SortOption SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (SetProperty(ref _selectedSortOption, value))
                {
                    RefreshListings();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public MarketListing SelectedListing
        {
            get => _selectedListing;
            set
            {
                if (SetProperty(ref _selectedListing, value))
                {
                    AddToCartCommand?.RaiseCanExecuteChanged();
                    BuyNowCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddToCartCommand { get; }
        public RelayCommand BuyNowCommand { get; }

        public CatalogViewModel(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _marketService = new MarketService();
            _userService = new UserService();

            InitSortOptions();
            LoadGames();
            LoadTypeAndRarityFilters();
            RefreshListings();

            RefreshCommand = new RelayCommand(_ => Refresh());
            AddToCartCommand = new RelayCommand(_ => AddToCart(), _ => SelectedListing != null);
            BuyNowCommand = new RelayCommand(_ => BuyNow(), _ => SelectedListing != null);
        }

        private void InitSortOptions()
        {
            SortOptions.Clear();
            SortOptions.Add(new SortOption { DisplayName = "Без сортировки", Value = null });
            SortOptions.Add(new SortOption { DisplayName = "Цена ↑", Value = "price_asc" });
            SortOptions.Add(new SortOption { DisplayName = "Цена ↓", Value = "price_desc" });

            SelectedSortOption = SortOptions.First();
        }

        private void LoadTypeAndRarityFilters()
        {
            Types.Clear();
            Rarities.Clear();

            Types.Add("Все");
            Rarities.Add("Все");

            try
            {
                int? gameId = SelectedGame?.GameId;

                var types = _marketService.GetItemTypes(gameId);
                var rarities = _marketService.GetItemRarities(gameId);

                foreach (var t in types)
                    Types.Add(t);

                foreach (var r in rarities)
                    Rarities.Add(r);

                SelectedType = "Все";
                SelectedRarity = "Все";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки фильтров: " + ex.Message;
            }
        }


        private void LoadGames()
        {
            Games.Clear();

            Games.Add(new Game
            {
                GameId = 0,
                Name = "Все"
            });

            try
            {
                var games = _marketService.GetGames();
                foreach (var g in games)
                    Games.Add(g);

                SelectedGame = Games.FirstOrDefault();
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки игр: " + ex.Message;
            }
        }


        private void Refresh()
        {
            RefreshListings();
            LoadTypeAndRarityFilters();
            StatusMessage = "Список лотов обновлён.";
        }

        private void RefreshListings()
        {
            Listings.Clear();

            try
            {
                int? gameId = null;
                if (SelectedGame != null && SelectedGame.GameId != 0)
                    gameId = SelectedGame.GameId;

                string sortValue = SelectedSortOption?.Value;

                var data = _marketService.GetCatalog(gameId, SearchText, sortValue).ToList();

                if (!string.IsNullOrWhiteSpace(SelectedType) && SelectedType != "Все")
                    data = data.Where(l => l.InventoryItem.Item.Type == SelectedType).ToList();

                if (!string.IsNullOrWhiteSpace(SelectedRarity) && SelectedRarity != "Все")
                    data = data.Where(l => l.InventoryItem.Item.Rarity == SelectedRarity).ToList();

                decimal tmp;
                if (!string.IsNullOrWhiteSpace(MinPriceText) &&
                    decimal.TryParse(MinPriceText.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out tmp))
                {
                    data = data.Where(l => l.Price >= tmp).ToList();
                }

                if (!string.IsNullOrWhiteSpace(MaxPriceText) &&
                    decimal.TryParse(MaxPriceText.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out tmp))
                {
                    data = data.Where(l => l.Price <= tmp).ToList();
                }

                var cartItems = _userService.GetUserCart(_currentUser.UserId);
                var cartIds = cartItems
                    .Select(ci => ci.MarketListingId)
                    .ToHashSet();

                foreach (var listing in data)
                {
                    listing.IsInCurrentUserCart = cartIds.Contains(listing.MarketListingId);
                    listing.IsOwnedByCurrentUser = listing.SellerUserId == _currentUser.UserId;
                    Listings.Add(listing);
                }

                StatusMessage = Listings.Any()
                    ? $"Найдено лотов: {Listings.Count}."
                    : "По текущему фильтру лоты не найдены.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки каталога: " + ex.Message;
            }
        }


        private void AddToCart()
        {
            if (SelectedListing == null)
                return;

            StatusMessage = null;

            if (_marketService.AddToCart(_currentUser.UserId, SelectedListing.MarketListingId, out var errorMessage))
            {
                StatusMessage = "Товар добавлен в корзину.";
                RefreshListings();
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }

        private void BuyNow()
        {
            if (SelectedListing == null)
                return;

            StatusMessage = null;

            if (_marketService.BuyListing(_currentUser.UserId, SelectedListing.MarketListingId, out var errorMessage))
            {
                StatusMessage = "Покупка успешно выполнена.";
                RefreshListings();
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }
    }
}
