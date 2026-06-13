using SwimmingPool.Models;
using MahApps.Metro.Controls;

namespace SwimmingPool.Views
{
    public partial class PoolWindow : MetroWindow
    {
        public Pool Pool { get; private set; }
        public string Title { get; private set; }

        public PoolWindow(Pool pool, string title)
        {
            InitializeComponent();
            Pool = pool;
            Title = title;
            this.DataContext = this;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
