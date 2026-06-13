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
    public class ReportsViewModel : BaseViewModel
    {
        private readonly ReportsRepository _repository = new ReportsRepository();

        private int _totalClients;
        public int TotalClients { get => _totalClients; set { _totalClients = value; OnPropertyChanged(); } }

        private int _activeMemberships;
        public int ActiveMemberships { get => _activeMemberships; set { _activeMemberships = value; OnPropertyChanged(); } }

        private decimal _totalPeriodRevenue;
        public decimal TotalPeriodRevenue { get => _totalPeriodRevenue; set { _totalPeriodRevenue = value; OnPropertyChanged(); } }

        public ObservableCollection<RevenueReportItem> RevenueReportItems { get; set; }

        private DateTime _startDate = DateTime.Now.AddMonths(-1);
        public DateTime StartDate { get => _startDate; set { _startDate = value; OnPropertyChanged(); } }

        private DateTime _endDate = DateTime.Now;
        public DateTime EndDate { get => _endDate; set { _endDate = value; OnPropertyChanged(); } }

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportReportCommand { get; }

        public ReportsViewModel()
        {
            RevenueReportItems = new ObservableCollection<RevenueReportItem>();
            GenerateReportCommand = new RelayCommand(GenerateReport);
            ExportReportCommand = new RelayCommand(ExportReport);
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            try
            {
                TotalClients = _repository.GetTotalClientsCount();
                ActiveMemberships = _repository.GetActiveMembershipsCount();
                GenerateReport(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateReport(object obj)
        {
            try
            {
                var items = _repository.GetRevenueByPeriod(StartDate, EndDate);
                RevenueReportItems.Clear();
                foreach (var item in items)
                {
                    RevenueReportItems.Add(item);
                }
                TotalPeriodRevenue = RevenueReportItems.Sum(item => item.TotalRevenue);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportReport(object obj)
        {
            if (RevenueReportItems.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта. Сначала сформируйте отчет.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PDF файл (*.pdf)|*.pdf",
                FileName = $"Отчет_Выручка_{StartDate:dd.MM}-{EndDate:dd.MM}.pdf"
            };

            if (sfd.ShowDialog() == true)
            {
                AdvancedPdfService.ExportStandardRevenueReport(RevenueReportItems.ToList(), StartDate, EndDate, sfd.FileName);
            }
        }
    }
}