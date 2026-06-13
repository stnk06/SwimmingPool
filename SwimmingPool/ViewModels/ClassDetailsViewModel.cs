using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SwimmingPool.ViewModels
{
    public class ClassDetailsViewModel : BaseViewModel
    {
        private readonly ClassRepository _classRepository;
        private readonly ClassRegistrationRepository _registrationRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ActivityTypeRepository _activityTypeRepository;
        private readonly TrainerRepository _trainerRepository;
        private readonly PoolRepository _poolRepository;

        public Class CurrentClass { get; }

        public List<ActivityType> ActivityTypes { get; }
        public List<Trainer> Trainers { get; }
        public List<Pool> Pools { get; }

        public ObservableCollection<Client> RegisteredClients { get; }
        private ObservableCollection<Client> _availableClientsSource;
        public ICollectionView AvailableClientsView { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                AvailableClientsView.Refresh();
            }
        }

        public string RegisteredCountText => $"Записано: {RegisteredClients.Count} / {CurrentClass.MaxParticipants}";

        private Client _selectedAvailableClient;
        public Client SelectedAvailableClient
        {
            get => _selectedAvailableClient;
            set { _selectedAvailableClient = value; OnPropertyChanged(); }
        }

        private Client _selectedRegisteredClient;
        public Client SelectedRegisteredClient
        {
            get => _selectedRegisteredClient;
            set { _selectedRegisteredClient = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand UnregisterCommand { get; }
        public ICommand SaveClassCommand { get; }

        public Action CloseAction { get; set; }

        public ClassDetailsViewModel(Class selectedClass)
        {
            _classRepository = new ClassRepository();
            _registrationRepository = new ClassRegistrationRepository();
            _clientRepository = new ClientRepository();
            _activityTypeRepository = new ActivityTypeRepository();
            _trainerRepository = new TrainerRepository();
            _poolRepository = new PoolRepository();

            CurrentClass = new Class
            {
                ClassId = selectedClass.ClassId,
                ActivityTypeId = selectedClass.ActivityTypeId,
                TrainerId = selectedClass.TrainerId,
                PoolId = selectedClass.PoolId,
                StartTime = selectedClass.StartTime,
                EndTime = selectedClass.EndTime,
                MaxParticipants = selectedClass.MaxParticipants
            };

            CurrentClass.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Class.MaxParticipants))
                {
                    OnPropertyChanged(nameof(RegisteredCountText));
                }
            };

            ActivityTypes = _activityTypeRepository.GetAllActivityTypes();
            Trainers = _trainerRepository.GetAllTrainers();
            Pools = _poolRepository.GetAllPools();

            RegisteredClients = new ObservableCollection<Client>();
            _availableClientsSource = new ObservableCollection<Client>();

            AvailableClientsView = CollectionViewSource.GetDefaultView(_availableClientsSource);
            AvailableClientsView.Filter = FilterAvailableClients;

            RegisterCommand = new RelayCommand(RegisterClient, CanRegisterClient);
            UnregisterCommand = new RelayCommand(UnregisterClient, CanUnregisterClient);
            SaveClassCommand = new RelayCommand(SaveClass, CanSaveClass);

            LoadClients();
        }

        private void LoadClients()
        {
            var registered = _registrationRepository.GetRegisteredClientsForClass(CurrentClass.ClassId);
            RegisteredClients.Clear();
            foreach (var client in registered)
            {
                RegisteredClients.Add(client);
            }

            var allClients = _clientRepository.GetAllClients();
            _availableClientsSource.Clear();

            var registeredIds = new HashSet<int>(registered.Select(r => r.ClientId));
            foreach (var client in allClients.Where(c => !registeredIds.Contains(c.ClientId)))
            {
                _availableClientsSource.Add(client);
            }

            OnPropertyChanged(nameof(RegisteredCountText));
            AvailableClientsView.Refresh();
        }

        private bool FilterAvailableClients(object obj)
        {
            if (obj is Client client)
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return true;
                var search = SearchText.ToLower();
                return client.FullName.ToLower().Contains(search) ||
                       (client.PhoneNumber != null && client.PhoneNumber.Contains(search));
            }
            return false;
        }

        private bool CanRegisterClient(object obj)
        {
            return SelectedAvailableClient != null && RegisteredClients.Count < CurrentClass.MaxParticipants;
        }

        private void RegisterClient(object obj)
        {
            if (SelectedAvailableClient == null) return;

            try
            {
                _registrationRepository.RegisterClientToClass(CurrentClass.ClassId, SelectedAvailableClient.ClientId);

                RegisteredClients.Add(SelectedAvailableClient);
                _availableClientsSource.Remove(SelectedAvailableClient);

                OnPropertyChanged(nameof(RegisteredCountText));
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanUnregisterClient(object obj)
        {
            return SelectedRegisteredClient != null;
        }

        private void UnregisterClient(object obj)
        {
            if (SelectedRegisteredClient == null) return;

            try
            {
                _registrationRepository.UnregisterClientFromClass(CurrentClass.ClassId, SelectedRegisteredClient.ClientId);

                _availableClientsSource.Add(SelectedRegisteredClient);
                RegisteredClients.Remove(SelectedRegisteredClient);

                OnPropertyChanged(nameof(RegisteredCountText));
                AvailableClientsView.Refresh();
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveClass(object obj)
        {
            return CurrentClass.ActivityTypeId > 0 &&
                   CurrentClass.TrainerId > 0 &&
                   CurrentClass.PoolId > 0 &&
                   CurrentClass.MaxParticipants > 0 &&
                   CurrentClass.EndTime > CurrentClass.StartTime;
        }

        private void SaveClass(object obj)
        {
            try
            {
                _classRepository.UpdateClass(CurrentClass);
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении параметров занятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}