using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using SwimmingPool.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SwimmingPool.ViewModels
{
    public class MembershipsViewModel : BaseViewModel
    {
        private readonly MembershipRepository _membershipRepository;
        private readonly MembershipTypeRepository _membershipTypeRepository;

        private ObservableCollection<Membership> _membershipsSource;
        public ICollectionView MembershipsView { get; private set; }

        public ObservableCollection<MembershipType> MembershipTypes { get; set; }

        // Фильтры
        private MembershipType _selectedTypeFilter;
        public MembershipType SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set
            {
                _selectedTypeFilter = value;
                OnPropertyChanged();
                MembershipsView.Refresh();
            }
        }

        private DateTime? _dateFrom;
        public DateTime? DateFrom
        {
            get => _dateFrom;
            set { _dateFrom = value; OnPropertyChanged(); MembershipsView.Refresh(); }
        }

        private DateTime? _dateTo;
        public DateTime? DateTo
        {
            get => _dateTo;
            set { _dateTo = value; OnPropertyChanged(); MembershipsView.Refresh(); }
        }

        // Поиск (по клиенту)
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // Мы НЕ вызываем MembershipsView.Refresh() здесь, так как фильтрация строк не нужна,
                // только подсветка, которая работает через Binding в View.
                // Но если вы хотите, чтобы при вводе все же обновлялся View (например, для сортировки или чего-то еще),
                // можно оставить Refresh, но убрать логику из FilterMemberships.
                // В данном случае Refresh не обязателен для одной лишь подсветки, так как Binding во View сработает сам.
            }
        }

        private Membership _selectedMembership;
        public Membership SelectedMembership
        {
            get => _selectedMembership;
            set { _selectedMembership = value; OnPropertyChanged(); }
        }

        public ICommand AddMembershipCommand { get; }
        public ICommand EditMembershipCommand { get; }
        public ICommand DeleteMembershipCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public MembershipsViewModel()
        {
            _membershipRepository = new MembershipRepository();
            _membershipTypeRepository = new MembershipTypeRepository();

            _membershipsSource = new ObservableCollection<Membership>();
            MembershipsView = CollectionViewSource.GetDefaultView(_membershipsSource);
            MembershipsView.Filter = FilterMemberships;

            MembershipTypes = new ObservableCollection<MembershipType>(_membershipTypeRepository.GetAllMembershipTypes());

            LoadMemberships();

            AddMembershipCommand = new RelayCommand(AddMembership);
            EditMembershipCommand = new RelayCommand(EditMembership, CanExecute);
            DeleteMembershipCommand = new RelayCommand(DeleteMembership, CanExecute);
            ClearFilterCommand = new RelayCommand(ClearFilter);
        }

        private bool FilterMemberships(object obj)
        {
            if (obj is Membership m)
            {
                // 1. Фильтр по типу (оставляем скрытие)
                if (SelectedTypeFilter != null && m.MembershipTypeId != SelectedTypeFilter.MembershipTypeId)
                    return false;

                // 2. Фильтр по дате покупки (оставляем скрытие)
                if (DateFrom.HasValue && m.PurchaseDate < DateFrom.Value)
                    return false;
                if (DateTo.HasValue && m.PurchaseDate > DateTo.Value)
                    return false;

                // 3. Поиск по клиенту - УБРАНО СКРЫТИЕ
                // Теперь мы просто возвращаем true, а подсветка работает в View через TextBlockHighlighter

                return true;
            }
            return false;
        }

        private void ClearFilter(object obj)
        {
            SelectedTypeFilter = null;
            DateFrom = null;
            DateTo = null;
            SearchText = string.Empty;
            MembershipsView.Refresh(); // Обновляем, чтобы вернуть скрытые по дате/типу записи
        }

        private void LoadMemberships()
        {
            _membershipsSource.Clear();
            var items = _membershipRepository.GetAllMemberships();
            foreach (var item in items) _membershipsSource.Add(item);
            MembershipsView.Refresh();
        }

        private void AddMembership(object obj)
        {
            var newMembership = new Membership
            {
                PurchaseDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddDays(30)
            };

            var window = new MembershipWindow(newMembership, "ПРОДАЖА АБОНЕМЕНТА");

            if (window.ShowDialog() == true)
            {
                if (newMembership.ClientId <= 0 || newMembership.MembershipTypeId <= 0)
                {
                    MessageBox.Show("Пожалуйста, выберите клиента и тип абонемента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _membershipRepository.AddMembership(window.Membership);
                LoadMemberships();
            }
        }

        private void EditMembership(object obj)
        {
            if (SelectedMembership != null)
            {
                var membershipToEdit = new Membership
                {
                    MembershipId = SelectedMembership.MembershipId,
                    ClientId = SelectedMembership.ClientId,
                    MembershipTypeId = SelectedMembership.MembershipTypeId,
                    PurchaseDate = SelectedMembership.PurchaseDate,
                    ExpiryDate = SelectedMembership.ExpiryDate,
                    ClientFullName = SelectedMembership.ClientFullName,
                    MembershipTypeName = SelectedMembership.MembershipTypeName
                };

                var window = new MembershipWindow(membershipToEdit, "РЕДАКТИРОВАНИЕ");

                if (window.ShowDialog() == true)
                {
                    if (membershipToEdit.ClientId <= 0 || membershipToEdit.MembershipTypeId <= 0)
                    {
                        MessageBox.Show("Пожалуйста, выберите клиента и тип абонемента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _membershipRepository.UpdateMembership(window.Membership);
                    LoadMemberships();
                }
            }
        }

        private void DeleteMembership(object obj)
        {
            if (SelectedMembership != null)
            {
                if (MessageBox.Show($"Удалить абонемент клиента {SelectedMembership.ClientFullName}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _membershipRepository.DeleteMembership(SelectedMembership.MembershipId);
                    LoadMemberships();
                }
            }
        }

        private bool CanExecute(object obj) => SelectedMembership != null;
    }
}