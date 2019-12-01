//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.IO;
using System.Reflection;
using System.Windows.Threading;

namespace WinCompose
{
    /// <summary>
    /// Wrap an action around a dispatcher timer so it is always run on
    /// the dispatcher thread. This is a fire-and-forget action.
    /// </summary>
    public static class DispatcherTrigger
    {
        public static Action Create(Action a)
        {
            var trigger = new DispatcherTimer();
            trigger.Tick += (o, e) => { trigger.Stop(); a(); };
            Action ret = () => trigger.Start();
            return ret;
        }
    }

    static class Utils
    {
        public static bool EnsureDirectory(string directory)
        {
            try
            {
                Directory.CreateDirectory(directory);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string AppDataDir
        {
            get
            {
                var appdata = Environment.SpecialFolder.ApplicationData;
                var appdatadir = Path.Combine(Environment.GetFolderPath(appdata),
                                              "WinCompose");
                return IsInstalled ? appdatadir : ExecutableDir;
            }
        }

        public static string DataDir
            => Path.Combine(ExecutableDir, IsInstalled ? "res" :
                                           IsDebugging ? "../../rules" : "rules");

        public static string UserDir
            => Environment.ExpandEnvironmentVariables("%USERPROFILE%");

        public static bool IsInstalled
            => File.Exists(Path.Combine(ExecutableDir, "unins000.dat"));

        public static bool IsDebugging
            => File.Exists(Path.ChangeExtension(ExecutableName, ".pdb"));

        private static string ExecutableName
            => Uri.UnescapeDataString(new UriBuilder(ExecutableCodeBase).Path);

        private static string ExecutableCodeBase
            => Assembly.GetExecutingAssembly().GetName().CodeBase;

        private static string ExecutableDir
            => Path.GetDirectoryName(ExecutableName);
    }
}

