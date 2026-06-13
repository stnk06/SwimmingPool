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
    public class PoolsViewModel : BaseViewModel
    {
        private readonly PoolRepository _repository;
        public ObservableCollection<Pool> Pools { get; set; }
        private Pool _selectedPool;
        public Pool SelectedPool
        {
            get => _selectedPool;
            set { _selectedPool = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ExportExcelCommand { get; }

        public PoolsViewModel()
        {
            _repository = new PoolRepository();
            Pools = new ObservableCollection<Pool>();
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
                Pools.Clear();
                var items = _repository.GetAllPools();
                foreach (var item in items)
                {
                    Pools.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных о бассейнах: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportExcel(object obj)
        {
            var mapping = new Dictionary<string, string>
            {
                { "PoolName", "Название" },
                { "DepthMeters", "Глубина (м)" },
                { "WorkingHours", "Часы работы" }
            };
            ExcelExportService.ExportToExcel(Pools.ToList(), mapping, "Бассейны");
        }

        private void Add(object obj)
        {
            var newPool = new Pool();
            var window = new PoolWindow(newPool, "Добавление бассейна");

            if (window.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(newPool.PoolName))
                {
                    MessageBox.Show("Поле 'Название' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (newPool.DepthMeters <= 0)
                {
                    MessageBox.Show("Глубина должна быть больше нуля.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _repository.AddPool(window.Pool);
                LoadData();
            }
        }

        private void Edit(object obj)
        {
            if (SelectedPool != null)
            {
                var poolCopy = new Pool
                {
                    PoolId = SelectedPool.PoolId,
                    PoolName = SelectedPool.PoolName,
                    DepthMeters = SelectedPool.DepthMeters,
                    WorkingHours = SelectedPool.WorkingHours
                };
                var window = new PoolWindow(poolCopy, "Редактирование бассейна");

                if (window.ShowDialog() == true)
                {
                    if (string.IsNullOrWhiteSpace(poolCopy.PoolName))
                    {
                        MessageBox.Show("Поле 'Название' не может быть пустым.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (poolCopy.DepthMeters <= 0)
                    {
                        MessageBox.Show("Глубина должна быть больше нуля.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    _repository.UpdatePool(window.Pool);
                    LoadData();
                }
            }
        }

        private void Delete(object obj)
        {
            if (SelectedPool != null)
            {
                if (MessageBox.Show($"Удалить бассейн '{SelectedPool.PoolName}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _repository.DeletePool(SelectedPool.PoolId);
                    LoadData();
                }
            }
        }

        private bool CanExecute(object obj) => SelectedPool != null;
    }
}