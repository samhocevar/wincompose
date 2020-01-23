//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2020 Sam Hocevar <sam@hocevar.net>
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
using System.Diagnostics;

namespace WinCompose
{
    public delegate void Action(); // This type was only added in .NET 3.5

    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Some commandline flags just trigger a message broadcast
            var command_flags = new Dictionary<string, MenuCommand>()
            {
                {  "-sequences", MenuCommand.ShowSequences },
                {  "-settings",  MenuCommand.ShowOptions },
            };

            foreach (var arg in args)
            {
                if (command_flags.TryGetValue(arg, out var cmd))
                {
                    NativeMethods.PostMessage(HWND.BROADCAST, WM_WINCOMPOSE.OPEN, (int)cmd, 0);
                    return;
                }
            }

            // If started from Task Scheduler, we need to detach otherwise the
            // system may kill us after some time.
            if (Array.Find(args, arg => arg == "/fromtask") != null)
            {
                Process.Start(Application.ResourceAssembly.Location);
                return;
            }

            // Do this before Composer.Init() because of the Disabled setting
            Settings.LoadConfig();

            Settings.LoadSequences();
            Metadata.LoadDB();
            KeyboardHook.Init();
            Updater.Init();

            Settings.StartWatchConfigFile();

            try
            {
                var app = new Application();
                app.Run();
            }
            finally
            {
                Settings.StopWatchConfigFile();
                Updater.Fini();
                KeyboardHook.Fini();
                Settings.SaveConfig();
                Metadata.SaveDB();
                Updater.Fini();
            }
        }
    }
}

