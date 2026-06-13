using SwimmingPool.Models;
using SwimmingPool.Services;
using System.Collections.Generic;
using MahApps.Metro.Controls;

namespace SwimmingPool.Views
{
    public partial class MembershipWindow : MetroWindow
    {
        public Membership Membership { get; private set; }
        public string Title { get; private set; }
        public List<Client> Clients { get; set; }
        public List<MembershipType> MembershipTypes { get; set; }

        public MembershipWindow(Membership membership, string title)
        {
            InitializeComponent();
            Membership = membership;
            Title = title;
            Clients = new ClientRepository().GetAllClients();
            MembershipTypes = new MembershipTypeRepository().GetAllMembershipTypes();
            this.DataContext = this;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
