using SwimmingPool.Infrastructure;
using SwimmingPool.Models;
using SwimmingPool.Services;
using SwimmingPool.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SwimmingPool.ViewModels
{
    public class UsersViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly User _currentUser;

        public ObservableCollection<User> Users { get; set; }

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set { _selectedUser = value; OnPropertyChanged(); }
        }

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public UsersViewModel(User currentUser)
        {
            _currentUser = currentUser;
            _authService = new AuthService();
            Users = new ObservableCollection<User>();

            AddUserCommand = new RelayCommand(AddUser);
            EditUserCommand = new RelayCommand(EditUser, CanExecute);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanExecute);

            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                Users.Clear();
                var items = _authService.GetAllUsers();
                foreach (var item in items) Users.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUser(object obj)
        {
            var window = new UserWindow(); // Режим создания
            if (window.ShowDialog() == true)
            {
                try
                {
                    _authService.AddUser(window.Username, window.Password, window.Role);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditUser(object obj)
        {
            if (SelectedUser == null) return;

            var window = new UserWindow(SelectedUser); 
            if (window.ShowDialog() == true)
            {
                try
                {
                    _authService.UpdateUser(SelectedUser.UserId, window.Username, window.Role, window.Password);
                    LoadUsers();
                    MessageBox.Show("Данные пользователя успешно обновлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteUser(object obj)
        {
            if (SelectedUser == null) return;

            if (MessageBox.Show($"Удалить пользователя '{SelectedUser.Username}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _authService.DeleteUser(SelectedUser.UserId, _currentUser.UserId);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private bool CanExecute(object obj) => SelectedUser != null;
    }
}