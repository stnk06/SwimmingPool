using SwimmingPool.Models;
using SwimmingPool.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace SwimmingPool.Models
{
    public class CalendarDay : BaseViewModel
    {
        public DateTime Date { get; set; }
        public int DayNumber => Date.Day;
        public bool IsTargetMonth { get; set; }
        public ObservableCollection<Class> Classes { get; set; } = new ObservableCollection<Class>();
    }
}

