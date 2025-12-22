using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using SkinMarketHelper.Commands;
using SkinMarketHelper.Models;
using SkinMarketHelper.Services;

namespace SkinMarketHelper.ViewModels
{
    public class AdminPanelViewModel : BaseViewModel
    {
        private readonly User _currentAdmin;
        private readonly AdminService _adminService;
        private readonly ReportService _reportService;

        public ObservableCollection<User> Users { get; } =
            new ObservableCollection<User>();

        public ObservableCollection<MarketListing> Listings { get; } =
            new ObservableCollection<MarketListing>();

        public ObservableCollection<string> AvailableRoles { get; } =
            new ObservableCollection<string> { "User", "Admin" };

        private User _selectedUser;
        private string _selectedUserRole;
        private MarketListing _selectedListing;
        private string _statusMessage;
        private string _adminReportFilePath;


        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    SelectedUserRole = _selectedUser?.Role;
                }
            }
        }

        public string SelectedUserRole
        {
            get => _selectedUserRole;
            set => SetProperty(ref _selectedUserRole, value);
        }

        public string AdminReportFilePath
        {
            get => _adminReportFilePath;
            set => SetProperty(ref _adminReportFilePath, value);
        }

        public MarketListing SelectedListing
        {
            get => _selectedListing;
            set => SetProperty(ref _selectedListing, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand SaveUserRoleCommand { get; }
        public RelayCommand CancelListingCommand { get; }
        public RelayCommand ExportAdminReportCommand { get; }

        public AdminPanelViewModel(User currentAdmin)
        {
            _currentAdmin = currentAdmin ?? throw new ArgumentNullException(nameof(currentAdmin));
            _adminService = new AdminService();
            _reportService = new ReportService();

            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            AdminReportFilePath = Path.Combine(documents,
                $"SkinMarketHelper_AdminReport_{DateTime.Now:yyyyMMdd}.pdf");

            RefreshCommand = new RelayCommand(_ => Refresh());
            SaveUserRoleCommand = new RelayCommand(_ => SaveUserRole(), _ => SelectedUser != null && !string.IsNullOrWhiteSpace(SelectedUserRole));
            CancelListingCommand = new RelayCommand(_ => CancelListing(), _ => SelectedListing != null);
            ExportAdminReportCommand = new RelayCommand(_ => ExportAdminReport());

            LoadUsers();
            LoadListings();
        }

        private void LoadUsers()
        {
            Users.Clear();
            try
            {
                var users = _adminService.GetAllUsers();
                foreach (var u in users)
                {
                    Users.Add(u);
                }

                StatusMessage = $"Загружено пользователей: {Users.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки пользователей: " + ex.Message;
            }
        }

        private void LoadListings()
        {
            Listings.Clear();
            try
            {
                var listings = _adminService.GetAllListings();
                foreach (var l in listings)
                {
                    Listings.Add(l);
                }

                StatusMessage = $"Загружено лотов: {Listings.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки лотов: " + ex.Message;
            }
        }

        private void Refresh()
        {
            LoadUsers();
            LoadListings();
        }

        private void SaveUserRole()
        {
            if (SelectedUser == null)
            {
                StatusMessage = "Выберите пользователя.";
                return;
            }

            if (_adminService.UpdateUserRole(SelectedUser.UserId, SelectedUserRole, out var errorMessage))
            {
                SelectedUser.Role = SelectedUserRole;
                OnPropertyChanged(nameof(SelectedUser));
                StatusMessage = $"Роль пользователя {SelectedUser.Username} изменена на {SelectedUserRole}.";
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }

        private void ExportAdminReport()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Сохранение отчёта по площадке",
                Filter = "PDF файлы (*.pdf)|*.pdf|Все файлы (*.*)|*.*",
                FileName = $"Отчёт_площадка_{DateTime.Now:yyyyMMdd}.pdf",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            bool? result = dialog.ShowDialog();
            if (result != true)
                return;

            string filePath = dialog.FileName;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                StatusMessage = "Укажите путь для сохранения отчёта.";
                return;
            }

            if (_reportService.ExportAdminSummaryToPdf(filePath, out var errorMessage))
            {
                StatusMessage = "Административный отчёт сформирован: " + filePath;
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }


        private void CancelListing()
        {
            if (SelectedListing == null)
            {
                StatusMessage = "Выберите лот.";
                return;
            }

            if (_adminService.CancelListing(SelectedListing.MarketListingId, out var errorMessage))
            {
                SelectedListing.Status = "Cancelled";
                OnPropertyChanged(nameof(SelectedListing));
                StatusMessage = $"Лот #{SelectedListing.MarketListingId} отменён.";
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }
    }
}
