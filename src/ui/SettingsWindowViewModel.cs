//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2018 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Windows;
using WinCompose.i18n;

namespace WinCompose
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        private DelegateCommand m_close_command;
        private string m_selected_language;
        private string m_close_button_text;
        private Visibility m_warn_message_visibility;

        public SettingsWindowViewModel()
        {
            m_close_command = new DelegateCommand(OnCloseCommandExecuted);
            m_selected_language = Settings.Language.Value;
            m_close_button_text = Text.Close;
            m_warn_message_visibility = Visibility.Collapsed;
        }

        public DelegateCommand CloseButtonCommand
        {
            get => m_close_command;
            private set => SetValue(ref m_close_command, value, nameof(CloseButtonCommand));
        }

        public string SelectedLanguage
        {
            get => m_selected_language;
            set => SetValue(ref m_selected_language, value, nameof(SelectedLanguage));
        }

        public Key ComposeKey0 { get => GetComposeKey(0); set => SetComposeKey(0, value); }
        public Key ComposeKey1 { get => GetComposeKey(1); set => SetComposeKey(1, value); }

        private Key GetComposeKey(int index)
        {
            return Settings.ComposeKeys.Value.Count > index ? Settings.ComposeKeys.Value[index] : null;
        }

        private void SetComposeKey(int index, Key key)
        {
            // Rebuild a complete list to force saving configuration file
            var newlist = new KeySequence(Settings.ComposeKeys.Value);
            while (newlist.Count <= index)
                newlist.Add(null);
            newlist[index] = key;
            Settings.ComposeKeys.Value = newlist;
        }

        public string CloseButtonText
        {
            get => m_close_button_text;
            private set => SetValue(ref m_close_button_text, value, nameof(CloseButtonText));
        }

        public Visibility WarnMessageVisibility
        {
            get => m_warn_message_visibility;
            set => SetValue(ref m_warn_message_visibility, value, nameof(WarnMessageVisibility));
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(SelectedLanguage))
            {
                Settings.Language.Value = SelectedLanguage;
                WarnMessageVisibility   = Visibility.Visible;
                CloseButtonText         = Text.Restart;
                CloseButtonCommand      = new DelegateCommand(OnRestartCommandExecuted);
            }
        }

        private static void OnCloseCommandExecuted(object parameter)
        {
            ((Window)parameter).Hide();
        }

        private static void OnRestartCommandExecuted(object parameter)
        {
            System.Windows.Forms.Application.Restart();
        }
    }
}
