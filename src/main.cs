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

        [STAThread]
        static void Main()
        {
            Settings.LoadConfig();
            Settings.LoadSequences();
            KeyboardHook.Install();

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var notifyicon = new NotifyIcon
                {
                    Visible = true,
                    Icon = properties.resources.icon_normal,
                    ContextMenu = new ContextMenu(new[]
                    {
                        new MenuItem(properties.resources.ShowSequences, ShowSequencesClicked),
                        new MenuItem(properties.resources.ShowSettings, ShowSettingsClicked),
                        new MenuItem(properties.resources.Exit, ExitClicked)
                    })
                };
                notifyicon.DoubleClick += NotifyiconDoubleclicked;
                Application.Run();
                GC.KeepAlive(notifyicon);
            }
            finally
            {
                KeyboardHook.Uninstall();
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
                MessageBox.Show("Unable to start the GUI process.", "WinCompose", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
