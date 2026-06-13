using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using SwimmingPool.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SwimmingPool.ViewModels
{
    public class ClientsViewModel : BaseViewModel
    {
        private readonly ClientRepository _clientRepository;
        private readonly WordExportService _wordService;

        private ObservableCollection<Client> _clientsSource;
        public ICollectionView ClientsView { get; private set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _birthDateFrom;
        public DateTime? BirthDateFrom
        {
            get => _birthDateFrom;
            set
            {
                _birthDateFrom = value;
                OnPropertyChanged();
                ClientsView.Refresh();
            }
        }

        private DateTime? _birthDateTo;
        public DateTime? BirthDateTo
        {
            get => _birthDateTo;
            set
            {
                _birthDateTo = value;
                OnPropertyChanged();
                ClientsView.Refresh();
            }
        }

        private Client _selectedClient;
        public Client SelectedClient { get => _selectedClient; set { _selectedClient = value; OnPropertyChanged(); } }

        public ICommand AddClientCommand { get; }
        public ICommand EditClientCommand { get; }
        public ICommand DeleteClientCommand { get; }
        public ICommand CreateContractCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand RegisterSickLeaveCommand { get; }

        public ClientsViewModel()
        {
            _clientRepository = new ClientRepository();
            _wordService = new WordExportService();

            _clientsSource = new ObservableCollection<Client>();
            ClientsView = CollectionViewSource.GetDefaultView(_clientsSource);
            ClientsView.Filter = FilterClients;

            LoadClients();

            AddClientCommand = new RelayCommand(AddClient);
            EditClientCommand = new RelayCommand(EditClient, CanEditOrDelete);
            DeleteClientCommand = new RelayCommand(DeleteClient, CanEditOrDelete);
            CreateContractCommand = new RelayCommand(CreateContract, CanEditOrDelete);
            ClearFilterCommand = new RelayCommand(ClearFilter);

            RegisterSickLeaveCommand = new RelayCommand(ExecuteRegisterSickLeave, CanEditOrDelete);
        }

        private bool FilterClients(object obj)
        {
            if (obj is Client client)
            {
                bool matchesDate = true;
                if (BirthDateFrom.HasValue)
                    matchesDate &= client.DateOfBirth >= BirthDateFrom.Value;
                if (BirthDateTo.HasValue)
                    matchesDate &= client.DateOfBirth <= BirthDateTo.Value;

                return matchesDate;
            }
            return false;
        }

        private void LoadClients()
        {
            _clientsSource.Clear();
            var items = _clientRepository.GetAllClients();
            foreach (var item in items) _clientsSource.Add(item);
            ClientsView.Refresh();
        }

        private void ClearFilter(object obj)
        {
            SearchText = string.Empty;
            BirthDateFrom = null;
            BirthDateTo = null;
            ClientsView.Refresh();
        }

        private void AddClient(object parameter)
        {
            var newClient = new Client();
            var window = new ClientWindow(newClient, "НОВЫЙ КЛИЕНТ");
            if (window.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(newClient.FullName) || string.IsNullOrWhiteSpace(newClient.PassportNumber))
                {
                    MessageBox.Show("Заполните обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _clientRepository.AddClient(newClient);
                LoadClients();
            }
        }

        private void EditClient(object parameter)
        {
            if (SelectedClient != null)
            {
                var clientToEdit = new Client
                {
                    ClientId = SelectedClient.ClientId,
                    FullName = SelectedClient.FullName,
                    DateOfBirth = SelectedClient.DateOfBirth,
                    PassportNumber = SelectedClient.PassportNumber,
                    PhoneNumber = SelectedClient.PhoneNumber,
                    Address = SelectedClient.Address
                };
                var window = new ClientWindow(clientToEdit, "РЕДАКТИРОВАНИЕ");
                if (window.ShowDialog() == true)
                {
                    _clientRepository.UpdateClient(clientToEdit);
                    LoadClients();
                }
            }
        }

        private void DeleteClient(object parameter)
        {
            if (SelectedClient != null)
            {
                if (MessageBox.Show($"Удалить {SelectedClient.FullName}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _clientRepository.DeleteClient(SelectedClient.ClientId);
                    LoadClients();
                }
            }
        }

        private void CreateContract(object parameter)
        {
            if (SelectedClient != null) _wordService.GenerateClientContract(SelectedClient);
        }

        private void ExecuteRegisterSickLeave(object obj)
        {
            if (SelectedClient == null) return;

            var sickLeaveRepository = new SickLeaveRepository(DatabaseService.ConnectionString);
            var membershipRepository = new MembershipRepository(); // Создаем репозиторий абонементов

            // Передаем оба репозитория во ViewModel, чтобы она могла проверить наличие абонемента
            var sickLeaveViewModel = new SickLeaveViewModel(SelectedClient, sickLeaveRepository, membershipRepository);
            var sickLeaveWindow = new SickLeaveWindow(sickLeaveViewModel);

            sickLeaveWindow.ShowDialog();

            LoadClients();
        }

        private bool CanEditOrDelete(object parameter) => SelectedClient != null;
    }
}