//
// WinCompose — a compose key for Windows
//
// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//                     2014 Benjamin Litzelmann
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What the Fuck You Want
//   to Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WinCompose
{
    public static class Settings
    {
        private const string GlobalSection = "global";
        private const string ConfigFileName = "settings.ini";
        private static FileSystemWatcher watcher;
        private static Timer reloadTimer;

        static Settings()
        {
            ComposeKey = new SettingsEntry<Key>(GlobalSection, "compose_key", m_default_compose_key);
            ResetDelay = new SettingsEntry<int>(GlobalSection, "reset_delay", -1);
            CaseInsensitive = new SettingsEntry<bool>(GlobalSection, "case_insensitive", false);
            DiscardOnInvalid = new SettingsEntry<bool>(GlobalSection, "discard_on_invalid", false);
            BeepOnInvalid = new SettingsEntry<bool>(GlobalSection, "beep_on_invalid", false);
            KeepOriginalKey = new SettingsEntry<bool>(GlobalSection, "keep_original_key", false);
        }

        public static SettingsEntry<Key> ComposeKey { get; private set; }

        public static SettingsEntry<int> ResetDelay { get; private set; }

        public static SettingsEntry<bool> CaseInsensitive { get; private set; }

        public static SettingsEntry<bool> DiscardOnInvalid { get; private set; }
        
        public static SettingsEntry<bool> BeepOnInvalid { get; private set; }

        public static SettingsEntry<bool> KeepOriginalKey { get; private set; }

        public static IEnumerable<Key> ValidComposeKeys { get { return m_valid_compose_keys; } }

        public static void StartWatchConfigFile()
        {
            watcher = new FileSystemWatcher(GetConfigDir(), ConfigFileName);
            watcher.Changed += ConfigFileChanged;
            watcher.EnableRaisingEvents = true;
        }

        public static void StopWatchConfigFile()
        {
            watcher.Dispose();
            watcher = null;
        }

        private static void ConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            if (reloadTimer == null)
            {
                // This event is triggered multiple times.
                // Let's defer its handling to reload the config only once.
                reloadTimer = new Timer(ReloadConfig, null, 300, Timeout.Infinite);
            }
        }

        private static void ReloadConfig(object state)
        {
            reloadTimer.Dispose();
            reloadTimer = null;
            LoadConfig();
        }

        public static void LoadConfig()
        {
            string val;

            // The key used as the compose key
            ComposeKey.Load();

            if (!m_valid_compose_keys.Contains(ComposeKey.Value))
                ComposeKey.Value = m_default_compose_key;

            // The timeout delay
            ResetDelay.Load();

            // The interface language
            // TODO: language settings
            val = LoadEntry("language");
            if (m_valid_languages.ContainsKey(val))
                m_language = val;
            else
                m_language = m_default_language;

            // Various options
            CaseInsensitive.Load();
            DiscardOnInvalid.Load();
            BeepOnInvalid.Load();
            KeepOriginalKey.Load();
        }

        public static void SaveConfig()
        {
            SaveEntry("reset_delay", m_delay.ToString());
            SaveEntry("language", m_language);
            ComposeKey.Save();
            CaseInsensitive.Save();
            DiscardOnInvalid.Save();
            BeepOnInvalid.Save();
            KeepOriginalKey.Save();
        }

        public static void LoadSequences()
        {
            LoadSequenceFile(Path.Combine(GetDataDir(), "Xorg.txt"));
            LoadSequenceFile(Path.Combine(GetDataDir(), "XCompose.txt"));
            LoadSequenceFile(Path.Combine(GetDataDir(), "WinCompose.txt"));

            LoadSequenceFile(Path.Combine(GetUserDir(), ".XCompose"));
            LoadSequenceFile(Path.Combine(GetUserDir(), ".XCompose.txt"));
        }

        public static bool IsComposeKey(Key key)
        {
            return ComposeKey.Value == key;
        }

        public static bool IsUsableKey(Key key)
        {
            return key.IsPrintable() || m_key_names.ContainsValue(key);
        }

        public static SequenceTree GetSequenceList()
        {
            return m_sequences;
        }

        public static bool IsValidPrefix(List<Key> sequence)
        {
            return m_sequences.IsValidPrefix(sequence);
        }

        public static bool IsValidSequence(List<Key> sequence)
        {
            return m_sequences.IsValidSequence(sequence);
        }

        public static string GetSequenceResult(List<Key> sequence)
        {
            return m_sequences.GetSequenceResult(sequence);
        }

        public static List<SequenceDescription> GetSequenceDescriptions()
        {
            return m_sequences.GetSequenceDescriptions();
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
            var m1 = Regex.Match(line, @"^\s*<Multi_key>\s*([^:]*):[^""]*""(([^""]|\"")*)""[^#]*#\s*(.*)");
            //                                             ^^^^^^^         ^^^^^^^^^^^^^^           ^^^^
            //                                              keys              result                desc
            if (m1.Groups.Count < 4)
                return;

            var keys = Regex.Split(m1.Groups[1].Captures[0].ToString(), @"[\s<>]+");

            if (keys.Length < 4) // We need 2 keys + 2 empty strings
                return;

            List<Key> seq = new List<Key>();

            for (int i = 1; i < keys.Length; ++i)
            {
                if (keys[i] == String.Empty)
                    continue;

                if (m_key_names.ContainsKey(keys[i]))
                    seq.Add(m_key_names[keys[i]]);
                else if (keys[i].Length == 1)
                    seq.Add(new Key(keys[i]));
                else
                    return; // Unknown key name! Better bail out
            }

            string result = m1.Groups[2].Captures[0].ToString();
            string description = m1.Groups.Count >= 5 ? m1.Groups[4].Captures[0].ToString() : "";

            m_sequences.Add(seq, result, description);
        }

        // Tree of all known sequences
        private static SequenceTree m_sequences = new SequenceTree();


        private static readonly List<Key> m_valid_compose_keys = new List<Key>
        {
           new Key(VK.LMENU),
           new Key(VK.RMENU),
           new Key(VK.LCONTROL),
           new Key(VK.RCONTROL),
           new Key(VK.LWIN),
           new Key(VK.RWIN),
           new Key(VK.CAPITAL),
           new Key(VK.NUMLOCK),
           new Key(VK.PAUSE),
           new Key(VK.APPS),
           new Key(VK.ESCAPE),
           new Key(VK.SCROLL),
           new Key("`"),
        };

        private static readonly Key m_default_compose_key = new Key(VK.RMENU);

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

        private static int m_delay = -1;

        private static readonly Dictionary<string, Key> m_key_names
         = new Dictionary<string, Key>()
        {
            // ASCII-mapped keys
            { "space",        new Key(" ") },  // 0x20
            { "exclam",       new Key("!") },  // 0x21
            { "quotedbl",     new Key("\"") }, // 0x22
            { "numbersign",   new Key("#") },  // 0x23
            { "dollar",       new Key("$") },  // 0x24
            { "percent",      new Key("%") },  // 0x25
            { "ampersand",    new Key("&") },  // 0x26
            { "apostrophe",   new Key("'") },  // 0x27
            { "parenleft",    new Key("(") },  // 0x28
            { "parenright",   new Key(")") },  // 0x29
            { "asterisk",     new Key("*") },  // 0x2a
            { "plus",         new Key("+") },  // 0x2b
            { "comma",        new Key(",") },  // 0x2c
            { "minus",        new Key("-") },  // 0x2d
            { "period",       new Key(".") },  // 0x2e
            { "slash",        new Key("/") },  // 0x2f
            { "colon",        new Key(":") },  // 0x3a
            { "semicolon",    new Key(";") },  // 0x3b
            { "less",         new Key("<") },  // 0x3c
            { "equal",        new Key("=") },  // 0x3d
            { "greater",      new Key(">") },  // 0x3e
            { "question",     new Key("?") },  // 0x3f
            { "at",           new Key("@") },  // 0x40
            { "bracketleft",  new Key("[") },  // 0x5b
            { "backslash",    new Key("\\") }, // 0x5c
            { "bracketright", new Key("]") },  // 0x5d
            { "asciicircum",  new Key("^") },  // 0x5e
            { "underscore",   new Key("_") },  // 0x5f
            { "grave",        new Key("`") },  // 0x60
            { "braceleft",    new Key("{") },  // 0x7b
            { "bar",          new Key("|") },  // 0x7c
            { "braceright",   new Key("}") },  // 0x7d
            { "asciitilde",   new Key("~") },  // 0x7e

            // Non-printing keys
            { "up",     new Key(VK.UP) },
            { "down",   new Key(VK.DOWN) },
            { "left",   new Key(VK.LEFT) },
            { "right",  new Key(VK.RIGHT) },
        };

        public static string GetConfigFile()
        {
            return Path.Combine(GetConfigDir(), ConfigFileName);
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
