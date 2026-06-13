using Microsoft.Win32;
using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using SwimmingPool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SwimmingPool.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ClientRepository _clientRepository;
        private readonly MembershipRepository _membershipRepository;
        private readonly ReportsRepository _reportsRepository;
        private List<Client> _allClients;

        public ICommand QuickRegisterCommand { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterClients(); }
        }

        private string _qrScannerInput;
        public string QrScannerInput
        {
            get => _qrScannerInput;
            set
            {
                _qrScannerInput = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(_qrScannerInput) && Guid.TryParse(_qrScannerInput, out Guid token))
                {
                    CheckInByToken(token);
                }
            }
        }

        public ObservableCollection<Client> SearchResults { get; set; }

        private Client _selectedClient;
        public Client SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(); }
        }

        public ICommand CheckInCommand { get; }

        public DailyStats TodaysStats { get; set; }
        public ObservableCollection<Membership> ExpiringMemberships { get; set; }
        public ICommand RefreshWidgetsCommand { get; }

        public DashboardViewModel()
        {
            _clientRepository = new ClientRepository();
            _membershipRepository = new MembershipRepository();
            _reportsRepository = new ReportsRepository();

            SearchResults = new ObservableCollection<Client>();
            TodaysStats = new DailyStats();
            ExpiringMemberships = new ObservableCollection<Membership>();

            LoadDataForSearch();
            LoadWidgetsData();

            QuickRegisterCommand = new RelayCommand(QuickRegister);
            CheckInCommand = new RelayCommand(CheckIn, CanCheckIn);
            RefreshWidgetsCommand = new RelayCommand(param => LoadWidgetsData());
        }

        private void LoadDataForSearch()
        {
            _allClients = _clientRepository.GetAllClients();
            FilterClients();
        }

        private void LoadWidgetsData()
        {
            try
            {
                var stats = _reportsRepository.GetTodaysStats();
                TodaysStats.NewMembershipsToday = stats.NewMembershipsToday;
                TodaysStats.RevenueToday = stats.RevenueToday;

                ExpiringMemberships.Clear();
                var expiring = _membershipRepository.GetExpiringSoonMemberships();
                foreach (var item in expiring)
                {
                    ExpiringMemberships.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке данных для виджетов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterClients()
        {
            SearchResults.Clear();
            var source = _allClients ?? new List<Client>();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var client in source) SearchResults.Add(client);
            }
            else
            {
                var lowerSearchText = SearchText.ToLower();
                var filtered = source.Where(c => c.FullName.ToLower().Contains(lowerSearchText) ||
                                                     (c.PhoneNumber != null && c.PhoneNumber.Contains(SearchText)));
                foreach (var client in filtered) SearchResults.Add(client);
            }
        }

        private void QuickRegister(object parameter)
        {
            var window = new QuickRegistrationWindow();
            if (window.ShowDialog() == true)
            {
                try
                {
                    var newClient = window.Client;
                    var newMembership = window.Membership;

                    int newClientId = _clientRepository.AddClient(newClient);
                    newClient.ClientId = newClientId;

                    newMembership.ClientId = newClientId;
                    _membershipRepository.AddMembership(newMembership);

                    MessageBox.Show($"Клиент '{newClient.FullName}' и его абонемент успешно зарегистрированы!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = "PDF Document (*.pdf)|*.pdf",
                        FileName = $"Пропуск_{newClient.FullName.Replace(" ", "_")}.pdf",
                        Title = "Сохранить пропуск клиента"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        PdfExportService.GenerateClientPass(newClient, saveFileDialog.FileName);
                    }

                    LoadDataForSearch();
                    LoadWidgetsData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CheckIn(object parameter)
        {
            if (SelectedClient == null) return;
            ProcessCheckIn(SelectedClient);
        }

        private void CheckInByToken(Guid token)
        {
            var client = _allClients.FirstOrDefault(c => c.ClientToken == token);
            if (client != null)
            {
                ProcessCheckIn(client);
                QrScannerInput = string.Empty;
            }
        }

        private void ProcessCheckIn(Client client)
        {
            var activeMembership = _membershipRepository.GetActiveMembershipForClient(client.ClientId);

            if (activeMembership != null)
            {
                MessageBox.Show($"ВХОД РАЗРЕШЕН\nКлиент: {client.FullName}\nАбонемент: {activeMembership.MembershipTypeName}\nДействителен до: {activeMembership.ExpiryDate:dd.MM.yyyy}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"ВХОД ЗАПРЕЩЕН\nКлиент: {client.FullName}\nНет активного абонемента!",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool CanCheckIn(object parameter) => SelectedClient != null;
    }
}