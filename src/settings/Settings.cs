//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace WinCompose
{
    public static class Settings
    {
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
            m_ini_file = new IniFile(Path.Combine(Utils.AppDataDir, "settings.ini"));

            // Add a save handler to our EntryLocation attributes
            foreach (var v in typeof(Settings).GetProperties())
            {
                foreach (EntryLocation attr in v.GetCustomAttributes(typeof(EntryLocation), true))
                {
                    var entry = v.GetValue(null, null) as SettingsEntry;
                    entry.ValueChanged += () => ThreadPool.QueueUserWorkItem(_ => m_ini_file.SaveEntry(entry.ToString(), attr.Section, attr.Key));
                }
            }

            // Reload all sequences when these values change
            UseXorgRules.ValueChanged += () => LoadSequences();
            UseXComposeRules.ValueChanged += () => LoadSequences();
            UseEmojiRules.ValueChanged += () => LoadSequences();

            // Refresh the window properties when this value changes
            IgnoreRegex.ValueChanged += () => KeyboardLayout.Window.Refresh();
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
        [EntryLocation("composing", "use_xorg_rules")]
        public static SettingsEntry<bool> UseXorgRules { get; } = new SettingsEntry<bool>(true);
        [EntryLocation("composing", "use_xcompose_rules")]
        public static SettingsEntry<bool> UseXComposeRules { get; } = new SettingsEntry<bool>(true);
        [EntryLocation("composing", "use_emoji_rules")]
        public static SettingsEntry<bool> UseEmojiRules { get; } = new SettingsEntry<bool>(true);
        [EntryLocation("composing", "unicode_input")]
        public static SettingsEntry<bool> UnicodeInput { get; } = new SettingsEntry<bool>(true);
        [EntryLocation("composing", "case_insensitive")]
        public static SettingsEntry<bool> CaseInsensitive { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("composing", "discard_on_invalid")]
        public static SettingsEntry<bool> DiscardOnInvalid { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("composing", "swap_on_invalid")]
        public static SettingsEntry<bool> SwapOnInvalid { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("composing", "beep_on_invalid")]
        public static SettingsEntry<bool> BeepOnInvalid { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("composing", "keep_original_key")]
        public static SettingsEntry<bool> KeepOriginalKey { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("composing", "always_compose")]
        public static SettingsEntry<bool> AlwaysCompose { get; } = new SettingsEntry<bool>(false);

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
        [EntryLocation("tweaks", "keep_icon_visible")]
        public static SettingsEntry<bool> KeepIconVisible { get; } = new SettingsEntry<bool>(false);
        [EntryLocation("tweaks", "disable_icon")]
        public static SettingsEntry<bool> DisableIcon { get; } = new SettingsEntry<bool>(false);

        [EntryLocation("advanced", "ignore_regex")]
        public static SettingsEntry<string> IgnoreRegex { get; } = new SettingsEntry<string>("");

        public static IEnumerable<Key> ValidComposeKeys => m_valid_compose_keys;
        public static Dictionary<string, string> ValidLanguages => m_valid_languages;

        public static void StartWatchConfigFile()
        {
            m_ini_file.OnFileChanged += LoadConfig;
        }

        public static void StopWatchConfigFile()
        {
            m_ini_file.OnFileChanged -= LoadConfig;
            m_ini_file.Dispose();
        }

        private static IniFile m_ini_file;

        private static void ValidateComposeKeys()
        {
            KeySequence compose_keys = new KeySequence();
            if (ComposeKeys.Value?.Count == 0)
            {
                // The default compose key for the first time WinCompose is launched
                compose_keys.Add(m_default_compose_key);
            }
            else
            {
                // Validate the list of compose keys, ensuring there are only valid keys
                // and there are no duplicates. Also remove VK.DISABLED from the list
                // but re-add it if there are no valid keys at all.
                foreach (Key k in ComposeKeys.Value)
                {
                    bool is_valid = (k.VirtualKey >= VK.F1 && k.VirtualKey <= VK.F24)
                                     || m_valid_compose_keys.Contains(k);
                    if (is_valid && k.VirtualKey != VK.DISABLED && !compose_keys.Contains(k))
                        compose_keys.Add(k);
                }

                if (compose_keys.Count == 0)
                    compose_keys.Add(new Key(VK.DISABLED));
            }
            ComposeKeys.Value = compose_keys;
        }

        public static void LoadConfig()
        {
            Log.Debug($"Reloading configuration file {m_ini_file.FullPath}");

            foreach (var v in typeof(Settings).GetProperties())
            {
                foreach (EntryLocation attr in v.GetCustomAttributes(typeof(EntryLocation), true))
                {
                    var entry = v.GetValue(null, null) as SettingsEntry;
                    m_ini_file.LoadEntry(entry, attr.Section, attr.Key);
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
            Log.Debug($"Saving configuration file {m_ini_file.FullPath}");

            foreach (var v in typeof(Settings).GetProperties())
            {
                foreach (EntryLocation attr in v.GetCustomAttributes(typeof(EntryLocation), true))
                {
                    var entry = v.GetValue(null, null) as SettingsEntry;
                    m_ini_file.SaveEntry(entry.ToString(), attr.Section, attr.Key);
                }
            }
        }

        public static void LoadSequences()
        {
            m_sequences.Clear();

            if (UseXorgRules.Value)
                m_sequences.LoadResource("3rdparty.xorg.rules");
            if (UseXComposeRules.Value)
                m_sequences.LoadResource("3rdparty.xcompose.rules");
            if (UseEmojiRules.Value)
            {
                m_sequences.LoadFile(Path.Combine(Utils.DataDir, "Emoji.txt"));
                m_sequences.LoadFile(Path.Combine(Utils.DataDir, "WinCompose.txt"));
            }

            m_sequences.LoadFile(Path.Combine(Utils.UserDir, ".XCompose"));
            m_sequences.LoadFile(Path.Combine(Utils.UserDir, ".XCompose.txt"));
        }

        /// <summary>
        /// Find the preferred application for .txt files and launch it to
        /// open .XCompose. If the file does not exist, try .XCompose.txt
        /// instead. If it doesn’t exist either, create .XCompose ourselves.
        /// </summary>
        public static void EditCustomRulesFile()
        {
            // Ensure the rules file exists.
            string user_file = Path.Combine(Utils.UserDir, ".XCompose");
            if (!File.Exists(user_file))
            {
                string alt_file = Path.Combine(Utils.UserDir, ".XCompose.txt");
                string default_file = Path.Combine(Utils.DataDir, "DefaultUserSequences.txt");
                if (File.Exists(alt_file))
                {
                    user_file = alt_file;
                }
                else if (File.Exists(default_file))
                {
                    var text = File.ReadAllText(default_file);
                    var replacedText = text.Replace("%DataDir%", Utils.DataDir);
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

        public static bool IsValidPrefix(KeySequence sequence, bool ignore_case)
            => m_sequences.IsValidPrefix(sequence, ignore_case);
        public static bool IsValidSequence(KeySequence sequence, bool ignore_case)
            => m_sequences.IsValidSequence(sequence, ignore_case);
        public static string GetSequenceResult(KeySequence sequence, bool ignore_case)
            => m_sequences.GetSequenceResult(sequence, ignore_case);

        public static bool IsValidGenericPrefix(KeySequence sequence)
        {
            if (!UnicodeInput.Value)
                return false;
            var seq_string = sequence.ToString().Replace(", ", "").ToLower(CultureInfo.InvariantCulture);
            return m_match_gen_prefix.Match(seq_string).Success;
        }

        public static bool IsValidGenericSequence(KeySequence sequence)
        {
            if (!UnicodeInput.Value)
                return false;
            var seq_string = sequence.ToString().Replace(", ", "").ToLower(CultureInfo.InvariantCulture);
            return m_match_gen_seq.Match(seq_string).Success;
        }

        public static string GetGenericSequenceResult(KeySequence sequence)
        {
            if (!UnicodeInput.Value)
                return "";
            var seq_string = sequence.ToString().Replace(", ", "").ToLower(CultureInfo.InvariantCulture);
            var m = m_match_gen_seq.Match(seq_string);
            if (!m.Success)
                return "";
            int codepoint = Convert.ToInt32(m.Groups[1].Value, 16);
            if (codepoint < 0 || codepoint > 0x10ffff)
                return "";
            if (codepoint >= 0xd800 && codepoint < 0xe000)
                return "";
            return char.ConvertFromUtf32(codepoint);
        }

        private static Regex m_match_gen_prefix = new Regex(@"^u[0-9a-f]*$");
        private static Regex m_match_gen_seq = new Regex(@"^u([0-9a-f]+)( |vk[.]return)$");

        public static List<SequenceDescription> GetSequenceDescriptions() => m_sequences.GetSequenceDescriptions();

        public static Metadata Metadata => m_metadata;

        // Tree of all known sequences
        private static SequenceTree m_sequences = new SequenceTree();

        // User data associated with sequences
        private static Metadata m_metadata = new Metadata();

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
           new Key(VK.SNAPSHOT),
           new Key(VK.SCROLL),
           new Key(VK.TAB),
           new Key(VK.HOME),
           new Key(VK.END),
           new Key("`"),
        };

        private static readonly Key m_default_compose_key = new Key(VK.RMENU);

        private static readonly
        Dictionary<string, string> m_valid_languages = GetSupportedLanguages();

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
    }
}

