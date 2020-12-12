//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2020 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Diagnostics;

namespace WinCompose
{
    internal static class SchTasks
    {
        public static bool HasTask()
            => !string.IsNullOrEmpty(Run("/query /tn WinCompose /xml"));

        private static string Run(string args)
        {
            var pi = new ProcessStartInfo();
            pi.FileName = "schtasks.exe";
            pi.Arguments = args;
            pi.UseShellExecute = false;
            pi.RedirectStandardOutput = true;
            var p = Process.Start(pi);
            p.WaitForExit();
            return p.ExitCode == 0 ? p.StandardOutput.ReadToEnd() : null;
        }
    }
}
