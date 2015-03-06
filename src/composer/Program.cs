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
using System.Diagnostics;
using WinForms = System.Windows.Forms;

namespace WinCompose
{
    static class Program
    {
        private static WinForms.NotifyIcon m_notifyicon;
        private static Mainwindow m_mainwindow;
        private static SettingsWindow m_settingswindow;

        [STAThread]
        static void Main()
        {
            Settings.LoadConfig();
            Settings.LoadSequences();
            KeyboardHook.Install();

            Settings.StartWatchConfigFile();

            try
            {
                WinForms.Application.EnableVisualStyles();
                WinForms.Application.SetCompatibleTextRenderingDefault(false);

                m_notifyicon = new WinForms.NotifyIcon
                {
                    Visible = true,
                    Icon = Properties.Resources.IconNormal,
                    ContextMenu = new WinForms.ContextMenu(new[]
                    {
                        new WinForms.MenuItem(i18n.Text.ShowSequences, ShowSequencesClicked),
                        new WinForms.MenuItem(i18n.Text.ShowSettings, ShowSettingsClicked),
                        new WinForms.MenuItem(i18n.Text.Exit, ExitClicked)
                    })
                };
                m_notifyicon.DoubleClick += NotifyiconDoubleclicked;

                m_settingswindow = new SettingsWindow()
                {
                    Visibility = System.Windows.Visibility.Hidden
                };

                m_mainwindow = new Mainwindow(Mainwindow.GuiPage.Sequences)
                {
                    Visibility = System.Windows.Visibility.Hidden
                };

                var timer = new WinForms.Timer
                {
                    Enabled = true,
                    Interval = 50, /* 50 milliseconds is probably enough */
                };
                timer.Tick += TimerTicked;

                WinForms.Application.Run();
                GC.KeepAlive(m_notifyicon);
            }
            finally
            {
                KeyboardHook.Uninstall();
                Settings.StopWatchConfigFile();
                Settings.SaveConfig();
            }
        }

        private static void NotifyiconDoubleclicked(object sender, EventArgs e)
        {
            if (m_mainwindow.IsVisible)
            {
                m_mainwindow.Hide();
            }
            else
            {
                m_mainwindow.Show();
            }
        }

        private static void TimerTicked(object sender, EventArgs e)
        {
            m_notifyicon.Icon = Composer.IsComposing() ? Properties.Resources.IconActive
                                                       : Properties.Resources.IconNormal;
            m_notifyicon.Text = String.Format(i18n.Text.TrayToolTip,
                                              Settings.GetComposeKeyName(),
                                              Settings.GetSequenceCount());
        }

        private static void ShowSequencesClicked(object sender, EventArgs e)
        {
            m_mainwindow.Show();
        }

        private static void ShowSettingsClicked(object sender, EventArgs e)
        {
            m_settingswindow.Show();
        }

        private static void ExitClicked(object sender, EventArgs e)
        {
            WinForms.Application.Exit();
        }
    }
}
