//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2018 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace WinCompose
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            ShowInTaskbar = false;
            InitializeComponent();

            DataContext = Log.Entries;
            Log.Entries.CollectionChanged += OnEntriesChanged;
        }

        ~DebugWindow()
        {
            Log.Entries.CollectionChanged -= OnEntriesChanged;
        }

        ScrollViewer m_scrollviewer;

        private void OnEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (m_scrollviewer == null)
            {
                // Find the ScrollViewer below our DockPanel
                foreach (var child in m_dockpanel.Children)
                {
                    ItemsControl c = child as ItemsControl;
                    if (c != null)
                    {
                        object o = c.Template.FindName("m_scrollviewer", c);
                        if (o is ScrollViewer)
                            m_scrollviewer = o as ScrollViewer;
                    }
                }
            }

            m_scrollviewer?.ScrollToEnd();
        }
    }
}
