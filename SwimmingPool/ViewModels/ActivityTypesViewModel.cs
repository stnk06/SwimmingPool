using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SwimmingPool.Views;

namespace SwimmingPool.ViewModels
{
    public class ActivityTypesViewModel : BaseViewModel
    {
        private readonly ActivityTypeRepository _repository = new ActivityTypeRepository();
        public ObservableCollection<ActivityType> ActivityTypes { get; set; }
        private ActivityType _selectedActivityType;
        public ActivityType SelectedActivityType
        {
            get => _selectedActivityType;
            set { _selectedActivityType = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExportExcelCommand { get; }

        public ActivityTypesViewModel()
        {
            ActivityTypes = new ObservableCollection<ActivityType>();
            LoadData();
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanExecute);
            DeleteCommand = new RelayCommand(Delete, CanExecute);
            ExportExcelCommand = new RelayCommand(ExportExcel);
        }

        private void LoadData()
        {
            try
            {
                ActivityTypes.Clear();
                var items = _repository.GetAllActivityTypes();
                foreach (var item in items)
                {
                    ActivityTypes.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке типов занятий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportExcel(object obj)
        {
            var mapping = new Dictionary<string, string>
            {
                { "TypeName", "Название занятия" },
                { "Description", "Описание" }
            };
            ExcelExportService.ExportToExcel(ActivityTypes.ToList(), mapping, "ТипыЗанятий");
        }

        private void Add(object obj)
        {
            var newType = new ActivityType();
            var window = new ActivityTypeWindow(newType, "Добавление типа занятия");
            if (window.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(newType.TypeName))
                {
                    MessageBox.Show("Поле 'Название' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _repository.AddActivityType(window.ActivityType);
                LoadData();
            }
        }

        private void Edit(object obj)
        {
            if (SelectedActivityType != null)
            {
                var typeCopy = new ActivityType
                {
                    ActivityTypeId = SelectedActivityType.ActivityTypeId,
                    TypeName = SelectedActivityType.TypeName,
                    Description = SelectedActivityType.Description
                };
                var window = new ActivityTypeWindow(typeCopy, "Редактирование типа занятия");

                if (window.ShowDialog() == true)
                {
                    if (string.IsNullOrWhiteSpace(typeCopy.TypeName))
                    {
                        MessageBox.Show("Поле 'Название' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    _repository.UpdateActivityType(window.ActivityType);
                    LoadData();
                }
            }
        }

        private void Delete(object obj)
        {
            if (SelectedActivityType != null)
            {
                if (MessageBox.Show($"Удалить тип занятия '{SelectedActivityType.TypeName}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _repository.DeleteActivityType(SelectedActivityType.ActivityTypeId);
                    LoadData();
                }
            }
        }

        private bool CanExecute(object obj) => SelectedActivityType != null;
    }
}