//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.IO;
using System.Windows.Input;

namespace WinCompose
{
    public class AboutBoxViewModel : ViewModelBase
    {
        private readonly DelegateCommand m_openwebsite_command;
        private readonly DelegateCommand m_reportbug_command;

        public AboutBoxViewModel()
        {
            m_openwebsite_command = new DelegateCommand(OnOpenWebsiteCommandExecuted);
            m_reportbug_command = new DelegateCommand(OnReportBugCommandExecuted);
        }


        public ICommand OpenWebsiteCommand => m_openwebsite_command;
        public ICommand OpenReportBugCommand => m_reportbug_command;

        public Stream AuthorsDocument
            => Application.GetResourceStream(new Uri("pack://application:,,,/res/contributors.html")).Stream;

        public Stream LicenseDocument
            => Application.GetResourceStream(new Uri("pack://application:,,,/res/copying.html")).Stream;

        public string Version => Settings.Version;

        private static void OnOpenWebsiteCommandExecuted(object parameter)
            => System.Diagnostics.Process.Start("http://wincompose.info/");

        private static void OnReportBugCommandExecuted(object parameter)
            => System.Diagnostics.Process.Start("https://github.com/samhocevar/wincompose/issues/new");
    }
}
