//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2016 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace WinCompose
{
    static class SysTray
    {
        private const int HEADER_SIZE = 20;
        private const int ENTRY_SIZE = 1640;
        private const int MAGIC_OFFSET = 528;

        /// <summary>
        /// Check that the systray icon for the program is marked as “always show”
        /// and restart explorer if any changes were made. The argument is a regex
        /// used to match against the executable path.
        /// </summary>
        public static void AlwaysShow(string pattern)
        {
            string key_path = @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\TrayNotify";
            string key_name = "IconStreams";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(key_path, true);
            byte[] data = (byte[])key.GetValue(key_name, null);

            bool has_changed = false;

            // This key should have a 20-byte header and several 1640-byte entries
            if (data != null && data.Length % ENTRY_SIZE == HEADER_SIZE)
            {
                for (int offset = HEADER_SIZE; offset < data.Length; offset += ENTRY_SIZE)
                {
                    // This icon is already configured to be always displayed,
                    // we can just ignore it.
                    if (data[offset + MAGIC_OFFSET] == 2)
                        continue;

                    // Retrieve the executable name, which is all the data before
                    // our magic value.
                    string application_path = "";
                    for (int i = 0; i < MAGIC_OFFSET; i += 2)
                    {
                        int ch = data[offset + i] + 16 * data[offset + i + 1];
                        if (ch == 0)
                            break;

                        if (ch >= 'a' && ch <= 'm' || ch >= 'A' && ch <= 'M')
                            ch += 13;
                        else if (ch >= 'n' && ch <= 'z' || ch >= 'N' && ch <= 'Z')
                            ch -= 13;

                        application_path += (char)ch;
                    }

                    if (Regex.Match(application_path, pattern, RegexOptions.IgnoreCase).Success)
                    {
                        Log.Debug("Enforcing SysTray visibility for {0}", application_path);
                        data[offset + MAGIC_OFFSET] = 2;
                        has_changed = true;
                    }
                }

                if (has_changed)
                {
                    key.SetValue(key_name, data);

                    bool must_restart = false;

                    foreach (var process in Process.GetProcessesByName("explorer"))
                    {
                        process.Kill();
                        must_restart = true;
                    }

                    if (must_restart)
                        Process.Start("explorer.exe");
                }
            }

            key.Close();
        }
    }
}

