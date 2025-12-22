using System;
using System.Windows.Input;
using SkinMarketHelper.Commands;
using SkinMarketHelper.Models;
using SkinMarketHelper.Services;

namespace SkinMarketHelper.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly UserService _userService;

        private string _title = "SkinMarketHelper";
        private bool _isAuthenticated;
        private User _currentUser;
        private string _loginIdentifier;
        private string _loginError;


        private BaseViewModel _currentViewModel;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set
            {
                if (SetProperty(ref _isAuthenticated, value))
                {
                    OnPropertyChanged(nameof(IsAuthenticated));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    OnPropertyChanged(nameof(CurrentUser));
                    OnPropertyChanged(nameof(IsAdmin));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        public string LoginIdentifier
        {
            get => _loginIdentifier;
            set => SetProperty(ref _loginIdentifier, value);
        }
        public string LoginError
        {
            get => _loginError;
            set => SetProperty(ref _loginError, value);
        }
        public bool IsAdmin =>
            CurrentUser != null &&
            CurrentUser.Role != null &&
            CurrentUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public RelayCommand ShowCatalogCommand { get; }
        public RelayCommand ShowCartCommand { get; }
        public RelayCommand ShowInventoryCommand { get; }
        public RelayCommand ShowProfileCommand { get; }
        public RelayCommand ShowAdminCommand { get; }
        public RelayCommand ShowPriceComparisonCommand { get; }

        public MainViewModel()
        {
            _userService = new UserService();

            LoginCommand = new RelayCommand(_ => Login(), _ => !IsAuthenticated);
            LogoutCommand = new RelayCommand(_ => Logout(), _ => IsAuthenticated);

            ShowCatalogCommand = new RelayCommand(_ => ShowCatalog(), _ => IsAuthenticated);
            ShowPriceComparisonCommand = new RelayCommand(_ => ShowPriceComparison(), _ => IsAuthenticated);
            ShowCartCommand = new RelayCommand(_ => ShowCart(), _ => IsAuthenticated);
            ShowInventoryCommand = new RelayCommand(_ => ShowInventory(), _ => IsAuthenticated);
            ShowProfileCommand = new RelayCommand(_ => ShowProfile(), _ => IsAuthenticated);
            ShowAdminCommand = new RelayCommand(_ => ShowAdmin(), _ => IsAuthenticated && IsAdmin);
        }

        private void ShowPriceComparison()
        {
            if (!IsAuthenticated || CurrentUser == null)
                return;

            CurrentViewModel = new PriceComparisonViewModel(CurrentUser);
        }


        private void Login()
        {
            LoginError = null;

            var user = _userService.Login(LoginIdentifier, out var errorMessage);
            if (user == null)
            {
                LoginError = errorMessage;
                return;
            }

            CurrentUser = user;
            IsAuthenticated = true;
            Title = $"SkinMarketHelper - {CurrentUser.Username} ({CurrentUser.Role})";

            LoginIdentifier = string.Empty;

            ShowCatalog();
        }

        private void Logout()
        {
            CurrentUser = null;
            IsAuthenticated = false;
            Title = "SkinMarketHelper";
            LoginError = null;
            LoginIdentifier = string.Empty;
            CurrentViewModel = null;
        }

        private void ShowCatalog()
        {
            if (!IsAuthenticated || CurrentUser == null)
                return;

            CurrentViewModel = new CatalogViewModel(CurrentUser);
        }

        private void ShowCart()
        {
            if (!IsAuthenticated || CurrentUser == null)
                return;

            CurrentViewModel = new CartViewModel(CurrentUser);
        }

        private void ShowInventory()
        {
            if (!IsAuthenticated || CurrentUser == null)
                return;

            CurrentViewModel = new InventoryViewModel(CurrentUser);
        }

        private void ShowProfile()
        {
            if (!IsAuthenticated || CurrentUser == null)
                return;

            CurrentViewModel = new ProfileViewModel(CurrentUser);
        }

        private void ShowAdmin()
        {
            if (!IsAdmin || !IsAuthenticated || CurrentUser == null)
                return;

            CurrentViewModel = new AdminPanelViewModel(CurrentUser);
        }
    }
}
