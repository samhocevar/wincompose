//
// WinCompose — a compose key for Windows
//
// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//                     2014 Benjamin Litzelmann
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What the Fuck You Want
//   to Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinCompose
{
    static class Program
    {
        private static Process guiProcess;
        private static NotifyIcon m_notifyicon;

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
            if (guiProcess == null)
            {
                StartGui(GuiPage.Sequences);
            }
            else
            {
                TerminateGui();
            }
        }

        private static void TimerTicked(object sender, EventArgs e)
        {
            m_notifyicon.Icon = Compose.IsComposing() ? Properties.Resources.IconActive
                                                      : Properties.Resources.IconNormal;
        }

        private static void ShowSequencesClicked(object sender, EventArgs e)
        {
            StartGui(GuiPage.Sequences);
        }

        private static void ShowSettingsClicked(object sender, EventArgs e)
        {
            StartGui(GuiPage.Settings);
        }

        private static void ExitClicked(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private static void StartGui(GuiPage page)
        {
            try
            {
                guiProcess = Process.Start("WinCompose.Gui.exe", page.ToString());
                if (guiProcess != null)
                {
                    guiProcess.EnableRaisingEvents = true;
                    guiProcess.Exited += (s, e) => guiProcess = null;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.ErrorStartingGui, Properties.Resources.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void TerminateGui()
        {
            try
            {
                if (guiProcess != null)
                {
                    guiProcess.Kill();
                }
            }
            finally
            {
                guiProcess = null;
            }
        }
    }
}
