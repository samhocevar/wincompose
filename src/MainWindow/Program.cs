using System;
using System.Windows;

namespace WinCompose.Gui
{
    static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var initialPage = GuiPage.Sequences;
            if (args.Length >= 1)
            {
                try
                {
                    initialPage = (GuiPage)Enum.Parse(typeof(GuiPage), args[0]);

                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid page name passed as argument.", "WinCompose", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            var app = new App();
            app.Run(new Mainwindow(initialPage));
        }
    }
}
