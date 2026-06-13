using SwimmingPool.Models;
using MahApps.Metro.Controls;

namespace SwimmingPool.Views
{
    public partial class MembershipTypeWindow : MetroWindow
    {
        public MembershipType MembershipType { get; private set; }
        public string Title { get; private set; }

        public MembershipTypeWindow(MembershipType membershipType, string title)
        {
            InitializeComponent();
            MembershipType = membershipType;
            Title = title;
            this.DataContext = this;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
