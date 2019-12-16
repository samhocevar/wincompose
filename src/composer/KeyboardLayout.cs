//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WinCompose
{

public static class KeyboardLayout
{
    /// <summary>
    /// Attempt to enumerate all dead keys available on the current keyboard
    /// layout and cache the results in <see cref="m_possible_dead_keys"/>.
    /// </summary>
    private static void AnalyzeLayout()
    {
        // Clear key buffer
        VkToUnicode(VK.SPACE);
        VkToUnicode(VK.SPACE);

        // Compute an input locale identifier suitable for ToUnicodeEx(). This is
        // necessary because some IMEs interfer with ToUnicodeEx, e.g. Japanese,
        // so instead we pretend we use an English US keyboard.
        // I could check that Korean (Microsoft IME) and Chinese Simplified
        // (Microsoft Pinyin) are not affected.

        // High bytes are the device ID, low bytes are the language ID
        var current_device_id = (ulong)m_current_layout >> 16;
        var japanese_lang_id = NativeMethods.MAKELANG(LANG.JAPANESE, SUBLANG.JAPANESE_JAPAN);
        if (current_device_id == japanese_lang_id)
        {
            var english_lang_id = NativeMethods.MAKELANG(LANG.ENGLISH, SUBLANG.DEFAULT);
            m_transformed_hkl = (IntPtr)((english_lang_id << 16) | english_lang_id);
        }

        // Check that the transformed HKL actually works; otherwise, revert.
        if (VkToUnicode(VK.SPACE) != " ")
            m_transformed_hkl = m_current_layout;

        // Try every keyboard key followed by space to see which ones are
        // dead keys. This way, when later we want to know if a dead key is
        // currently buffered, we just call VkToUnicode(VK.SPACE) and match
        // the result with what we found here. This code also precomputes
        // characters obtained with AltGr.
        m_possible_dead_keys = new Dictionary<string, int>();
        m_possible_altgr_keys = new Dictionary<string, string>();

        string[] no_altgr = new string[0x200];
        byte[] state = new byte[256];

        for (int i = 0; i < 0x400; ++i)
        {
            VK vk = (VK)(i & 0xff);
            bool has_shift = (i & 0x100) != 0;
            bool has_altgr = (i & 0x200) != 0;

            state[(int)VK.SHIFT] = (byte)(has_shift ? 0x80 : 0x00);
            state[(int)VK.CONTROL] = (byte)(has_altgr ? 0x80 : 0x00);
            state[(int)VK.MENU] = (byte)(has_altgr ? 0x80 : 0x00);

            // First the key we’re interested in, then the space key
            string str_if_normal = VkToUnicode(vk, (SC)0, state, (LLKHF)0);
            string str_if_dead = VkToUnicode(VK.SPACE);
            VkToUnicode(VK.SPACE); // Additional safety to clear buffer

            bool has_dead = str_if_dead != "" && str_if_dead != " ";

            // If the AltGr gives us a result and it’s different from without
            // AltGr, we need to remember it.
            string str = has_dead ? str_if_dead : str_if_normal;
            if (has_altgr)
            {
                if (no_altgr[i - 0x200] != "" && str != "" && no_altgr[i - 0x200] != str)
                {
                    Log.Debug("VK {0} is “{1}” but “{2}” with AltGr",
                              vk.ToString(), no_altgr[i - 0x200], str);
                    m_possible_altgr_keys[no_altgr[i - 0x200]] = str;
                }
            }
            else
            {
                no_altgr[i] = str;
            }

            // If the resulting string is not the space character, it means
            // that it was a dead key. Good!
            if (has_dead)
            {
                Log.Debug("VK {0} is dead key “{1}”",
                          vk.ToString(), str_if_dead);
                m_possible_dead_keys[str_if_dead] = i;
            }
        }
    }

    /// <summary>
    /// Save the dead key if there is one, since we'll be calling ToUnicodeEx
    /// later on. This effectively removes any dead key from the ToUnicodeEx
    /// internal buffer.
    /// </summary>
    public static void SaveDeadKey()
    {
        m_saved_dead_key = 0;
        string str = VkToUnicode(VK.SPACE);
        if (str != " ")
            m_possible_dead_keys.TryGetValue(str, out m_saved_dead_key);
    }

    /// <summary>
    /// Restore the previously saved dead key. This should restore the
    /// ToUnicodeEx internal buffer.
    /// </summary>
    public static void RestoreDeadKey()
    {
        if (m_saved_dead_key == 0)
            return;

        byte[] state = new byte[256];

        VK vk = (VK)(m_saved_dead_key & 0xff);
        bool has_shift = (m_saved_dead_key & 0x100) != 0;
        bool has_altgr = (m_saved_dead_key & 0x200) != 0;

        state[(int)VK.SHIFT] = (byte)(has_shift ? 0x80 : 0x00);
        state[(int)VK.CONTROL] = (byte)(has_altgr ? 0x80 : 0x00);
        state[(int)VK.MENU] = (byte)(has_altgr ? 0x80 : 0x00);

        VkToUnicode(vk, (SC)0, state, (LLKHF)0);
    }

    public static void CheckForChanges()
    {
        // Detect keyboard layout changes by querying the foreground window
        // for its layout, and apply the same layout to WinCompose itself.
        Window.Hwnd = NativeMethods.GetForegroundWindow();

        var tid = NativeMethods.GetWindowThreadProcessId(Window.Hwnd, out var pid);
        var active_layout = NativeMethods.GetKeyboardLayout(tid);
        if (active_layout != m_current_layout)
        {
            m_transformed_hkl = m_current_layout = active_layout;

            Log.Debug("Active window layout tid:{0} handle:0x{1:X} lang:0x{2:X}",
                      tid, (uint)active_layout >> 16, (uint)active_layout & 0xffff);

            if (active_layout != (IntPtr)0)
                NativeMethods.ActivateKeyboardLayout(active_layout, 0);

            tid = NativeMethods.GetCurrentThreadId();
            active_layout = NativeMethods.GetKeyboardLayout(tid);

            Log.Debug("WinCompose process layout tid:{0} handle:0x{1:X} lang:0x{2:X}",
                      tid, (uint)active_layout >> 16, (uint)active_layout & 0xffff);

            // We need to rebuild the list of dead keys
            AnalyzeLayout();
        }
    }

    /// <summary>
    /// Does the current active layout have an AltGr key?
    /// </summary>
    public static bool HasAltGr => m_possible_altgr_keys.Count > 0;

    /// <summary>
    /// Convert a key to its corresponding AltGr variant
    /// </summary>
    public static Key KeyToAltGrVariant(Key key)
    {
        if (m_possible_altgr_keys.TryGetValue(key.PrintableResult, out var result))
            return new Key(result);
        return null;
    }

    internal static Key VkToKey(VK vk, SC sc, LLKHF flags, bool has_shift,
                                bool has_altgr, bool has_capslock)
    {
        byte[] keystate = new byte[256];
        NativeMethods.GetKeyboardState(keystate);
        keystate[(int)VK.SHIFT] = (byte)(has_shift ? 0x80 : 0x00);
        keystate[(int)VK.CONTROL] = (byte)(has_altgr ? 0x80 : 0x00);
        keystate[(int)VK.MENU] = (byte)(has_altgr ? 0x80 : 0x00);
        keystate[(int)VK.CAPITAL] = (byte)(has_capslock ? 0x01 : 0x00);

        // These two calls must be done together and in this order.
        string str_if_normal = VkToUnicode(vk, sc, keystate, flags);
        string str_if_dead = VkToUnicode(VK.SPACE);

        // This indicates that vk was a dead key
        if (str_if_dead != "" && str_if_dead != " ")
            return new Key(str_if_dead);

        // Special case: we don't consider characters such as Esc as printable
        // otherwise they are not properly serialised in the config file.
        if (str_if_normal == "" || str_if_normal[0] < ' ')
            return new Key(vk);

        return new Key(str_if_normal);
    }

    private static string VkToUnicode(VK vk)
    {
        return VkToUnicode(vk, (SC)0, new byte[256], (LLKHF)0);
    }

    private static string VkToUnicode(VK vk, SC sc, byte[] keystate, LLKHF flags)
    {
        const int buflen = 4;
        byte[] buf = new byte[2 * buflen];
        int ret = NativeMethods.ToUnicodeEx(vk, sc, keystate, buf, buflen,
                                            flags, m_transformed_hkl);
        if (ret > 0 && ret < buflen)
        {
            return Encoding.Unicode.GetString(buf, 0, ret * 2);
        }
        return "";
    }

    public struct WindowProperties
    {
        public void Refresh()
        {
            const int len = 256;
            string wclass = "?", wname = "?";
            StringBuilder buf = new StringBuilder(len);
            if (NativeMethods.GetClassName(Hwnd, buf, len) > 0)
                wclass = buf.ToString();
            buf = new StringBuilder(len);
            if (NativeMethods.GetWindowText(Hwnd, buf, len) > 0)
                wname = buf.ToString();

            Log.Debug($"Window {Hwnd} (class: {wclass}) (name: {wname}) got focus");

            IsGtk = m_match_gtk.Match(wclass).Success;
            IsOffice = m_match_office.Match(wclass).Success;
            IsOtherDesktop = m_match_desktop.Match(wclass).Success;

            try
            {
                var regex = new Regex($"^({Settings.IgnoreRegex})$");
                IsOtherDesktop = IsOtherDesktop || regex.Match(wclass).Success
                                                || regex.Match(wname).Success;
            }
            catch {}
        }

        // Match window class for standard GTK applications, with additional
        // case for XChat and HexChat.
        private static Regex m_match_gtk = new Regex("^(gdk|xchat|hexchat)WindowToplevel$");
        // Match Office applications (Word, Outlook…)
        private static Regex m_match_office = new Regex("^(rctrl_renwnd32|OpusApp)$");
        // Match windows where we should be inactive (Synergy, Xorg on cygwin…)
        private static Regex m_match_desktop = new Regex("^(SynergyDesk|cygwin/x.*)$");

        // Keep track of the window that has focus
        public IntPtr Hwnd
        {
            get => m_hwnd;
            set
            {
                if (value == m_hwnd)
                    return;
                m_hwnd = value;
                Refresh();
            }
        }

        public bool IsGtk { get; private set; }
        public bool IsOffice { get; private set; }
        public bool IsOtherDesktop { get; private set; }

        private IntPtr m_hwnd;
    }

    public static WindowProperties Window;

    /// <summary>
    /// The dead key previously saved by SavedDeadKey().
    /// </summary>
    private static int m_saved_dead_key;

    private static Dictionary<string, int> m_possible_dead_keys;
    private static Dictionary<string, string> m_possible_altgr_keys;

    // Initialise with -1 to make sure the above dictionaries are
    // properly initialised even if the layout is found to be 0x0.
    private static IntPtr m_current_layout = new IntPtr(-1);

    /// <summary>
    /// Alternate layout to use with ToUnicodeEx
    /// </summary>
    private static IntPtr m_transformed_hkl;
}

}
