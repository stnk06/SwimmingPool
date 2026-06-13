using Microsoft.Win32;
using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SwimmingPool.Views;

namespace SwimmingPool.ViewModels
{
    public class ClassesViewModel : BaseViewModel
    {
        private readonly ClassRepository _classRepository = new ClassRepository();
        private List<Class> _allClasses;

        public ObservableCollection<CalendarDay> Days { get; set; }
        public ObservableCollection<string> DayNames { get; set; }

        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get => _currentDate;
            set { _currentDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentMonthYearText)); GenerateCalendar(); }
        }

        public string CurrentMonthYearText => CurrentDate.ToString("MMMM yyyy", new CultureInfo("ru-RU")).ToUpper();

        public ICommand NextMonthCommand { get; }
        public ICommand PrevMonthCommand { get; }
        public ICommand GoToTodayCommand { get; }
        public ICommand AddClassCommand { get; }
        public ICommand EditClassCommand { get; }
        public ICommand DeleteClassCommand { get; }
        public ICommand ExportToPdfCommand { get; }

        public ClassesViewModel()
        {
            Days = new ObservableCollection<CalendarDay>();
            DayNames = new ObservableCollection<string> { "ПОНЕДЕЛЬНИК", "ВТОРНИК", "СРЕДА", "ЧЕТВЕРГ", "ПЯТНИЦА", "СУББОТА", "ВОСКРЕСЕНЬЕ" };
            _currentDate = DateTime.Today;

            NextMonthCommand = new RelayCommand(p => CurrentDate = CurrentDate.AddMonths(1));
            PrevMonthCommand = new RelayCommand(p => CurrentDate = CurrentDate.AddMonths(-1));
            GoToTodayCommand = new RelayCommand(p => CurrentDate = DateTime.Today);

            AddClassCommand = new RelayCommand(AddClass);
            EditClassCommand = new RelayCommand(EditClass);
            DeleteClassCommand = new RelayCommand(DeleteClass);
            ExportToPdfCommand = new RelayCommand(ExportToPdf);

            LoadData();
        }

        private void ExportToPdf(object obj)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"Расписание_{CurrentDate:MMMM_yyyy}.pdf",
                Title = "Сохранить расписание как PDF"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                PdfExportService.GenerateSchedule(Days, saveFileDialog.FileName, CurrentMonthYearText);
            }
        }

        private void LoadData()
        {
            _allClasses = _classRepository.GetAllClasses();
            GenerateCalendar();
        }

        private void GenerateCalendar()
        {
            Days.Clear();
            var firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(CurrentDate.Year, CurrentDate.Month);

            int startOffset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

            for (int i = 0; i < startOffset; i++)
            {
                var day = firstDayOfMonth.AddDays(-startOffset + i);
                Days.Add(new CalendarDay { Date = day, IsTargetMonth = false });
            }

            for (int i = 0; i < daysInMonth; i++)
            {
                var day = firstDayOfMonth.AddDays(i);
                Days.Add(new CalendarDay { Date = day, IsTargetMonth = true });
            }

            int remainingCells = 42 - Days.Count;
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            for (int i = 1; i <= remainingCells; i++)
            {
                var day = lastDayOfMonth.AddDays(i);
                Days.Add(new CalendarDay { Date = day, IsTargetMonth = false });
            }

            DistributeClasses();
        }

        private void DistributeClasses()
        {
            if (_allClasses == null) return;

            foreach (var day in Days)
            {
                day.Classes.Clear();
                var classesForDay = _allClasses.Where(c => c.StartTime.Date == day.Date.Date).OrderBy(c => c.StartTime);
                foreach (var cls in classesForDay)
                {
                    day.Classes.Add(cls);
                }
            }
        }

        private void AddClass(object parameter)
        {
            var newClass = new Class();
            if (parameter is CalendarDay day)
            {
                newClass.StartTime = day.Date.Date.AddHours(9);
                newClass.EndTime = day.Date.Date.AddHours(10);
            }

            var window = new ClassWindow(newClass, "Добавление нового занятия");
            if (window.ShowDialog() == true)
            {
                if (window.Class.ActivityTypeId <= 0 || window.Class.TrainerId <= 0 || window.Class.PoolId <= 0)
                {
                    MessageBox.Show("Все поля (Тип занятия, Тренер, Бассейн) должны быть выбраны.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _classRepository.AddClass(window.Class);
                LoadData();
            }
        }

        private void EditClass(object parameter)
        {
            if (!(parameter is Class selectedClass)) return;

            var detailsViewModel = new ClassDetailsViewModel(selectedClass);
            var window = new ClassDetailsWindow(detailsViewModel);

            window.ShowDialog();

            LoadData();
        }

        private void DeleteClass(object parameter)
        {
            if (!(parameter is Class selectedClass)) return;

            if (MessageBox.Show($"Удалить занятие '{selectedClass.ActivityTypeName}' в {selectedClass.StartTime:g}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _classRepository.DeleteClass(selectedClass.ClassId);
                LoadData();
            }
        }
    }
}