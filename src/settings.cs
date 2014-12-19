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
using System.Text.RegularExpressions;

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

        public static void LoadSequences()
        {
            LoadSequenceFile(Path.Combine(GetDataDir(), "Xorg.txt"));
            LoadSequenceFile(Path.Combine(GetDataDir(), "XCompose.txt"));
            LoadSequenceFile(Path.Combine(GetDataDir(), "WinCompose.txt"));

            LoadSequenceFile(Path.Combine(GetUserDir(), ".XCompose"));
            LoadSequenceFile(Path.Combine(GetUserDir(), ".XCompose.txt"));
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

        public static Dictionary<string, Sequence> GetSequences()
        {
            return m_sequences;
        }

        public static bool IsValidPrefix(string prefix)
        {
            return m_prefixes.ContainsKey(prefix);
        }

        public static bool IsValidSequence(string sequence)
        {
            return m_sequences.ContainsKey(sequence);
        }

        public static Sequence GetSequence(string sequence)
        {
            Sequence ret = null;
            m_sequences.TryGetValue(sequence, out ret);
            return ret;
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

        private static void LoadSequenceFile(string path)
        {
            try
            {
                foreach (string line in File.ReadAllLines(path))
                    LoadSequenceString(line);
            }
            catch (Exception) { }
        }

        private static void LoadSequenceString(string line)
        {
            // Only bother with sequences that start with <Multi_key>
            var m1 = Regex.Match(line, @"\s*<Multi_key>\s*([^:]*):[^""]*""([^""]|\"")*""[^#]*#\s*(.*)");
            //                                            ^^^^^^^         ^^^^^^^^^^^^           ^^^^
            //                                             keys              result              desc
            if (m1.Groups.Count < 3)
                return;

            var keys = Regex.Split(m1.Groups[1].Captures[0].ToString(), @"[\s<>]+");

            if (keys.Length < 4) // We need 2 keys + 2 empty strings
                return;

            Sequence seq = new Sequence();

            for (int i = 1; i < keys.Length; ++i)
            {
                if (keys[i] == String.Empty)
                    continue;
                else if (keys[i].Length == 1)
                    seq.m_keys += keys[i];
                else if (m_sc_names.ContainsKey(keys[i]))
                    seq.m_keys += (char)m_sc_names[keys[i]];
                else
                    return; // Unknown key name! Better bail out
            }

            seq.m_result = m1.Groups[2].Captures[0].ToString();
            seq.m_description = m1.Groups.Count >= 4 ? m1.Groups[3].Captures[0].ToString() : "";

            m_sequences[seq.m_keys] = seq;

            // FIXME: find what to put in there
            for (int i = 1; i < seq.m_keys.Length; ++i)
                m_prefixes[seq.m_keys.Substring(1, i)] = null;
        }

        // FIXME: this should be an acyclic graph so that we immediately
        // know whether a character is part of a valid sequence, plus we
        // could do some typing predictions!
        private static Dictionary<string, Sequence> m_sequences
         = new Dictionary<string, Sequence>();

        private static Dictionary<string, object> m_prefixes
         = new Dictionary<string, object>();


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

        private static readonly Dictionary<string, SC> m_sc_names
         = new Dictionary<string, SC>()
        {
            // ASCII-mapped keys
            { "space",        (SC)' ' },  // 0x20
            { "exclam",       (SC)'!' },  // 0x21
            { "quotedbl",     (SC)'"' },  // 0x22
            { "numbersign",   (SC)'#' },  // 0x23
            { "dollar",       (SC)'$' },  // 0x24
            { "percent",      (SC)'%' },  // 0x25
            { "ampersand",    (SC)'&' },  // 0x26
            { "apostrophe",   (SC)'\'' }, // 0x27
            { "parenleft",    (SC)'(' },  // 0x28
            { "parenright",   (SC)')' },  // 0x29
            { "asterisk",     (SC)'*' },  // 0x2a
            { "plus",         (SC)'+' },  // 0x2b
            { "comma",        (SC)',' },  // 0x2c
            { "minus",        (SC)'-' },  // 0x2d
            { "period",       (SC)'.' },  // 0x2e
            { "slash",        (SC)'/' },  // 0x2f
            { "colon",        (SC)':' },  // 0x3a
            { "semicolon",    (SC)';' },  // 0x3b
            { "less",         (SC)'<' },  // 0x3c
            { "equal",        (SC)'=' },  // 0x3d
            { "greater",      (SC)'>' },  // 0x3e
            { "question",     (SC)'?' },  // 0x3f
            { "at",           (SC)'@' },  // 0x40
            { "bracketleft",  (SC)'[' },  // 0x5b
            { "backslash",    (SC)'\\' }, // 0x5c
            { "bracketright", (SC)']' },  // 0x5d
            { "asciicircum",  (SC)'^' },  // 0x5e
            { "underscore",   (SC)'_' },  // 0x5f
            { "grave",        (SC)'`' },  // 0x60
            { "braceleft",    (SC)'{' },  // 0x7b
            { "bar",          (SC)'|' },  // 0x7c
            { "braceright",   (SC)'}' },  // 0x7d
            { "asciitilde",   (SC)'~' },  // 0x7e
        };

        private static readonly Dictionary<string, VK> m_vk_names
         = new Dictionary<string, VK>()
        {
            // Non-printing keys; but we need an internal representation
            { "up",     VK.UP },
            { "down",   VK.DOWN },
            { "left",   VK.LEFT },
            { "right",  VK.RIGHT },
        };

        private static string GetConfigFile()
        {
            return Path.Combine(GetConfigDir(), "settings.ini");
        }

        private static string GetConfigDir()
        {
            var appdata = Environment.SpecialFolder.ApplicationData;
            return IsInstalled() ? Environment.GetFolderPath(appdata)
                                 : GetExeDir();
        }

        private static string GetDataDir()
        {
            return IsInstalled() ? Path.Combine(GetExeDir(), "res")
                 : IsDebugging() ? Path.Combine(GetExeDir(), "../../res")
                 : GetExeDir();
        }

        private static string GetUserDir()
        {
            return Environment.ExpandEnvironmentVariables("%USERPROFILE%");
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
