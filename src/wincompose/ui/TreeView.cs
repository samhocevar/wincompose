//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Windows;

using WPFGrid = System.Windows.Controls.Grid;
using WPFTreeView = System.Windows.Controls.TreeView;
using WPFTreeViewItem = System.Windows.Controls.TreeViewItem;

namespace WinCompose
{
    class TreeView : WPFTreeView
    {
        protected override DependencyObject GetContainerForItemOverride()
            => new TreeViewItem();

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is TreeViewItem;

        class TreeViewItem : WPFTreeViewItem
        {
            public TreeViewItem()
                => Loaded += OnLoaded;

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                if (VisualChildrenCount > 0)
                {
                    var grid = GetVisualChild(0) as WPFGrid;
                    if (grid != null && grid.ColumnDefinitions.Count == 3)
                        grid.ColumnDefinitions.RemoveAt(1);
                }
            }

            protected override DependencyObject GetContainerForItemOverride()
                => new TreeViewItem();

            protected override bool IsItemItsOwnContainerOverride(object item)
                => item is TreeViewItem;
        }
    }
}
