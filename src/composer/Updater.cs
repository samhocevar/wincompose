//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2018 Sam Hocevar <sam@hocevar.net>
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
using System.Threading;

namespace WinCompose
{

static class Updater
{
    public static void Init()
    {
        m_thread = new Thread(() => { Updater.Run(); });
        m_thread.Start();
    }

    public static void Fini()
    {
        m_thread.Interrupt();
        m_thread.Join();
    }

    /// <summary>
    /// Other modules can listen to this event to be warned when upgrade information
    /// has been retrieved.
    /// </summary>
    public static event Action Changed;

    private static void Run()
    {
        for (;;)
        {
            try
            {
                UpdateStatus();

                if (HasNewerVersion)
                {
                    Changed?.Invoke();
                }

                // Sleep between 30 and 90 minutes before querying again
                Thread.Sleep(new Random().Next(30, 90) * 60 * 1000);
            }
            catch (ThreadInterruptedException)
            {
                return;
            }
        }
    }

    public static bool HasNewerVersion
    {
        get
        {
            string latest = Get("Latest");
            if (latest == null)
                return false;

            var current = SplitVersionString(Settings.Version);
            var available = SplitVersionString(latest);

            for (int i = 0; i < 4; ++i)
                if (current[i] < available[i])
                    return true;

            return false;
        }
    }

    private static List<int> SplitVersionString(string str)
    {
        List<int> ret = new List<int>();
        int tmp;

        // If we fail to parse a chunk, use 1 instead of 0 so that version
        // 1.2.3.foo is still greater than 1.2.3.0.
        foreach (var e in str.Replace("beta", ".").Split(new char[] { '.' }))
            ret.Add(int.TryParse(e, out tmp) ? tmp : 1);

        // If fewer than 4 elements, add zeroes; if more than 4, remove them
        while (ret.Count < 4)
            ret.Add(0);

        while (ret.Count > 4)
            ret.RemoveAt(ret.Count - 1);

        // Handle beta versions in the form 1.2.3beta456 that need to be
        // smaller than 1.2.3.0 but greater than any realistic 1.2.2.x.
        if (str.Contains("beta"))
        {
            ret[2] -= 1;
            ret[3] += 100000000;
        }

        return ret;
    }

    public static string Get(string key)
    {
        string ret = null;
        m_data.TryGetValue(key, out ret);
        return ret;
    }

    /// <summary>
    /// Query the WinCompose website for update information
    /// </summary>
    private static void UpdateStatus()
    {
        try
        {
            WebClient browser = new WebClient();
            browser.Headers.Add("user-agent", GetUserAgent());
            using (Stream s = browser.OpenRead("http://wincompose.info/status.txt"))
            using (StreamReader sr = new StreamReader(s))
            {
                m_data.Clear();

                for (string line = sr.ReadLine(); line != null;  line = sr.ReadLine())
                {
                    string pattern = "^([^#: ][^: ]*):  *(.*[^ ]) *$";
                    var m = Regex.Match(line, pattern);
                    if (m.Groups.Count == 3)
                    {
                        string key = m.Groups[1].Captures[0].ToString();
                        string val = m.Groups[2].Captures[0].ToString();
                        m_data[key] = val;
                    }
                }
            }
        }
        catch(Exception) {}
    }

    private static string GetUserAgent()
    {
        return string.Format("WinCompose/{0} ({1}{2})",
                             Settings.Version,
                             Environment.OSVersion,
                             Settings.IsDebugging() ? "; Development" :
                             Settings.IsInstalled() ? "" : "; Portable");
    }

    private static Dictionary<string, string> m_data = new Dictionary<string, string>();
    private static Thread m_thread;
}

}

