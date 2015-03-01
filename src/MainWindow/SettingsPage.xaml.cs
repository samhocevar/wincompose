using System.Windows;

namespace WinCompose.Gui
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            Unloaded += ApplySettings;
        }

        private void ApplySettings(object sender, RoutedEventArgs e)
        {
            Settings.SaveConfig();
        }
    }
}
