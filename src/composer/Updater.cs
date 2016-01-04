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
                GetStatusData();

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
        if (m_data.ContainsKey("Latest"))
        {
            ;
        }
        return false;
    }

    /// <summary>
    /// Query the WinCompose website for update information
    /// </summary>
    private static void GetStatusData()
    {
        try
        {
            WebClient browser = new WebClient();
            browser.Headers.Add("user-agent", GetUserAgent());
            Stream s = browser.OpenRead("http://wincompose.info/status.txt");
            StreamReader sr = new StreamReader(s);

            for (string line = sr.ReadLine(); line != null;  line = sr.ReadLine())
            {
                string pattern = "^([^#: ][^: ]*):  *(.*[^ ]) *$";
                var m = Regex.Match(line, pattern);
                if (m.Groups.Count == 3)
                    m_data[m.Groups[1].Captures[0].ToString()] = m.Groups[2].Captures[0].ToString();
            }

            sr.Close();
            s.Close();
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

