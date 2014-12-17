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

namespace wincompose
{
    static class settings
    {
        public static void load_config()
        {
            string val;
            
            // The key used as the compose key
            val = load_config_entry("compose_key");
            if (!m_valid_compose_keys.ContainsKey(val))
                val = "ralt";
            m_compose_key = m_valid_compose_keys[val];

            // The timeout delay
            val = load_config_entry("reset_delay");

            // The interface language
            val = load_config_entry("language");
            if (!m_valid_languages.ContainsKey(val))
                val = "";
            m_language = val;

            // Various options
            val = load_config_entry("case_insensitive");
            val = load_config_entry("discard_on_invalid");
            val = load_config_entry("beep_on_invalid");

            // Save config to sanitise it
            save_config();
        }

        public static void save_config()
        {
            save_config_entry("compose_key", "ralt");
            save_config_entry("reset_delay", m_delay.ToString());
            save_config_entry("language", "");
        }

        public static bool is_compose_key(VK key)
        {
            return m_compose_key == key;
        }

        private static string load_config_entry(string key)
        {
            var tmp = new StringBuilder(255);
            GetPrivateProfileString("global", key, "", tmp, 255, get_config_file());
            return tmp.ToString();
        }

        private static void save_config_entry(string key, string val)
        {
            WritePrivateProfileString("global", key, val, get_config_file());
        }

        private static readonly Dictionary<string, VK> m_valid_compose_keys = new Dictionary<string, VK>()
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

        private static VK m_compose_key = VK.RMENU;

        private static readonly Dictionary<string, string> m_valid_languages = new Dictionary<string, string>()
        {
            { ""  , "Autodetect" },
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

        private static string m_language = "";

        private static readonly Dictionary<int, string> m_valid_delays = new Dictionary<int, string>()
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

        private static string get_config_file()
        {
            return Path.Combine(get_config_dir(), "settings.ini");
        }

        private static string get_config_dir()
        {
            return is_portable() ? "." : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        private static string get_install_dir()
        {
            return is_portable() ? "." : get_exe_dir();
        }

        private static string get_exe_dir()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(assembly.GetName().CodeBase);
        }

        private static bool is_portable()
        {
            return !File.Exists(Path.Combine(get_exe_dir(), "unins000.dat"));
        }

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);
    }
}
