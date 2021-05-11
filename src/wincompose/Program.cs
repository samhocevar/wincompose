//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2021 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WinCompose
{

    static class Program
    {
        static Dictionary<string, MenuCommand> m_command_flags = new Dictionary<string, MenuCommand>()
        {
            {  "-sequences", MenuCommand.ShowSequences },
            {  "-settings",  MenuCommand.ShowOptions },
        };

        [STAThread]
        static void Main(string[] args)
        {
            // Some commandline flags just trigger a message broadcast
            foreach (var arg in args)
            {
                if (m_command_flags.TryGetValue(arg, out var cmd))
                {
                    NativeMethods.PostMessage(HWND.BROADCAST, WM_WINCOMPOSE.OPEN, (int)cmd, 0);
                    return;
                }
            }

            Logging.Init();
            Logger.Info($"WinCompose {Settings.Version} started, arguments: “{string.Join(" ", args)}”");

            // Do this early because of the AutoLaunch setting
            Settings.LoadConfig();

            bool from_task = args.Contains("-fromtask");
            bool from_startup = args.Contains("-fromstartup");

            // If run from the task scheduler or from startup but autolaunch is
            // disabled, exit early.
            if ((from_task || from_startup) && !Settings.AutoLaunch.Value)
                return;


            // If run from Task Scheduler, we need to detach otherwise the
            // system may kill us after some time.
            if (from_task)
            {
                Process.Start(Application.ResourceAssembly.Location, "-detached");
                return;
            }

            // If run from the start menu but there is a task scheduler entry,
            // give it priority.
            if (from_startup && SchTasks.HasTask("WinCompose"))
                return;

            SchTasks.InstallTask("WinCompose");

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
                Logger.Info("Program shut down");
            }
        }

        private static ILogger Logger = LogManager.GetCurrentClassLogger();
    }
}

