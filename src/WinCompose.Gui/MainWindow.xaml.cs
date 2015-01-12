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

        private GuiPage activePage;

        public Mainwindow(GuiPage initialPage)
        {
            InitializeComponent();
            viewModel = new RootViewModel();
            DataContext = viewModel;
            OpenFromTray(initialPage);
        }

        public string SwitchPageName { get { return GetSwitchPageName(ActivePage); } }

        public GuiPage ActivePage { get { return activePage; } set { activePage = value; OnPropertyChanged("ActivePage", "SwitchPageName"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(params string[] propertyNames)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                foreach (var propertyName in propertyNames)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }
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
                OpenFromTray(ActivePage);
            }
            else
            {
                CloseToTray();
            }
        }

        private void ContextMenuShowSequences(object sender, RoutedEventArgs e)
        {
            OpenFromTray(GuiPage.Sequences);
        }

        private void ContextMenuShowSettings(object sender, RoutedEventArgs e)
        {
            OpenFromTray(GuiPage.Settings);
        }

        private void ContextMenuExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            CloseToTray();
        }

        private void OpenFromTray(GuiPage page)
        {
            ShowInTaskbar = true;
            Show();
            Activate();
            LoadPage(page);
            ActivePage = page;
        }

        private void CloseToTray()
        {
            ShowInTaskbar = false;
            LoadPage(GuiPage.None);
            Hide();
            Close();
        }

        private void CloseWindowClicked(object sender, CancelEventArgs e)
        {
            //CloseToTray();
            //e.Cancel = true;
        }

        private void PageSwitchClicked(object sender, RoutedEventArgs e)
        {
            var nextPage = ActivePage == GuiPage.Sequences ? GuiPage.Settings : GuiPage.Sequences;
            LoadPage(nextPage);
            ActivePage = nextPage;
        }

        public void LoadPage(GuiPage page)
        {
            switch (page)
            {
                case GuiPage.None:
                    MainFrame.Navigate(null);
                    break;
                case GuiPage.Sequences:
                    MainFrame.Navigate(new SequencePage(viewModel));
                    break;
                case GuiPage.Settings:
                    MainFrame.Navigate(new SettingsPage());
                    break;
                default:
                    throw new ArgumentOutOfRangeException("page");
            }
        }

        private static string GetSwitchPageName(GuiPage page)
        {
            switch (page)
            {
                case GuiPage.None:
                    return string.Empty;
                case GuiPage.Sequences:
                    return Gui.Properties.Resources.Settings;
                case GuiPage.Settings:
                    return Gui.Properties.Resources.Sequences;
                default:
                    throw new ArgumentOutOfRangeException("page");
            }
        }
    }
}
