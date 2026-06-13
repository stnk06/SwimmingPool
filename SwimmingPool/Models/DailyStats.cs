using SwimmingPool.ViewModels;

namespace SwimmingPool.Models
{
    public class DailyStats : BaseViewModel
    {
        private int _newMembershipsToday;
        public int NewMembershipsToday
        {
            get => _newMembershipsToday;
            set { _newMembershipsToday = value; OnPropertyChanged(); }
        }

        private decimal _revenueToday;
        public decimal RevenueToday
        {
            get => _revenueToday;
            set { _revenueToday = value; OnPropertyChanged(); }
        }

    }
}
