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

using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace WinCompose
{
    public static class PixelBasedScrollingBehavior 
    {
        public static bool GetIsEnabled(DependencyObject o)
            => (bool)o.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject o, bool val)
            => o.SetValue(IsEnabledProperty, val);

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(PixelBasedScrollingBehavior),
                                                new UIPropertyMetadata(false, IsEnabledChanged));

        private static void IsEnabledChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var val = (bool)e.NewValue;

            // Code is inspired by https://stackoverflow.com/a/17431815/111461
            if (o is VirtualizingPanel || o is ItemsControl)
            {
                var t = typeof(Window).Assembly.GetType("System.Windows.Controls.ScrollUnit");
                var f = typeof(VirtualizingPanel).GetField("ScrollUnitProperty",
                            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (t != null && f?.GetValue(null) is DependencyProperty dp)
                {
                    o.SetValue(dp, Enum.Parse(t, val ? "Pixel" : "Item"));
                    return;
                }
            }

            if (o is VirtualizingPanel)
            {
                var prop = o.GetType().GetProperty("IsPixelBased",
                                BindingFlags.NonPublic | BindingFlags.Instance);
                prop?.SetValue(o, val, null);
            }
        }
    }
}
