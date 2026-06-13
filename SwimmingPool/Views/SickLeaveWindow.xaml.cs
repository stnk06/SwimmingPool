using System;
using System.Windows;
using MahApps.Metro.Controls;
using SwimmingPool.ViewModels;

namespace SwimmingPool.Views
{
    public partial class SickLeaveWindow : MetroWindow
    {
        public SickLeaveWindow(SickLeaveViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseAction = new Action(this.Close);
        }
    }
}