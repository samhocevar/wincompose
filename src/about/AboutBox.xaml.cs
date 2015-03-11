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
