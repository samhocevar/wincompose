using System;
using System.Windows;
using WinForms = System.Windows.Forms;
namespace WinCompose.gui
{
    /// <summary>
    /// Interaction logic for mainwindow.xaml
    /// </summary>
    public partial class Mainwindow
    {
        private RootViewModel viewModel;

        public Mainwindow()
        {
            InitializeComponent();
            var notifyicon = new WinForms.NotifyIcon
            {
                Visible = true,
                Icon = properties.resources.icon_normal
            };

            viewModel = new RootViewModel();
            DataContext = viewModel;
            notifyicon.DoubleClick += NotifyiconDoubleclicked;
#if RELEASE
            CloseToTray();
#endif
        }

        private void NotifyiconDoubleclicked(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                OpenFromTray();
            }
            else
            {
                CloseToTray();
            }
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            CloseToTray();
        }
        
        private void OpenFromTray()
        {
            ShowInTaskbar = true;
            Show();
            Activate();
        }

        private void CloseToTray()
        {
            ShowInTaskbar = false;
            Hide();
        }

        private void ClearSearchClicked(object sender, RoutedEventArgs e)
        {
            viewModel.SearchText = "";
        }
    }
}
