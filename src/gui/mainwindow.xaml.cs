using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
namespace WinCompose.gui
{
    /// <summary>
    /// Interaction logic for mainwindow.xaml
    /// </summary>
    public partial class Mainwindow : INotifyPropertyChanged
    {
        private readonly RootViewModel viewModel;

        private Page activePage;
        private string switchPageName = properties.resources.Settings;

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

        public string SwitchPageName { get { return switchPageName; } set { switchPageName = value; OnPropertyChanged("SwitchPageName"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void NotifyiconMouseDown(object sender, WinForms.MouseEventArgs e)
        {
            if (e.Button == WinForms.MouseButtons.Right)
            {
                var menu = (ContextMenu)FindResource("NotifierContextMenu");
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
            activePage = new SequencePage(viewModel);
            MainFrame.Navigate(activePage);
        }

        private void CloseToTray()
        {
            ShowInTaskbar = false;
            MainFrame.Navigate(null);
            Hide();
        }

        private void CloseWindowClicked(object sender, CancelEventArgs e)
        {
            CloseToTray();
            e.Cancel = true;
        }

        private void PageSwitchClicked(object sender, RoutedEventArgs e)
        {
            if (SwitchPageName == properties.resources.Settings)
            {
                activePage = new SettingsPage();
                SwitchPageName = properties.resources.Sequences;
            }
            else
            {
                activePage = new SequencePage(viewModel);
                SwitchPageName = properties.resources.Settings;
            }
            MainFrame.Navigate(activePage);
        }
    }
}
