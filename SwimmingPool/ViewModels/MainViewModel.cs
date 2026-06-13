using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SwimmingPool.ViewModels
{
    public class NavItem
    {
        public string Label { get; set; }
        public string Icon { get; set; }
        public object ViewModel { get; set; }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private object _currentViewModel;
        private NavItem _selectedNavItem;
        private User _currentUser;

        public List<NavItem> NavigationItems { get; set; }

        public string CurrentUserName => $"Пользователь: {_currentUser?.Username} ({_currentUser?.Role})";

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged("CurrentViewModel"); }
        }

        public NavItem SelectedNavItem
        {
            get => _selectedNavItem;
            set
            {
                if (value != null && value.ViewModel != null)
                {
                    _selectedNavItem = value;
                    CurrentViewModel = value.ViewModel;
                    OnPropertyChanged("SelectedNavItem");
                }
            }
        }

        public ICommand LogoutCommand { get; }

        public MainViewModel(User user)
        {
            _currentUser = user;
            _authService = new AuthService();
            LogoutCommand = new RelayCommand(Logout);

            var dashboardVM = new DashboardViewModel();
            var clientsVM = new ClientsViewModel();
            var membershipsVM = new MembershipsViewModel();
            var classesVM = new ClassesViewModel();
            var trainersVM = new TrainersViewModel();

            NavigationItems = new List<NavItem>();

            NavigationItems.Add(new NavItem { Label = "Главная", Icon = "Home", ViewModel = dashboardVM });
            NavigationItems.Add(new NavItem { Label = "ТАБЛИЦЫ", Icon = "Grid", ViewModel = null });
            NavigationItems.Add(new NavItem { Label = "Клиенты", Icon = "AccountGroupOutline", ViewModel = clientsVM });
            NavigationItems.Add(new NavItem { Label = "Абонементы", Icon = "CreditCardOutline", ViewModel = membershipsVM });
            NavigationItems.Add(new NavItem { Label = "Расписание", Icon = "CalendarClock", ViewModel = classesVM });

            NavigationItems.Add(new NavItem { Label = "СПРАВОЧНИКИ", Icon = "BookOpenPageVariantOutline", ViewModel = null });
            NavigationItems.Add(new NavItem { Label = "Тренеры", Icon = "AccountStarOutline", ViewModel = trainersVM });

            // Доступ только для Администратора
            if (_currentUser.IsAdmin)
            {
                var membershipTypesVM = new MembershipTypesViewModel();
                var activityTypesVM = new ActivityTypesViewModel();
                var poolsVM = new PoolsViewModel();
                var analyticsVM = new AdvancedReportsViewModel();
                var usersVM = new UsersViewModel(_currentUser);

                NavigationItems.Add(new NavItem { Label = "Типы абонементов", Icon = "TicketPercentOutline", ViewModel = membershipTypesVM });
                NavigationItems.Add(new NavItem { Label = "Типы занятий", Icon = "ClipboardListOutline", ViewModel = activityTypesVM });
                NavigationItems.Add(new NavItem { Label = "Бассейны", Icon = "Waves", ViewModel = poolsVM });
                NavigationItems.Add(new NavItem { Label = "АДМИНИСТРАТОР", Icon = "Security", ViewModel = null });
                NavigationItems.Add(new NavItem { Label = "Аналитика и Отчеты", Icon = "ChartBar", ViewModel = analyticsVM });
                NavigationItems.Add(new NavItem { Label = "Пользователи", Icon = "AccountCog", ViewModel = usersVM });
            }

            SelectedNavItem = NavigationItems.First();
        }

        private void Logout(object obj)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _authService.Logout();

                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}