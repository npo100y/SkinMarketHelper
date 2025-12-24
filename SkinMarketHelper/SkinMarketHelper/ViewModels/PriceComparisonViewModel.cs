using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using SkinMarketHelper.Commands;
using SkinMarketHelper.Models;
using SkinMarketHelper.Services;

namespace SkinMarketHelper.ViewModels
{
    public class PriceComparisonViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private readonly MarketService _marketService;

        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();
        public ObservableCollection<string> Types { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Rarities { get; } = new ObservableCollection<string>();
        public ObservableCollection<PriceComparisonEntry> Entries { get; } = new ObservableCollection<PriceComparisonEntry>();

        private Game _selectedGame;
        private string _selectedType;
        private string _selectedRarity;
        private string _minPriceText;
        private string _maxPriceText;
        private string _searchText;
        private PriceComparisonEntry _selectedEntry;
        private string _statusMessage;

        public Game SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (SetProperty(ref _selectedGame, value))
                {
                    LoadTypesAndRarities();
                    RefreshEntries();
                }
            }
        }

        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetProperty(ref _selectedType, value))
                {
                    RefreshEntries();
                }
            }
        }

        public string SelectedRarity
        {
            get => _selectedRarity;
            set
            {
                if (SetProperty(ref _selectedRarity, value))
                {
                    RefreshEntries();
                }
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

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public PriceComparisonEntry SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                if (SetProperty(ref _selectedEntry, value))
                {
                    OpenBestOfferCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ApplyFiltersCommand { get; }
        public RelayCommand OpenBestOfferCommand { get; }

        public PriceComparisonViewModel(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _marketService = new MarketService();

            RefreshCommand = new RelayCommand(_ => Refresh());
            ApplyFiltersCommand = new RelayCommand(_ => RefreshEntries());
            OpenBestOfferCommand = new RelayCommand(
        p => OpenBestOffer(p as PriceComparisonEntry),
        p =>
        {
            var entry = p as PriceComparisonEntry;
            return entry != null && !string.IsNullOrWhiteSpace(entry.BestMarketplaceUrl);
        });

            LoadGames();
            LoadTypesAndRarities();
            RefreshEntries();
        }

        private void LoadGames()
        {
            Games.Clear();
            Games.Add(new Game { GameId = 0, Name = "Все" });

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

        private void OpenBestOffer(PriceComparisonEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.BestMarketplaceUrl))
                return;

            try
            {
                Process.Start(new ProcessStartInfo(entry.BestMarketplaceUrl)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                StatusMessage = "Не удалось открыть ссылку: " + ex.Message;
            }
        }


        private void LoadTypesAndRarities()
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
                StatusMessage = "Ошибка загрузки фильтров по типу/редкости: " + ex.Message;
            }
        }

        private void Refresh()
        {
            LoadGames();
            LoadTypesAndRarities();
            RefreshEntries();
        }

        private void RefreshEntries()
        {
            Entries.Clear();

            try
            {
                int? gameId = null;
                if (SelectedGame != null && SelectedGame.GameId != 0)
                    gameId = SelectedGame.GameId;
                decimal? minPrice = null;
                decimal? maxPrice = null;

                if (!string.IsNullOrWhiteSpace(MinPriceText) &&
                    decimal.TryParse(MinPriceText.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var min))
                {
                    minPrice = min;
                }

                if (!string.IsNullOrWhiteSpace(MaxPriceText) &&
                    decimal.TryParse(MaxPriceText.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var max))
                {
                    maxPrice = max;
                }

                var list = _marketService.GetExternalPriceComparisons(
                    gameId,
                    SelectedType,
                    SelectedRarity,
                    minPrice,
                    maxPrice,
                    SearchText);

                foreach (var entry in list)
                    Entries.Add(entry);

                if (!Entries.Any())
                    StatusMessage = "По текущему фильтру внешние лоты не найдены.";
                else
                    StatusMessage = $"Найдено записей: {Entries.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки внешних цен: " + ex.Message;
            }
        }
    }
}
