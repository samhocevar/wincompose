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
        private readonly RootViewModel viewModel;

        public Mainwindow()
        {
            InitializeComponent();
            var notifyicon = new WinForms.NotifyIcon
            {
                Visible = true,
                Icon = properties.resources.icon_normal
            };
            notifyicon.DoubleClick += NotifyiconDoubleclicked;
            notifyicon.MouseDown += NotifyiconMouseDown;
            viewModel = new RootViewModel();
            DataContext = viewModel;
#if !RELEASE
            OpenFromTray();
#endif
        }

        private void NotifyiconMouseDown(object sender, WinForms.MouseEventArgs e)
        {
            if (e.Button == WinForms.MouseButtons.Right)
            {
                var menu = (System.Windows.Controls.ContextMenu)this.FindResource("NotifierContextMenu");
                menu.IsOpen = true;
            }
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

        private void ContextMenuShowSequences(object sender, RoutedEventArgs e)
        {
            OpenFromTray();
        }

        private void ContextMenuExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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
            MainFrame.Navigate(new SequencePage(viewModel));
        }

        private void CloseToTray()
        {
            ShowInTaskbar = false;
            MainFrame.Navigate(null);
            Hide();
        }

        private void CloseWindowClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseToTray();
            e.Cancel = true;
        }
    }
}
