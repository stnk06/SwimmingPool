using SwimmingPool.Models;
using MahApps.Metro.Controls;

namespace SwimmingPool.Views
{
    public partial class TrainerWindow : MetroWindow
    {
        public Trainer Trainer { get; private set; }
        public string Title { get; private set; }

        public TrainerWindow(Trainer trainer, string title)
        {
            InitializeComponent();
            Trainer = trainer;
            Title = title;
            this.DataContext = this;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
