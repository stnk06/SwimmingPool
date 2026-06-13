using SwimmingPool.Models;
using MahApps.Metro.Controls;

namespace SwimmingPool.Views
{
    public partial class ActivityTypeWindow : MetroWindow
    {
        public ActivityType ActivityType { get; private set; }
        public string Title { get; private set; }

        public ActivityTypeWindow(ActivityType activityType, string title)
        {
            InitializeComponent();
            ActivityType = activityType;
            Title = title;
            this.DataContext = this;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
