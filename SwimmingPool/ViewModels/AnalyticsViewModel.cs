using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using SwimmingPool.Infrastructure;
using SwimmingPool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Xceed.Document.NET;

namespace SwimmingPool.ViewModels
{
    public class AnalyticsViewModel : BaseViewModel
    {
        private readonly ReportsRepository _repository;

        public SeriesCollection RevenueSeries { get; set; }
        public string[] RevenueLabels { get; set; }
        public Func<double, string> CurrencyFormatter { get; set; }

        public SeriesCollection PopularitySeries { get; set; }

        public ICommand ExportReportCommand { get; }

        public AnalyticsViewModel()
        {
            _repository = new ReportsRepository();
            CurrencyFormatter = value => value.ToString("C");

            LoadRevenueData();
            LoadPopularityData();

            ExportReportCommand = new RelayCommand(ExportReport);
        }

        private void LoadRevenueData()
        {
            var data = _repository.GetRevenueByMonth(6); // Последние 6 месяцев

            RevenueSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Выручка",
                    Values = new ChartValues<decimal>(data.Values),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10
                }
            };
            RevenueLabels = data.Keys.ToArray();
            OnPropertyChanged(nameof(RevenueSeries));
            OnPropertyChanged(nameof(RevenueLabels));
        }

        private void LoadPopularityData()
        {
            var data = _repository.GetMembershipTypePopularity();
            PopularitySeries = new SeriesCollection();

            foreach (var item in data)
            {
                PopularitySeries.Add(new PieSeries
                {
                    Title = item.Key,
                    Values = new ChartValues<int> { item.Value },
                    DataLabels = true
                });
            }
            OnPropertyChanged(nameof(PopularitySeries));
        }

        private void ExportReport(object obj)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"Финансовый_Отчет_{DateTime.Now:dd-MM-yyyy}.pdf",
                Title = "Сохранить отчет"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var revData = _repository.GetRevenueByMonth(12); // За год

                PdfExportService.GenerateFinancialReport(revData, saveFileDialog.FileName);
            }
        }
    }
}