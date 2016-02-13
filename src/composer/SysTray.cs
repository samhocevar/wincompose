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

using Microsoft.Win32;

namespace WinCompose
{
    static class SysTray
    {
        /// <summary>
        /// Indicates whether explorer.exe should be forcefully restarted for
        /// our changes to become visible.
        /// </summary>
        public static bool MustKillExplorer { get; private set; }

        private const int HEADER_SIZE = 20;
        private const int ENTRY_SIZE = 1640;
        private const int MAGIC_OFFSET = 528;

        public static void Fixup()
        {
            string name = @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\TrayNotify";
            string value = "IconStreams";
            RegistryKey subkey = Registry.CurrentUser.OpenSubKey(name, true);
            byte[] data = (byte[])subkey.GetValue(value, null);

            // This key should have a 20-byte header and several 1640-byte entries
            if (data == null || data.Length % ENTRY_SIZE != HEADER_SIZE)
            {
                subkey.Close();
                return;
            }

            bool has_changed = false;

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

                if (application_path.ToLower().EndsWith(@"\wincompose.exe"))
                {
                    Log.Debug("Enforcing SysTray visibility for {0}", application_path);
                    data[offset + MAGIC_OFFSET] = 2;
                    has_changed = true;
                }
            }

            if (has_changed)
            {
                subkey.SetValue(value, data);
                MustKillExplorer = true;
            }

            subkey.Close();
        }
    }
}

