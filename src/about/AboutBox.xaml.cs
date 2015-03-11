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
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WinCompose
{
    /// <summary>
    /// Interaction logic for AboutBox.xaml
    /// </summary>
    public partial class AboutBox : Window
    {
        public static readonly DependencyProperty DocumentProperty;

        static AboutBox()
        {
            DocumentProperty = DependencyProperty.RegisterAttached("Document", typeof(FlowDocument), typeof(AboutBox), new PropertyMetadata(OnDocumentPropertyChanged));
        }

        public AboutBox()
        {
            DataContext = new AboutBoxViewModel();
            InitializeComponent();
        }

        public static FlowDocument GetDocument(RichTextBox text_box) { return text_box.GetValue(DocumentProperty) as FlowDocument; }
        public static void SetDocument(RichTextBox text_box, FlowDocument value) { text_box.SetValue(DocumentProperty, value); }

        private static void OnDocumentPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var text_box = (RichTextBox)obj;
            text_box.Document = args.NewValue as FlowDocument;
        }
    }
}
