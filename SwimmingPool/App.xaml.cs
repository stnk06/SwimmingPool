using SwimmingPool.Services;
using SwimmingPool.ViewModels;
using SwimmingPool.Views;
using System.Windows;

namespace SwimmingPool
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var authService = new AuthService();
            var user = authService.TryAutoLogin();

            if (user != null)
            {
                var mainWindow = new MainWindow();
                mainWindow.DataContext = new MainViewModel(user);
                mainWindow.Show();
            }
            else
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
        }
    }
}