using SwimmingPool.Models;
using SwimmingPool.Services;
using System.Collections.Generic;
using System.Windows;

namespace SwimmingPool.Views
{
    public partial class QuickRegistrationWindow : MahApps.Metro.Controls.MetroWindow
    {
        public Client Client { get; private set; }
        public Membership Membership { get; private set; }
        public List<MembershipType> MembershipTypes { get; set; }

        public QuickRegistrationWindow()
        {
            InitializeComponent();
            Client = new Client();
            Membership = new Membership();
            MembershipTypes = new MembershipTypeRepository().GetAllMembershipTypes();
            this.DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Client.FullName) || Membership.MembershipTypeId <= 0)
            {
                MessageBox.Show("Пожалуйста, заполните ФИО клиента и выберите тип абонемента.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
        }
    }
}
