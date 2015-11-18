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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace WinCompose
{
    static class Updater
    {
        public static void CheckForUpdates()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            WebClient browser = new WebClient();
            browser.Headers.Add("user-agent", GetUserAgent());
            Stream s = browser.OpenRead("http://wincompose.info/status.txt");
            StreamReader sr = new StreamReader(s);

            for (string line = sr.ReadLine(); line != null;  line = sr.ReadLine())
            {
                string pattern = "([^:]*): (.*[^ ]) *";
                var m = Regex.Match(line, pattern);
                if (m.Groups.Count == 3)
                    data[m.Groups[1].Captures[0].ToString()] = m.Groups[2].Captures[0].ToString();
            }

            sr.Close();
            s.Close();

            foreach (string k in data.Keys)
                Log.Debug("Update data " + k + ": " + data[k]);
        }

        private static string GetUserAgent()
        {
            return string.Format("WinCompose/{0} ({1}{2})",
                                 Settings.Version,
                                 Environment.OSVersion,
                                 Settings.IsDebugging() ? "; Development" :
                                 Settings.IsInstalled() ? "" : "; Portable");
        }
    }
}

