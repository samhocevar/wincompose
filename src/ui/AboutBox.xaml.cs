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

using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WinCompose
{
    /// <summary>
    /// Interaction logic for AboutBox.xaml
    /// </summary>
    public partial class AboutBox : BaseWindow
    {
        public static readonly DependencyProperty WebContentProperty;

        static AboutBox()
        {
            WebContentProperty = DependencyProperty.RegisterAttached("WebContent", typeof(Stream), typeof(AboutBox), new PropertyMetadata(OnWebContentPropertyChanged));
        }

        public AboutBox()
        {
            DataContext = new AboutBoxViewModel();
            InitializeComponent();
        }

        public static Stream GetWebContent(WebBrowser web_browser) { return web_browser.GetValue(WebContentProperty) as Stream; }
        public static void SetWebContent(WebBrowser web_browser, Stream value) { web_browser.SetValue(WebContentProperty, value); }

        private static void OnWebContentPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var web_browser = (WebBrowser)obj;
            web_browser.NavigateToStream(args.NewValue as Stream);
        }
    }
}
