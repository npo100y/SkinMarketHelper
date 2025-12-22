using System;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using System.Linq;
using SkinMarketHelper.Commands;
using SkinMarketHelper.Models;
using SkinMarketHelper.Services;

namespace SkinMarketHelper.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private readonly UserService _userService;
        private readonly ReportService _reportService;


        private string _statusMessage;
        private string _topUpAmountText;
        private string _reportFilePath;
        private string _withdrawAmountText;

        public User CurrentUser => _currentUser;

        public ObservableCollection<BalanceHistory> BalanceOperations { get; } =
            new ObservableCollection<BalanceHistory>();

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        public string TopUpAmountText
        {
            get => _topUpAmountText;
            set => SetProperty(ref _topUpAmountText, value);
        }

        public string WithdrawAmountText
        {
            get => _withdrawAmountText;
            set => SetProperty(ref _withdrawAmountText, value);
        }
        public string ReportFilePath
        {
            get => _reportFilePath;
            set => SetProperty(ref _reportFilePath, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand TopUpCommand { get; }
        public RelayCommand ExportReportCommand { get; }
        public RelayCommand WithdrawCommand { get; }

        public ProfileViewModel(User currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _userService = new UserService();
            _reportService = new ReportService();

            RefreshCommand = new RelayCommand(_ => Refresh());
            TopUpCommand = new RelayCommand(_ => TopUp());
            ExportReportCommand = new RelayCommand(_ => ExportReport());
            WithdrawCommand = new RelayCommand(_ => Withdraw());

            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ReportFilePath = Path.Combine(documents,
                $"SkinMarketHelper_{_currentUser.Username}_History_{DateTime.Now:yyyyMMdd}.pdf");

            LoadBalanceHistory();
        }

        private void Withdraw()
        {
            StatusMessage = null;

            if (string.IsNullOrWhiteSpace(WithdrawAmountText) ||
                !decimal.TryParse(WithdrawAmountText.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var amount))
            {
                StatusMessage = "Введите корректную сумму вывода.";
                return;
            }

            if (amount <= 0)
            {
                StatusMessage = "Сумма вывода должна быть больше нуля.";
                return;
            }

            if (_userService.WithdrawBalance(_currentUser.UserId, amount, out var errorMessage))
            {
                StatusMessage = $"Выведено {amount:F2} ₽.";
                _currentUser.Balance -= amount;
                OnPropertyChanged(nameof(CurrentUser));
                LoadBalanceHistory();
                WithdrawAmountText = string.Empty;
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }

        private void LoadBalanceHistory()
        {
            BalanceOperations.Clear();

            try
            {
                var operations = _userService.GetUserBalanceHistory(_currentUser.UserId);
                foreach (var op in operations)
                    BalanceOperations.Add(op);

                StatusMessage = $"Загружено операций: {BalanceOperations.Count}.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки истории баланса: " + ex.Message;
            }

            OnPropertyChanged(nameof(CurrentUser));
        }

        private void Refresh()
        {
            LoadBalanceHistory();
        }

        private void TopUp()
        {
            StatusMessage = null;

            if (string.IsNullOrWhiteSpace(TopUpAmountText) ||
                !decimal.TryParse(TopUpAmountText.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var amount))
            {
                StatusMessage = "Введите корректную сумму пополнения.";
                return;
            }

            if (amount <= 0)
            {
                StatusMessage = "Сумма пополнения должна быть больше нуля.";
                return;
            }

            if (_userService.TopUpBalance(_currentUser.UserId, amount, out var errorMessage))
            {
                StatusMessage = $"Баланс пополнен на {amount:F2} ₽.";
                _currentUser.Balance += amount;
                OnPropertyChanged(nameof(CurrentUser));
                LoadBalanceHistory();
                TopUpAmountText = string.Empty;
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }

        private void ExportReport()
        {

            var dialog = new SaveFileDialog
            {
                Title = "Сохранение отчёта",
                Filter = "PDF файлы (*.pdf)|*.pdf|Все файлы (*.*)|*.*",
                FileName = $"Отчёт_{_currentUser.Username}_{DateTime.Now:yyyyMMdd}.pdf",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            bool? result = dialog.ShowDialog();

            if (result != true)
                return;

            string filePath = dialog.FileName;

            StatusMessage = null;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                StatusMessage = "Укажите путь для сохранения отчёта.";
                return;
            }

            if (_reportService.ExportUserBalanceHistoryToPdf(
                    _currentUser,
                    BalanceOperations.ToList(),
                    filePath,
                    out var errorMessage))
            {
                StatusMessage = "Отчёт успешно сформирован: " + filePath;
            }
            else
            {
                StatusMessage = errorMessage;
            }
        }
    }
}
