using MahApps.Metro.Controls;
using SwimmingPool.ViewModels;
using System;

namespace SwimmingPool.Views
{
    public partial class ClassDetailsWindow : MetroWindow
    {
        public ClassDetailsWindow(ClassDetailsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseAction = new Action(this.Close);
        }
    }
}