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
    public partial class SequenceWindow : INotifyPropertyChanged
    {
        private RootViewModel ViewModel { get { return (RootViewModel)DataContext; } }

        public SequenceWindow()
        {
            InitializeComponent();
            DataContext = new RootViewModel();
            OpenFromTray();
        }

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

        private void ClearSearchClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchText = "";
        }

        private void CloseWindowClicked(object sender, CancelEventArgs e)
        {
            CloseToTray();
            e.Cancel = true;
        }
    }
}
