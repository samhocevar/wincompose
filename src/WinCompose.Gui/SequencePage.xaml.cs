using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WinCompose.gui
{
    /// <summary>
    /// Interaction logic for SequencePage.xaml
    /// </summary>
    public partial class SequencePage : Page
    {
        private RootViewModel ViewModel { get { return (RootViewModel)DataContext; } }

        public SequencePage(RootViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ClearSearchClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchText = "";
        }
    }
}
