// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//   This program is free software; you can redistribute it and/or
//   modify it under the terms of the Do What The Fuck You Want To
//   Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.using System;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace WinCompose
{
    static class Settings
    {
        public static void LoadConfig()
        {
            string val;

            // The key used as the compose key
            val = LoadEntry("compose_key");
            if (m_valid_compose_keys.ContainsKey(val))
                m_compose_key = m_valid_compose_keys[val];
            else
                m_compose_key = m_default_compose_key;

            // The timeout delay
            val = LoadEntry("reset_delay");

            // The interface language
            val = LoadEntry("language");
            if (m_valid_languages.ContainsKey(val))
                m_language = val;
            else
                m_language = m_default_language;

            // Various options
            val = LoadEntry("case_insensitive");
            m_case_insensitive = (val == "true");

            val = LoadEntry("discard_on_invalid");
            m_discard_on_invalid = (val == "true");

            val = LoadEntry("beep_on_invalid");
            m_beep_on_invalid = (val == "true");

            // Save config to sanitise it
            SaveConfig();
        }

        public static void SaveConfig()
        {
            foreach (var entry in m_valid_compose_keys)
                if (entry.Value == m_compose_key)
                    SaveEntry("compose_key", entry.Key);

            SaveEntry("reset_delay", m_delay.ToString());
            SaveEntry("language", m_language);
            SaveEntry("case_insensitive", m_case_insensitive);
            SaveEntry("discard_on_invalid", m_discard_on_invalid);
            SaveEntry("beep_on_invalid", m_beep_on_invalid);
        }

        public static bool IsComposeKey(VK key)
        {
            return m_compose_key == key;
        }

        public static bool IsCaseInsensitive()
        {
            return m_case_insensitive;
        }

        public static bool ShouldDiscardOnInvalid()
        {
            return m_discard_on_invalid;
        }

        public static bool ShouldBeepOnInvalid()
        {
            return m_beep_on_invalid;
        }

        private static string LoadEntry(string key)
        {
            const int len = 255;
            var tmp = new StringBuilder(len);
            GetPrivateProfileString("global", key, "",
                                    tmp, len, GetConfigFile());
            return tmp.ToString();
        }

        private static void SaveEntry(string key, string val)
        {
            WritePrivateProfileString("global", key, val, GetConfigFile());
        }

        private static void SaveEntry(string key, int val)
        {
            SaveEntry(key, val.ToString());
        }

        private static void SaveEntry(string key, bool val)
        {
            SaveEntry(key, val ? "true" : "false");
        }

        private static readonly Dictionary<string, VK> m_valid_compose_keys
         = new Dictionary<string, VK>()
        {
            { "lalt",       VK.LMENU },
            { "ralt",       VK.RMENU },
            { "lcontrol",   VK.LCONTROL },
            { "rcontrol",   VK.RCONTROL },
            { "lwin",       VK.LWIN },
            { "rwin",       VK.RWIN },
            { "capslock",   VK.CAPITAL },
            { "numlock",    VK.NUMLOCK },
            { "pause",      VK.PAUSE },
            { "appskey",    VK.APPS },
            { "esc",        VK.ESCAPE },
            { "scrolllock", VK.SCROLL },
        };

        private static readonly VK m_default_compose_key = VK.RMENU;
        private static VK m_compose_key = m_default_compose_key;

        private static readonly Dictionary<string, string> m_valid_languages
         = new Dictionary<string, string>()
        {
            { "",   "Autodetect" },
            { "be", "Беларуская" },
            { "cs", "Čeština" },
            { "da", "Dansk" },
            { "de", "Deutsch" },
            { "el", "Ελληνικά" },
            { "en", "English" },
            { "es", "Español" },
            { "et", "Eesti" },
            { "fi", "Suomi" },
            { "fr", "Français" },
            { "id", "Bahasa Indonesia" },
            { "nl", "Nederlands" },
            { "pl", "Polski" },
            { "ru", "Русский" },
            { "sc", "Sardu" },
            { "sv", "Svenska" },
        };

        private static readonly string m_default_language = "";
        private static string m_language = m_default_language;

        private static readonly Dictionary<int, string> m_valid_delays
         = new Dictionary<int, string>()
        {
            { 500,   "500 milliseconds" },
            { 1000,  "1 second" },
            { 2000,  "2 seconds" },
            { 3000,  "3 seconds" },
            { 5000,  "5 seconds" },
            { 10000, "10 seconds" },
            { -1,    "None" },
        };

        private static int m_delay = 500;

        private static bool m_case_insensitive = false;
        private static bool m_discard_on_invalid = false;
        private static bool m_beep_on_invalid = false;

        private static string GetConfigFile()
        {
            return Path.Combine(GetConfigDir(), "settings.ini");
        }

        private static string GetConfigDir()
        {
            var appdata = Environment.SpecialFolder.ApplicationData;
            return IsInstalled() ? Environment.GetFolderPath(appdata) : ".";
        }

        private static string GetDataDir()
        {
            return IsInstalled() ? GetExeDir()
                 : IsDebugging() ? "../../res"
                 : ".";
        }

        private static string GetExeName()
        {
            var codebase = Assembly.GetExecutingAssembly().GetName().CodeBase;
            return Uri.UnescapeDataString(new UriBuilder(codebase).Path);
        }

        private static string GetExeDir()
        {
            return Path.GetDirectoryName(GetExeName());
        }

        private static bool IsInstalled()
        {
            return File.Exists(Path.Combine(GetExeDir(), "unins000.dat"));
        }

        private static bool IsDebugging()
        {
            string exe = GetExeName();
            var lol = Path.ChangeExtension(exe, ".vshost.exe");
            return File.Exists(Path.ChangeExtension(exe, ".vshost.exe"));
        }

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string Section,
                                    string Key, string Value, string FilePath);
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, string Key,
              string Default, StringBuilder RetVal, int Size, string FilePath);
    }
}
