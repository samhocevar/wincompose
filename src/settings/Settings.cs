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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Xml;

namespace WinCompose
{
    public static class Settings
    {
        private const string ConfigFileName = "settings.ini";
        private static FileSystemWatcher m_watcher;
        private static Timer m_reload_timer;

        private static readonly Mutex m_mutex = new Mutex(false,
                          "wincompose-{1342C5FF-9483-45F3-BE0C-1C8D63CEA81C}");

        private class EntryLocation : Attribute
        {
            public EntryLocation(string section, string key)
            {
                Section = section;
                Key = key;
            }

            public readonly string Section, Key;
        }

        static Settings()
        {
            // Add a save handler to our EntryLocation attributes
            foreach (var v in typeof(Settings).GetProperties())
            {
                foreach (EntryLocation attr in v.GetCustomAttributes(typeof(EntryLocation), true))
                {
                    var entry = v.GetValue(null, null) as SettingsEntry;
                    entry.ValueChanged += () => ThreadPool.QueueUserWorkItem(_ => SaveEntry(entry.ToString(), attr.Section, attr.Key));
                }
            }
        }

        // The application version
        public static string Version
        {
            get
            {
                if (m_version == null)
                {
                    var doc = new XmlDocument();
                    doc.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("WinCompose.build.config"));
                    var mgr = new XmlNamespaceManager(doc.NameTable);
                    mgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

                    m_version = doc.DocumentElement.SelectSingleNode("//ns:Project/ns:PropertyGroup/ns:ApplicationVersion", mgr).InnerText;
                }

                return m_version;
            }
        }
        private static string m_version;

        [EntryLocation("global", "language")]
        public static SettingsEntry<string> Language { get; } = new SettingsEntry<string>("");
        [EntryLocation("global", "disabled")]
        public static SettingsEntry<bool> Disabled { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("global", "check_updates")]
        public static SettingsEntry<bool> CheckUpdates { get; } = new SettingsEntry<bool>(true);

        [EntryLocation("composing", "compose_key")]
        public static SettingsEntry<KeySequence> ComposeKeys { get; } = new SettingsEntry<KeySequence>(new KeySequence());
        [EntryLocation("composing", "reset_delay")]
        public static SettingsEntry<int> ResetDelay { get; } = new SettingsEntry<int>(-1);
        [EntryLocation("composing", "unicode_input")]
        public static SettingsEntry<bool> UnicodeInput { get; } = new SettingsEntry<bool>(true);
        [EntryLocation("composing", "case_insensitive")]
        public static SettingsEntry<bool> CaseInsensitive { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("composing", "discard_on_invalid")]
        public static SettingsEntry<bool> DiscardOnInvalid { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("composing", "beep_on_invalid")]
        public static SettingsEntry<bool> BeepOnInvalid { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("composing", "keep_original_key")]
        public static SettingsEntry<bool> KeepOriginalKey { get; } = new SettingsEntry<bool>(false);

        [EntryLocation("tweaks", "insert_zwsp")]
        public static SettingsEntry<bool> InsertZwsp { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("tweaks", "emulate_capslock")]
        public static SettingsEntry<bool> EmulateCapsLock { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("tweaks", "shift_disables_capslock")]
        public static SettingsEntry<bool> ShiftDisablesCapsLock { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("tweaks", "capslock_capitalizes")]
        public static SettingsEntry<bool> CapsLockCapitalizes { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("tweaks", "allow_injected")]
        public static SettingsEntry<bool> AllowInjected { get; } = new SettingsEntry<bool>(false);

        public static IEnumerable<Key> ValidComposeKeys => m_valid_compose_keys;
        public static Dictionary<string, string> ValidLanguages => m_valid_languages;

        public static void StartWatchConfigFile()
        {
            if (CreateConfigDir())
            {
                m_watcher = new FileSystemWatcher(GetConfigDir(), ConfigFileName);
                m_watcher.NotifyFilter = NotifyFilters.LastWrite;
                m_watcher.Changed += ConfigFileChanged;
                m_watcher.EnableRaisingEvents = true;
            }
        }

        public static void StopWatchConfigFile()
        {
            if (m_watcher != null)
            {
                m_watcher.Dispose();
                m_watcher = null;
            }
        }

        private static void ConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            if (m_reload_timer == null)
            {
                // This event is triggered multiple times.
                // Let's defer its handling to reload the config only once.
                Log.Debug("Configuration file changed, scheduling reload.");
                m_reload_timer = new Timer(ReloadConfig, null, 300, Timeout.Infinite);
            }
        }

        private static void ReloadConfig(object state)
        {
            m_reload_timer.Dispose();
            m_reload_timer = null;
            LoadConfig();
        }

        private static void ValidateComposeKeys()
        {
            // Validate the list of compose keys, ensuring there are only valid keys
            // and there are no duplicates. Also remove VK.DISABLED from the list
            // unless there are no valid keys at all.
            KeySequence compose_keys = new KeySequence();
            foreach (Key k in ComposeKeys.Value ?? compose_keys)
            {
                bool is_valid = (k.VirtualKey >= VK.F1 && k.VirtualKey <= VK.F24)
                                 || m_valid_compose_keys.Contains(k);
                if (is_valid && k.VirtualKey != VK.DISABLED && !compose_keys.Contains(k))
                    compose_keys.Add(k);
            }

            if (compose_keys.Count == 0)
                compose_keys.Add(new Key(VK.DISABLED));
            ComposeKeys.Value = compose_keys;
        }

        public static void LoadConfig()
        {
            Log.Debug($"Reloading configuration file {GetConfigFile()}");

            foreach (var v in typeof(Settings).GetProperties())
            {
                foreach (EntryLocation attr in v.GetCustomAttributes(typeof(EntryLocation), true))
                {
                    var entry = v.GetValue(null, null) as SettingsEntry;
                    LoadEntry(entry, attr.Section, attr.Key);
                }
            }

            ValidateComposeKeys();

            // HACK: if the user uses the "it-CH" locale, replace it with "it"
            // because we use "it-CH" as a special value to mean Sardinian.
            // The reason is that apparently we cannot define a custom
            // CultureInfo without registering it, and we cannot register it
            // without administrator privileges.
            // Same with "de-CH" which we use for Esperanto, and "be-BY" which
            // we use for Latin Belarus… this is ridiculous, Microsoft.
            if (Thread.CurrentThread.CurrentUICulture.Name == "it-CH"
                 || Thread.CurrentThread.CurrentUICulture.Name == "de-CH"
                 || Thread.CurrentThread.CurrentUICulture.Name == "be-BY")
            {
                try
                {
                    Thread.CurrentThread.CurrentUICulture
                        = Thread.CurrentThread.CurrentUICulture.Parent;
                }
                catch (Exception) { }
            }

            if (Language.Value != "")
            {
                if (m_valid_languages.ContainsKey(Language.Value))
                {
                    try
                    {
                        var ci = CultureInfo.GetCultureInfo(Language.Value);
                        Thread.CurrentThread.CurrentUICulture = ci;
                    }
                    catch (Exception) { }
                }
                else
                {
                    Language.Value = "";
                }
            }

            // Catch-22: we can only add this string when the UI language is known
            m_valid_languages[""] = i18n.Text.AutodetectLanguage;
        }

        public static void SaveConfig()
        {
            Log.Debug($"Saving configuration file {GetConfigFile()}");

            foreach (var v in typeof(Settings).GetProperties())
            {
                foreach (EntryLocation attr in v.GetCustomAttributes(typeof(EntryLocation), true))
                {
                    var entry = v.GetValue(null, null) as SettingsEntry;
                    SaveEntry(entry.ToString(), attr.Section, attr.Key);
                }
            }

            SaveEntry(m_delay.ToString(), "composing", "reset_delay");
        }

        public static void LoadSequences()
        {
            m_sequences = new SequenceTree();

            m_sequences.LoadResource("3rdparty.xorg.rules");
            m_sequences.LoadResource("3rdparty.xcompose.rules");

            m_sequences.LoadFile(Path.Combine(GetDataDir(), "Emoji.txt"));
            m_sequences.LoadFile(Path.Combine(GetDataDir(), "WinCompose.txt"));

            m_sequences.LoadFile(Path.Combine(GetUserDir(), ".XCompose"));
            m_sequences.LoadFile(Path.Combine(GetUserDir(), ".XCompose.txt"));
        }

        /// <summary>
        /// Find the preferred application for .txt files and launch it to
        /// open .XCompose. If the file does not exist, try .XCompose.txt
        /// instead. If it doesn’t exist either, create .XCompose ourselves.
        /// </summary>
        public static void EditCustomRulesFile()
        {
            // Ensure the rules file exists.
            string user_file = Path.Combine(GetUserDir(), ".XCompose");
            if (!File.Exists(user_file))
            {
                string alt_file = Path.Combine(GetUserDir(), ".XCompose.txt");
                if (File.Exists(alt_file))
                {
                    user_file = alt_file;
                }
                else
                {
                    var text = File.ReadAllText(Path.Combine(GetDataDir(), "DefaultUserSequences.txt"), Encoding.UTF8);
                    var replacedText = text.Replace("%DataDir%", GetDataDir());
                    File.WriteAllText(user_file, replacedText, Encoding.UTF8);
                }
            }

            // Find the preferred application for .txt files
            HRESULT ret;
            uint length = 0;
            ret = NativeMethods.AssocQueryString(ASSOCF.NONE,
                            ASSOCSTR.EXECUTABLE, ".txt", null, null, ref length);
            if (ret != HRESULT.S_FALSE)
                return;

            var sb = new StringBuilder((int)length);
            ret = NativeMethods.AssocQueryString(ASSOCF.NONE,
                            ASSOCSTR.EXECUTABLE, ".txt", null, sb, ref length);
            if (ret != HRESULT.S_OK)
                return;

            // Open the rules file with that application
            var psinfo = new ProcessStartInfo
            {
                FileName = sb.ToString(),
                Arguments = user_file,
                UseShellExecute = true,
            };
            Process.Start(psinfo);
        }

        public static int SequenceCount => m_sequences.Count;

        public static SequenceTree GetSequenceList() => m_sequences;
        public static bool IsValidPrefix(KeySequence sequence, bool ignore_case) => m_sequences.IsValidPrefix(sequence, ignore_case);
        public static bool IsValidSequence(KeySequence sequence, bool ignore_case) => m_sequences.IsValidSequence(sequence, ignore_case);
        public static string GetSequenceResult(KeySequence sequence, bool ignore_case) => m_sequences.GetSequenceResult(sequence, ignore_case);
        public static List<SequenceDescription> GetSequenceDescriptions() => m_sequences.GetSequenceDescriptions();

        private static void LoadEntry(SettingsEntry entry, string section, string key)
        {
            try
            {
                if (!m_mutex.WaitOne(2000))
                    return;
            }
            catch (AbandonedMutexException)
            {
                /* Ignore; this might be a previous instance that crashed */
            }

            try
            {
                const int len = 255;
                var migrated = false;
                var tmp = new StringBuilder(len);
                var result = NativeMethods.GetPrivateProfileString(section, key, "",
                                                                   tmp, len, GetConfigFile());
                if (result == 0)
                {
                    // Compatibility code for keys that moved from the "global"
                    // to the "composing" or "tweaks" section.
                    if (section != "global")
                    {
                        result = NativeMethods.GetPrivateProfileString("global", key, "",
                                                                       tmp, len, GetConfigFile());
                        if (result == 0)
                            return;
                        migrated = true;
                    }
                }

                entry.LoadString(tmp.ToString());

                if (migrated)
                {
                    NativeMethods.WritePrivateProfileString("global", key, null,
                                                            GetConfigFile());
                    NativeMethods.WritePrivateProfileString(section, key, entry.ToString(),
                                                            GetConfigFile());
                }
            }
            finally
            {
                // Ensure the mutex is always released even if an
                // exception is thrown
                m_mutex.ReleaseMutex();
            }
        }

        private static void SaveEntry(string value, string section, string key)
        {
            if (CreateConfigDir())
            {
                try
                {
                    if (!m_mutex.WaitOne(2000))
                        return;
                }
                catch (AbandonedMutexException)
                {
                    /* Ignore; this might be a previous instance that crashed */
                }

                try
                {
                    Log.Debug($"Saving {section}.{key} = {value}");
                    NativeMethods.WritePrivateProfileString(section, key, value,
                                                            GetConfigFile());
                    // Ensure old keys are removed from the global section
                    if (section != "global")
                        NativeMethods.WritePrivateProfileString("global", key, null,
                                                                GetConfigFile());
                }
                finally
                {
                    // Ensure the mutex is always released even if an
                    // exception is thrown
                    m_mutex.ReleaseMutex();
                }
            }
        }

        // Tree of all known sequences
        private static SequenceTree m_sequences = new SequenceTree();

        // FIXME: couldn't we accept any compose key?
        private static readonly KeySequence m_valid_compose_keys = new KeySequence
        {
           new Key(VK.DISABLED),
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
           new Key(VK.CONVERT),
           new Key(VK.NONCONVERT),
           new Key(VK.INSERT),
           new Key(VK.PRINT),
           new Key(VK.SCROLL),
           new Key("`"),
        };

        private static readonly Key m_default_compose_key = new Key(VK.RMENU);

        private static readonly
        Dictionary<string, string> m_valid_languages = GetSupportedLanguages();

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

        private static Dictionary<string, string> GetSupportedLanguages()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>()
            {
                { "en", "English" },
            };

            // Enumerate all languages that have an embedded resource file version
            ResourceManager rm = new ResourceManager(typeof(i18n.Text));
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (CultureInfo ci in cultures)
            {
                string name = ci.Name;
                string native_name = ci.TextInfo.ToTitleCase(ci.NativeName);

                if (name != "") try
                {
                    if (rm.GetResourceSet(ci, true, false) != null)
                    {
                        // HACK: second part of our hack to support Sardinian,
                        // Esperanto and Latin Belarusian.
                        if (name == "it-CH")
                            native_name = "Sardu";
                        if (name == "de-CH")
                            native_name = "Esperanto";
                        if (name == "be-BY")
                            native_name = "Belarusian (latin)";

                        ret.Add(name, native_name);
                    }
                }
                catch (Exception) {}
            }

            return ret;
        }

        public static string GetConfigFile()
        {
            return Path.Combine(GetConfigDir(), ConfigFileName);
        }

        public static bool CreateConfigDir()
        {
            string config_dir = GetConfigDir();
            if (!Directory.Exists(config_dir))
            {
                try
                {
                    Directory.CreateDirectory(config_dir);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        private static string GetConfigDir()
        {
            var appdata = Environment.SpecialFolder.ApplicationData;
            var appdatadir = Path.Combine(Environment.GetFolderPath(appdata),
                                          "WinCompose");
            return IsInstalled() ? appdatadir : GetExeDir();
        }

        private static string GetDataDir()
        {
            return IsInstalled() ? Path.Combine(GetExeDir(), "res")
                 : IsDebugging() ? Path.Combine(GetExeDir(), "../../rules")
                 : Path.Combine(GetExeDir(), "rules");
        }

        public static string GetUserDir()
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

        public static bool IsInstalled()
        {
            return File.Exists(Path.Combine(GetExeDir(), "unins000.dat"));
        }

        public static bool IsDebugging()
        {
            string exe = GetExeName();
            return File.Exists(Path.ChangeExtension(exe, ".pdb"));
        }
    }
}

