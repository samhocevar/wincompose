using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Resources;
using WinCompose.i18n;

namespace WinCompose
{
    public class AboutBoxViewModel : ViewModelBase
    {
        private readonly DelegateCommand m_switch_document_command;
        private readonly DelegateCommand m_openwebsite_command;
        private readonly FlowDocument    m_contributors_document;
        private readonly FlowDocument    m_licence_document;
        private          String          m_active_document_title;
        private          FlowDocument    m_active_document;

        public AboutBoxViewModel()
        {
            m_switch_document_command = new DelegateCommand(OnSwitchDocumentCommandExecuted);
            m_openwebsite_command     = new DelegateCommand(OnOpenWebsiteCommandExecuted);
            m_contributors_document   = LoadDocument("pack://application:,,,/res/contributors.rtf");
            m_licence_document        = LoadDocument("pack://application:,,,/res/copying.rtf");
            m_active_document         = m_contributors_document;
            m_active_document_title   = Text.Contributors;
        }

        private FlowDocument LoadDocument(String resource_uri)
        {
            StreamResourceInfo stream_info = Application.GetResourceStream(new Uri(resource_uri));
            if(stream_info != null)
            {
                using(Stream stream = stream_info.Stream)
                {
                    var         flow_document   = new FlowDocument();
                    TextRange   text_range      = new TextRange(flow_document.ContentStart, flow_document.ContentEnd);
                    text_range.Load(stream, DataFormats.Rtf);
                    return flow_document;
                }
            }
            return null;
        }

        public ICommand SwitchDocumentCommand { get { return m_switch_document_command; } }
        public ICommand OpenWebsiteCommand    { get { return m_openwebsite_command; } }

        public String ActiveDocumentTitle
        {
            get { return m_active_document_title; }
            set { SetValue(ref m_active_document_title, value, "ActiveDocumentTitle");  }
        }

        public FlowDocument ActiveDocument
        {
            get { return m_active_document; }
            private set { SetValue(ref m_active_document, value, "ActiveDocument"); }
        }

        public String Version
        {
            get { return Assembly.GetEntryAssembly().GetName().Version.ToString(); }
        }

        private void OnSwitchDocumentCommandExecuted(object Parameter)
        {
            var document_name = (String)Parameter;
            if(document_name == "contributors")
            {
                ActiveDocumentTitle = Text.Contributors;
                ActiveDocument      = m_contributors_document;
            }
            else if(document_name == "licence")
            {
                ActiveDocumentTitle = Text.Licence;
                ActiveDocument      = m_licence_document;
            }
        }

        private static void OnOpenWebsiteCommandExecuted(object parameter)
        {
            System.Diagnostics.Process.Start("http://wincompose.info/");
        }
    }
}
