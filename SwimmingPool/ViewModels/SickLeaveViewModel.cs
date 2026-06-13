using System;
using System.Windows.Input;
using SwimmingPool.Models;
using SwimmingPool.Services;
using SwimmingPool.Infrastructure;

namespace SwimmingPool.ViewModels
{
    public class SickLeaveViewModel : BaseViewModel
    {
        private readonly ISickLeaveRepository _sickLeaveRepository;
        private readonly MembershipRepository _membershipRepository;
        private readonly Client _client;
        private readonly Membership _activeMembership;

        private DateTime _startDate;
        private DateTime _endDate;
        private string _notes;

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                    OnPropertyChanged(nameof(DaysCount));
                    // Обновляем команду при изменении дат
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                    OnPropertyChanged(nameof(DaysCount));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        public int DaysCount => EndDate >= StartDate ? (int)(EndDate.Date - StartDate.Date).TotalDays + 1 : 0;

        // --- НОВЫЕ СВОЙСТВА ДЛЯ ПРОВЕРКИ АБОНЕМЕНТА ---
        public bool HasActiveMembership => _activeMembership != null;

        public string ActiveMembershipInfo => HasActiveMembership
            ? $"Текущий абонемент: {_activeMembership.MembershipTypeName} (действует до {_activeMembership.ExpiryDate:dd.MM.yyyy})"
            : "ВНИМАНИЕ: У клиента нет активного абонемента! Заморозка невозможна.";


        public ICommand SaveCommand { get; }
        public Action CloseAction { get; set; }

        public SickLeaveViewModel(Client client, ISickLeaveRepository sickLeaveRepository, MembershipRepository membershipRepository)
        {
            _client = client;
            _sickLeaveRepository = sickLeaveRepository;
            _membershipRepository = membershipRepository;

            // Проверяем наличие абонемента при инициализации
            _activeMembership = _membershipRepository.GetActiveMembershipForClient(_client.ClientId);

            _startDate = DateTime.Today.AddDays(-7); // По умолчанию логично, что справку приносят после болезни
            _endDate = DateTime.Today;

            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
        }

        private bool CanExecuteSave(object obj)
        {
            // Блокируем кнопку сохранения, если абонемента нет, либо даты некорректны
            return HasActiveMembership && EndDate >= StartDate && DaysCount > 0 && _client != null && _client.ClientId > 0;
        }

        private void ExecuteSave(object obj)
        {
            var sickLeave = new SickLeave
            {
                ClientId = _client.ClientId,
                StartDate = StartDate,
                EndDate = EndDate,
                Notes = Notes,
                IsProcessed = true
            };

            _sickLeaveRepository.AddSickLeave(sickLeave);
            CloseAction?.Invoke();
        }
    }
}