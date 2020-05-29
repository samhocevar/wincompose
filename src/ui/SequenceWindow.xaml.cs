//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
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
using System.Windows.Input;
using System.Windows.Media;

namespace WinCompose
{
    /// <summary>
    /// Interaction logic for mainwindow.xaml
    /// </summary>
    public partial class SequenceWindow : INotifyPropertyChanged
    {
        public SequenceWindow()
        {
            InitializeComponent();
            DataContext = new RootViewModel();
            Activated += (o, e) => SearchBox.Focus();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private RootViewModel m_view_model => (RootViewModel)DataContext;

        private TextBox SearchBox
        {
            get
            {
                var grid = VisualTreeHelper.GetChild(SearchWidget, 0) as Grid;
                return VisualTreeHelper.GetChild(grid, 0) as TextBox;
            }
        }

        protected virtual void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCloseCommandExecuted(object Sender, ExecutedRoutedEventArgs e)
        {
            // If the search box is focused and non-empty, clear it; otherwise,
            // actually close the window.
            if (SearchBox.IsFocused && !string.IsNullOrEmpty(m_view_model.SearchText))
                m_view_model.SearchText = "";
            else
                Hide();
        }

        private void OnCopyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var seq_view = (ListBox.SelectedItem as SequenceViewModel)?.Result;
            if(seq_view != null)
                Clipboard.SetText(seq_view);
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
            => m_view_model.SearchText = "";

        private void EditUserDefinedSequences_Click(object sender, RoutedEventArgs e)
            => Settings.EditCustomRulesFile();

        private void ReloadUserDefinedSequences_Click(object sender, RoutedEventArgs e)
        {
            Settings.LoadSequences();
            // Create a new view model to sync the sequence list UI with the
            // active sequences. Preserve the search text while doing so.
            var search_text = m_view_model.SearchText;
            DataContext = new RootViewModel();
            m_view_model.SearchText = search_text;
        }

        private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
            => (ListBox.SelectedItem as SequenceViewModel)?.ToggleFavorite();
    }
}
