using SwimmingPool.Infrastructure;
using SwimmingPool.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SwimmingPool.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private string _username;
        private bool _rememberMe;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set { _rememberMe = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(Login);
        }

        private void Login(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Введите логин и пароль";
                return;
            }

            var user = _authService.Login(Username, password, RememberMe);

            if (user != null)
            {
                var mainWindow = new MainWindow();
                var mainViewModel = new MainViewModel(user);
                mainWindow.DataContext = mainViewModel;
                mainWindow.Show();

                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = mainWindow;
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль";
            }
        }
    }
}