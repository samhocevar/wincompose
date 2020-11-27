//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2020 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.IO;
using System.Text;
using System.Threading;

namespace WinCompose
{
    class IniFile : WatchedFile
    {
        public IniFile(string path)
          : base(path)
        { }

        public void LoadEntry(SettingsEntry entry, string section, string key)
        {
            try
            {
                if (!m_mutex.WaitOne(2000))
                {
                    Log.Debug($"Failed to acquire settings lock");
                    return;
                }
            }
            catch (AbandonedMutexException)
            {
                // Ignore; this might be a previous instance that crashed
            }

            try
            {
                const int len = 255;
                var must_migrate = false;
                var tmp = new StringBuilder(len);
                var result = NativeMethods.GetPrivateProfileString(section, key, "",
                                                                   tmp, len, FullPath);
                if (result == 0)
                {
                    // Compatibility code for keys that moved from the "global"
                    // to the "composing" or "tweaks" section.
                    if (section != "global")
                    {
                        result = NativeMethods.GetPrivateProfileString("global", key, "",
                                                                       tmp, len, FullPath);
                        if (result == 0)
                            return;
                        must_migrate = true;
                    }
                }

                entry.LoadString(tmp.ToString());

                if (must_migrate)
                {
                    NativeMethods.WritePrivateProfileString("global", key, null,
                                                            FullPath);
                    NativeMethods.WritePrivateProfileString(section, key, entry.ToString(),
                                                            FullPath);
                }
            }
            catch (Exception ex)
            {
                Log.Debug($"Failed to load settings: {ex}");
            }
            finally
            {
                // Ensure the mutex is always released even if an
                // exception is thrown
                m_mutex.ReleaseMutex();
            }
        }

        public void SaveEntry(string value, string section, string key)
        {
            if (!Utils.EnsureDirectory(Path.GetDirectoryName(FullPath)))
                return;

            try
            {
                if (!m_mutex.WaitOne(2000))
                {
                    Log.Debug($"Failed to acquire settings lock");
                    return;
                }
            }
            catch (AbandonedMutexException)
            {
                // Ignore; this might be a previous instance that crashed
            }

            try
            {
                Log.Debug($"Saving {section}.{key} = {value}");
                NativeMethods.WritePrivateProfileString(section, key, value, FullPath);
                // Ensure old keys are removed from the global section
                if (section != "global")
                    NativeMethods.WritePrivateProfileString("global", key, null, FullPath);
            }
            catch (Exception ex)
            {
                Log.Debug($"Failed to save settings: {ex}");
            }
            finally
            {
                // Ensure the mutex is always released even if an
                // exception is thrown
                m_mutex.ReleaseMutex();
            }
        }

        private static readonly Mutex m_mutex = new Mutex(false,
                          "wincompose-{1342C5FF-9483-45F3-BE0C-1C8D63CEA81C}");
    }
}

