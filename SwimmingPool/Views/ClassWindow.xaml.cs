using SwimmingPool.Models;
using SwimmingPool.Services;
using System.Collections.Generic;
using MahApps.Metro.Controls;

namespace SwimmingPool.Views
{
    public partial class ClassWindow : MetroWindow
    {
        public Class Class { get; private set; }
        public string Title { get; private set; }
        public List<ActivityType> ActivityTypes { get; set; }
        public List<Trainer> Trainers { get; set; }
        public List<Pool> Pools { get; set; }

        public ClassWindow(Class aClass, string title)
        {
            InitializeComponent();
            Class = aClass;
            Title = title;
            ActivityTypes = new ActivityTypeRepository().GetAllActivityTypes();
            Trainers = new TrainerRepository().GetAllTrainers();
            Pools = new PoolRepository().GetAllPools();
            this.DataContext = this;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
