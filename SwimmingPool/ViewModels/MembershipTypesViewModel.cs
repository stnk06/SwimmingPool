using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SwimmingPool.Views;

namespace SwimmingPool.ViewModels
{
    public class MembershipTypesViewModel : BaseViewModel
    {
        private readonly MembershipTypeRepository _repository;
        public ObservableCollection<MembershipType> MembershipTypes { get; set; }
        private MembershipType _selectedMembershipType;
        public MembershipType SelectedMembershipType
        {
            get => _selectedMembershipType;
            set { _selectedMembershipType = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExportExcelCommand { get; }

        public MembershipTypesViewModel()
        {
            _repository = new MembershipTypeRepository();
            MembershipTypes = new ObservableCollection<MembershipType>();
            LoadData();
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanExecute);
            DeleteCommand = new RelayCommand(Delete, CanExecute);
            ExportExcelCommand = new RelayCommand(ExportExcel);
        }

        private void LoadData()
        {
            MembershipTypes.Clear();
            var items = _repository.GetAllMembershipTypes();
            foreach (var item in items)
            {
                MembershipTypes.Add(item);
            }
        }

        private void ExportExcel(object obj)
        {
            var mapping = new Dictionary<string, string>
            {
                { "TypeName", "Название абонемента" },
                { "Description", "Описание" },
                { "Price", "Цена (руб)" },
                { "DurationInDays", "Длительность (дней)" }
            };
            ExcelExportService.ExportToExcel(MembershipTypes.ToList(), mapping, "ТипыАбонементов");
        }

        private void Add(object obj)
        {
            var newType = new MembershipType();
            var window = new MembershipTypeWindow(newType, "Добавление типа абонемента");

            if (window.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(newType.TypeName))
                {
                    MessageBox.Show("Поле 'Название' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (newType.Price <= 0)
                {
                    MessageBox.Show("Цена должна быть больше нуля.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (newType.DurationInDays <= 0)
                {
                    MessageBox.Show("Длительность должна быть больше нуля.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _repository.AddMembershipType(window.MembershipType);
                LoadData();
            }
        }

        private void Edit(object obj)
        {
            if (SelectedMembershipType != null)
            {
                var typeCopy = new MembershipType
                {
                    MembershipTypeId = SelectedMembershipType.MembershipTypeId,
                    TypeName = SelectedMembershipType.TypeName,
                    Description = SelectedMembershipType.Description,
                    Price = SelectedMembershipType.Price,
                    DurationInDays = SelectedMembershipType.DurationInDays
                };
                var window = new MembershipTypeWindow(typeCopy, "Редактирование типа абонемента");

                if (window.ShowDialog() == true)
                {
                    if (string.IsNullOrWhiteSpace(typeCopy.TypeName))
                    {
                        MessageBox.Show("Поле 'Название' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (typeCopy.Price <= 0)
                    {
                        MessageBox.Show("Цена должна быть больше нуля.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (typeCopy.DurationInDays <= 0)
                    {
                        MessageBox.Show("Длительность должна быть больше нуля.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _repository.UpdateMembershipType(window.MembershipType);
                    LoadData();
                }
            }
        }

        private void Delete(object obj)
        {
            if (SelectedMembershipType != null)
            {
                if (MessageBox.Show($"Удалить тип абонемента '{SelectedMembershipType.TypeName}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _repository.DeleteMembershipType(SelectedMembershipType.MembershipTypeId);
                    LoadData();
                }
            }
        }

        private bool CanExecute(object obj) => SelectedMembershipType != null;
    }
}