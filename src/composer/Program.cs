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
using System.Windows.Forms;

namespace WinCompose
{
    static class Program
    {
        private static NotifyIcon m_notifyicon;
        private static Mainwindow m_mainwindow;

        [STAThread]
        static void Main()
        {
            Settings.LoadConfig();
            Settings.LoadSequences();
            KeyboardHook.Install();

            Settings.StartWatchConfigFile();

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                m_notifyicon = new NotifyIcon
                {
                    Visible = true,
                    Icon = Properties.Resources.IconNormal,
                    ContextMenu = new ContextMenu(new[]
                    {
                        new MenuItem(Properties.Resources.ShowSequences, ShowSequencesClicked),
                        new MenuItem(Properties.Resources.ShowSettings, ShowSettingsClicked),
                        new MenuItem(Properties.Resources.Exit, ExitClicked)
                    })
                };
                m_notifyicon.DoubleClick += NotifyiconDoubleclicked;

                m_mainwindow = new Mainwindow(GuiPage.Sequences)
                {
                    Visibility = System.Windows.Visibility.Hidden
                };

                var timer = new Timer
                {
                    Enabled = true,
                    Interval = 50, /* 50 milliseconds is probably enough */
                };
                timer.Tick += TimerTicked;

                Application.Run();
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
        }

        private static void ShowSequencesClicked(object sender, EventArgs e)
        {
            // TODO: change page
            m_mainwindow.Show();
        }

        private static void ShowSettingsClicked(object sender, EventArgs e)
        {
            // TODO: change page
            m_mainwindow.Show();
        }

        private static void ExitClicked(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
