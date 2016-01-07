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
    public static event EventHandler Changed = delegate {};

    private static void Run()
    {
        for (;;)
        {
            try
            {
                UpdateStatus();

                if (HasNewerVersion())
                {
                    Changed(null, new EventArgs());
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

    public static bool HasNewerVersion()
    {
        var current = SplitVersionString(Settings.Version);
        var available = Get("Latest");

        if (available != null)
            for (int i = 0; i < 4; ++i)
                if (current[i] < available[i])
                   return true;

        return false;
    }

    private static List<int> SplitVersionString(string str)
    {
        List<int> ret = new List<int>();

        foreach (var e in str.Replace("beta", ".").Split(new char[] { '.' }))
            ret.Add(int.Parse(e));

        // If fewer than 4 elements, add zeroes, except for the last one
        // which needs to be higher than any beta version
        while (ret.Count < 4)
            ret.Add(ret.Count == 3 ? int.MaxValue : 0);

        while (ret.Count > 4)
            ret.RemoveAt(ret.Count - 1);

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

