using SwimmingPool.Models;
using MahApps.Metro.Controls;
using SwimmingPool.Services;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.Globalization;

namespace SwimmingPool.Views
{
    public partial class ClientWindow : MetroWindow
    {
        public Client Client { get; private set; }
        public string Title { get; private set; }
        private readonly QrService _qrService;
        private bool _isUpdating;

        public ClientWindow(Client client, string title)
        {
            InitializeComponent();
            Client = client;
            Title = title;
            _qrService = new QrService();
            this.DataContext = this;
            this.Loaded += ClientWindow_Loaded;
        }

        private void ClientWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateQrCode();
        }

        private void GenerateQr_Click(object sender, RoutedEventArgs e)
        {
            Client.ClientToken = System.Guid.NewGuid();
            UpdateQrCode();
        }

        private void PrintPdf_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Client.FullName))
            {
                MessageBox.Show("Сначала заполните ФИО клиента.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf",
                FileName = $"Пропуск_{Client.FullName.Replace(" ", "_")}.pdf",
                Title = "Сохранить пропуск как PDF"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                PdfExportService.GenerateClientPass(Client, saveFileDialog.FileName);
            }
        }

        private void UpdateQrCode()
        {
            if (Client != null && Client.ClientToken != System.Guid.Empty)
            {
                QrImage.Source = _qrService.GenerateQrCode(Client.ClientToken.ToString());
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrWhiteSpace(FullNameBox.Text))
            {
                var parts = FullNameBox.Text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2 || parts.Length > 3)
                {
                    MessageBox.Show("ФИО должно состоять из 2 или 3 слов (Фамилия Имя [Отчество]).", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Введите ФИО.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PhoneBox.Text.Length != 12) 
            {
                MessageBox.Show("Номер телефона должен быть в формате +7XXXXXXXXXX (полностью заполнен).", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PassportBox.Text.Length != 11)
            {
                MessageBox.Show("Номер паспорта должен быть в формате XXXX XXXXXX (полностью заполнен).", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.DialogResult = true;
        }

        private void FullName_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Zа-яА-Я\s\-]+$");
        }

        private void FullName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            var textBox = sender as TextBox;
            if (textBox == null) return;

            int selectionStart = textBox.SelectionStart;
            string text = textBox.Text;

            string formattedText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());

            if (text != formattedText)
            {
                _isUpdating = true;
                textBox.Text = formattedText;
                textBox.SelectionStart = selectionStart;
                _isUpdating = false;
            }
        }

        private void FullName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_isUpdating) return;
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string cleanText = Regex.Replace(textBox.Text.Trim(), @"\s+", " ");

            if (textBox.Text != cleanText)
            {
                _isUpdating = true;
                textBox.Text = cleanText;
                _isUpdating = false;
            }
        }

        private void Phone_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void Phone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text;

            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            string formatted = "+7";

            if (digitsOnly.Length > 0)
            {
                if (digitsOnly.StartsWith("7") || digitsOnly.StartsWith("8"))
                {
                    if (digitsOnly.Length > 1)
                        formatted += digitsOnly.Substring(1);
                }
                else
                {
                    formatted += digitsOnly;
                }
            }

            if (formatted.Length > 12) formatted = formatted.Substring(0, 12);

            if (text != formatted)
            {
                _isUpdating = true;
                textBox.Text = formatted;
                textBox.SelectionStart = formatted.Length;
                _isUpdating = false;
            }
        }

        private void Passport_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void Passport_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text;
            int selectionStart = textBox.SelectionStart;

            string digitsOnly = new string(text.Where(char.IsDigit).ToArray());

            string formatted = digitsOnly;
            if (digitsOnly.Length > 4)
            {
                formatted = digitsOnly.Insert(4, " ");
            }

            if (text != formatted)
            {
                _isUpdating = true;
                textBox.Text = formatted;

                if (selectionStart == 4 && formatted.Length > 4)
                    textBox.SelectionStart = 5;
                else if (selectionStart > formatted.Length)
                    textBox.SelectionStart = formatted.Length;
                else
                    textBox.SelectionStart = selectionStart;

                _isUpdating = false;
            }
        }
    }
}