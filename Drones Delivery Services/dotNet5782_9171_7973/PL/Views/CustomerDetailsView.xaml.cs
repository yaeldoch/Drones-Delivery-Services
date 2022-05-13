﻿using System.Windows.Controls;

namespace PL.Views
{
    /// <summary>
    /// Interaction logic for CustomerDetailsView.xaml
    /// </summary>
    public partial class CustomerDetailsView : UserControl
    {
        public CustomerDetailsView(int id)
        {
            InitializeComponent();
            DataContext = new ViewModels.CustomerDetailsViewModel(id);
        }
    }
}
