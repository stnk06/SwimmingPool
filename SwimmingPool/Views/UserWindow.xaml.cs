using MahApps.Metro.Controls;
using SwimmingPool.Models;
using System.Windows;
using System.Windows.Controls;

namespace SwimmingPool.Views
{
    public partial class UserWindow : MetroWindow
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Role { get; private set; }

        private bool _isEditMode;

        public UserWindow(User userToEdit = null)
        {
            InitializeComponent();
            _isEditMode = userToEdit != null;

            if (_isEditMode)
            {
                Title = "РЕДАКТИРОВАНИЕ ПОЛЬЗОВАТЕЛЯ";
                UsernameBox.Text = userToEdit.Username;

                // Устанавливаем роль в ComboBox
                foreach (ComboBoxItem item in RoleBox.Items)
                {
                    if (item.Content.ToString() == userToEdit.Role)
                    {
                        RoleBox.SelectedItem = item;
                        break;
                    }
                }

                // Меняем подсказку для пароля
                TextBoxHelper.SetWatermark(PasswordBox, "Оставьте пустым, чтобы не менять");
            }
            else
            {
                Title = "НОВЫЙ ПОЛЬЗОВАТЕЛЬ";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Username = UsernameBox.Text;
            Password = PasswordBox.Password;
            Role = RoleBox.Text;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Role))
            {
                MessageBox.Show("Логин и роль обязательны для заполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_isEditMode && string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Для нового пользователя необходимо задать пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }
    }
}