//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2021 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Collections.Specialized;
using System.Linq;
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

            DataContext = Logging.Entries;
            Logging.Entries.CollectionChanged += OnEntriesChanged;
        }

        ~DebugWindow()
        {
            Logging.Entries.CollectionChanged -= OnEntriesChanged;
        }

        ScrollViewer m_scrollviewer;

        private void OnEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (m_scrollviewer == null)
            {
                // Find the ScrollViewer below our DockPanel
                foreach (var c in m_dockpanel.Children.OfType<ItemsControl>())
                    if (c.Template.FindName("m_scrollviewer", c) is ScrollViewer sv)
                        m_scrollviewer = sv;
            }

            m_scrollviewer?.ScrollToEnd();
        }
    }
}
