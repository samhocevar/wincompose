//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2015 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace WinCompose
{
    /// <summary>
    /// Interaction logic for mainwindow.xaml
    /// </summary>
    public partial class Mainwindow : INotifyPropertyChanged
    {
        private readonly RootViewModel viewModel;

        private GuiPage activePage;

        /// <summary>
        /// Identifies a page of the GUI.
        /// </summary>
        public enum GuiPage
        {
            /// <summary>
            /// Identifies an empty page.
            /// </summary>
            None,

            /// <summary>
            /// Identifies the page displaying sequences.
            /// </summary>
            Sequences,

            /// <summary>
            /// Identifies the page displaying settings.
            /// </summary>
            Settings,
        }

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
            //Close();
            Settings.SaveConfig();
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
                    return i18n.Text.Settings;
                case GuiPage.Settings:
                    return i18n.Text.Sequences;
                default:
                    throw new ArgumentOutOfRangeException("page");
            }
        }
    }
}
