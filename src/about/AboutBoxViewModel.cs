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
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Resources;
using WinCompose.i18n;

namespace WinCompose
{
    public class AboutBoxViewModel : ViewModelBase
    {
        private readonly DelegateCommand    m_switch_document_command;
        private readonly DelegateCommand    m_openwebsite_command;
        private readonly DelegateCommand    m_reportbug_command;
        private readonly Uri                m_contributors_document_uri;
        private readonly Uri                m_licence_document_uri;
        private          string             m_active_document_title;
        private          Uri                m_active_document_uri;

        public AboutBoxViewModel()
        {
            m_switch_document_command       = new DelegateCommand(OnSwitchDocumentCommandExecuted);
            m_openwebsite_command           = new DelegateCommand(OnOpenWebsiteCommandExecuted);
            m_reportbug_command             = new DelegateCommand(OnReportBugCommandExecuted);
            m_contributors_document_uri     = new Uri("pack://application:,,,/res/contributors.html");
            m_licence_document_uri          = new Uri("pack://application:,,,/res/copying.html");
            m_active_document_uri           = m_contributors_document_uri;
            m_active_document_title         = Text.Contributors;
        }


        public ICommand SwitchDocumentCommand { get { return m_switch_document_command; } }
        public ICommand OpenWebsiteCommand    { get { return m_openwebsite_command; } }
        public ICommand OpenReportBugCommand  { get { return m_reportbug_command; } }

        public string ActiveDocumentTitle
        {
            get { return m_active_document_title; }
            set { SetValue(ref m_active_document_title, value, "ActiveDocumentTitle");  }
        }

        public Stream ActiveDocument
        {
            get { return Application.GetResourceStream(m_active_document_uri).Stream; }
        }

        public string Version
        {
            get { return Program.Version; }
        }

        private void OnSwitchDocumentCommandExecuted(object parameter)
        {
            var document_name = (string)parameter;
            if (document_name == "contributors")
            {
                ActiveDocumentTitle = Text.Contributors;
                m_active_document_uri = m_contributors_document_uri;
                OnPropertyChanged("ActiveDocument");
            }
            else if (document_name == "licence")
            {
                ActiveDocumentTitle = Text.License;
                m_active_document_uri = m_licence_document_uri;
                OnPropertyChanged("ActiveDocument");
            }
        }

        private static void OnOpenWebsiteCommandExecuted(object parameter)
        {
            System.Diagnostics.Process.Start("http://wincompose.info/");
        }

        private static void OnReportBugCommandExecuted(object parameter)
        {
            System.Diagnostics.Process.Start("https://github.com/samhocevar/wincompose/issues/new");
        }
    }
}
