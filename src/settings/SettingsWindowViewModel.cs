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

using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using WinCompose.i18n;

namespace WinCompose
{
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        private DelegateCommand    m_close_command;
        private string             m_selected_language;
        private string             m_close_button_text;
        private Visibility         m_warn_message_visibility;

        public SettingsWindowViewModel()
        {
            m_close_command             = new DelegateCommand(OnCloseCommandExecuted);
            m_selected_language         = Settings.Language.Value;
            m_close_button_text         = Text.Close;
            m_warn_message_visibility   = Visibility.Collapsed;
        }

        public DelegateCommand CloseButtonCommand
        {
            get { return m_close_command; }
            private set { SetValue(ref m_close_command, value, "CloseButtonCommand"); }
        }

        public string SelectedLanguage
        {
            get { return m_selected_language; }
            set { SetValue(ref m_selected_language, value, "SelectedLanguage"); }
        }

        public Key ComposeKey0
        {
            get { return Settings.ComposeKeys.Value[0]; }
            set { Settings.ComposeKeys.Value[0] = value; }
        }

        public string CloseButtonText
        {
            get { return m_close_button_text; }
            private set { SetValue(ref m_close_button_text, value, "CloseButtonText"); }
        }

        public Visibility WarnMessageVisibility
        {
            get { return m_warn_message_visibility; }
            set { SetValue(ref m_warn_message_visibility, value, "WarnMessageVisibility"); }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if(propertyName == "SelectedLanguage")
            {
                Settings.Language.Value = SelectedLanguage;
                WarnMessageVisibility   = Visibility.Visible;
                CloseButtonText         = Text.Restart;
                CloseButtonCommand      = new DelegateCommand(OnRestartCommandExecuted);
            }
        }

        private void OnCloseCommandExecuted(object parameter)
        {
            ((Window)parameter).Hide();
        }

        private void OnRestartCommandExecuted(object parameter)
        {
            System.Windows.Forms.Application.Restart();
        }
    }
}
