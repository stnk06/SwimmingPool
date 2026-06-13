using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SwimmingPool.ViewModels
{
    public class AdvancedReportsViewModel : BaseViewModel
    {
        private readonly AdvancedReportsRepository _repository;

        // Данные
        public ObservableCollection<TrainerPerformanceDTO> TrainerStats { get; set; }
        public ObservableCollection<MonthlyRevenueDTO> FinancialStats { get; set; }
        public ObservableCollection<PoolOccupancyDTO> PoolStats { get; set; }
        public ObservableCollection<AttendanceTrendDTO> AttendanceStats { get; set; }

        // Графики
        public SeriesCollection RevenueSeries { get; set; }
        public string[] RevenueLabels { get; set; }
        public Func<double, string> CurrencyFormatter { get; set; }

        public SeriesCollection TrainerWorkloadSeries { get; set; }
        public string[] TrainerLabels { get; set; }

        public SeriesCollection AttendanceSeries { get; set; }
        public string[] AttendanceLabels { get; set; }

        // Команды
        public ICommand RefreshCommand { get; }
        public ICommand ExportFinancialCommand { get; }
        public ICommand ExportTrainerCommand { get; }
        public ICommand ExportPoolCommand { get; }

        public AdvancedReportsViewModel()
        {
            _repository = new AdvancedReportsRepository();

            TrainerStats = new ObservableCollection<TrainerPerformanceDTO>();
            FinancialStats = new ObservableCollection<MonthlyRevenueDTO>();
            PoolStats = new ObservableCollection<PoolOccupancyDTO>();
            AttendanceStats = new ObservableCollection<AttendanceTrendDTO>();

            CurrencyFormatter = value => value.ToString("C");

            RefreshCommand = new RelayCommand(LoadData);

            // Команды с обработкой ошибок внутри
            ExportFinancialCommand = new RelayCommand(obj => ExportFinancial());
            ExportTrainerCommand = new RelayCommand(obj => ExportTrainer());
            ExportPoolCommand = new RelayCommand(obj => ExportPool());

            LoadData(null);
        }

        private void LoadData(object obj)
        {
            try
            {
                var trainers = _repository.GetTrainerPerformance();
                var finances = _repository.GetMonthlyRevenueStats();
                var pools = _repository.GetPoolOccupancy();
                var attendance = _repository.GetAttendanceTrends();

                TrainerStats.Clear();
                foreach (var t in trainers) TrainerStats.Add(t);

                FinancialStats.Clear();
                foreach (var f in finances) FinancialStats.Add(f);

                PoolStats.Clear();
                foreach (var p in pools) PoolStats.Add(p);

                AttendanceStats.Clear();
                foreach (var a in attendance) AttendanceStats.Add(a);

                BuildRevenueChart(finances);
                BuildTrainerChart(trainers);
                BuildAttendanceChart(attendance);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке аналитики: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportFinancial()
        {
            try
            {
                if (FinancialStats == null || !FinancialStats.Any())
                {
                    MessageBox.Show("Нет данных для экспорта. Проверьте подключение к базе или нажмите 'Обновить'.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF файл|*.pdf", FileName = $"Финансы_{DateTime.Now:yyyyMMdd}.pdf" };
                if (sfd.ShowDialog() == true)
                {
                    AdvancedPdfService.ExportFinancialReport(FinancialStats.ToList(), sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось начать экспорт: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportTrainer()
        {
            try
            {
                if (TrainerStats == null || !TrainerStats.Any())
                {
                    MessageBox.Show("Нет данных для экспорта.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF файл|*.pdf", FileName = $"Тренеры_{DateTime.Now:yyyyMMdd}.pdf" };
                if (sfd.ShowDialog() == true)
                {
                    AdvancedPdfService.ExportTrainerReport(TrainerStats.ToList(), sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось начать экспорт: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportPool()
        {
            try
            {
                if (PoolStats == null || !PoolStats.Any())
                {
                    MessageBox.Show("Нет данных для экспорта.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog { Filter = "PDF файл|*.pdf", FileName = $"Бассейны_{DateTime.Now:yyyyMMdd}.pdf" };
                if (sfd.ShowDialog() == true)
                {
                    AdvancedPdfService.ExportPoolReport(PoolStats.ToList(), sfd.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось начать экспорт: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuildRevenueChart(System.Collections.Generic.List<MonthlyRevenueDTO> data)
        {
            RevenueSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Выручка",
                    Values = new ChartValues<decimal>(data.Select(x => x.TotalRevenue)),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    StrokeThickness = 3
                }
            };
            RevenueLabels = data.Select(x => x.MonthYear).ToArray();
            OnPropertyChanged(nameof(RevenueSeries));
            OnPropertyChanged(nameof(RevenueLabels));
        }

        private void BuildTrainerChart(System.Collections.Generic.List<TrainerPerformanceDTO> data)
        {
            TrainerWorkloadSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Часов отработано",
                    Values = new ChartValues<double>(data.Select(x => x.TotalHours)),
                    DataLabels = true
                }
            };
            TrainerLabels = data.Select(x => x.TrainerName).ToArray();
            OnPropertyChanged(nameof(TrainerWorkloadSeries));
            OnPropertyChanged(nameof(TrainerLabels));
        }

        private void BuildAttendanceChart(System.Collections.Generic.List<AttendanceTrendDTO> data)
        {
            AttendanceSeries = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "Посетителей",
                    Values = new ChartValues<int>(data.Select(x => x.TotalVisits)),
                    DataLabels = true
                }
            };
            AttendanceLabels = data.Select(x => x.DayOfWeek).ToArray();
            OnPropertyChanged(nameof(AttendanceSeries));
            OnPropertyChanged(nameof(AttendanceLabels));
        }
    }
}