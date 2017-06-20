//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2016 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using WinForms = System.Windows.Forms;

namespace WinCompose
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Do this before Composer.Init() because of the Disabled setting
            Settings.LoadConfig();

            Composer.Init();
            Settings.LoadSequences();
            Updater.Init();

            Settings.StartWatchConfigFile();

            try
            {
                WinForms.Application.EnableVisualStyles();
                WinForms.Application.SetCompatibleTextRenderingDefault(false);

                // Must call this after SetCompatibleTextRenderingDefault()
                // because it uses a WinForms timer which seems to open a
                // hidden window. The reason we use a WinForms timer is so that
                // the hook is installed from the main thread.
                KeyboardHook.Init();

                using (SysTrayIcon icon = new SysTrayIcon())
                {
                    WinForms.Application.Run();
                }
            }
            finally
            {
                Settings.StopWatchConfigFile();
                Updater.Fini();
                KeyboardHook.Fini();
                Settings.SaveConfig();
                Composer.Fini();
                Updater.Fini();
            }
        }
    }
}

