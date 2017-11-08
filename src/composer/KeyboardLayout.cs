//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2017 Sam Hocevar <sam@hocevar.net>
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
        m_possible_dead_keys = new Dictionary<string, int>();
        m_possible_altgr_keys = new Dictionary<string, string>();

        // Try every keyboard key followed by space to see which ones are
        // dead keys. This way, when later we want to know if a dead key is
        // currently buffered, we just call ToUnicode(VK.SPACE) and match
        // the result with what we found here.
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

            // If the AltGr gives us a result and it’s different from without
            // AltGr, we need to remember it.
            string str = str_if_dead != " " ? str_if_dead : str_if_normal;
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
                no_altgr[i] = str_if_dead != " " ? str_if_dead : str_if_normal;
            }

            // If the resulting string is not the space character, it means
            // that it was a dead key. Good!
            if (str_if_dead != " ")
                m_possible_dead_keys[str_if_dead] = i;
        }

        // Clean up key buffer
        VkToUnicode(VK.SPACE);
        VkToUnicode(VK.SPACE);
    }

    /// <summary>
    /// Save the dead key if there is one, since we'll be calling ToUnicode
    /// later on. This effectively removes any dead key from the ToUnicode
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
    /// Restore the previously saved dead key. This should restore the ToUnicode
    /// internal buffer.
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
        // Detect keyboard layout changes by querying the foreground window,
        // and apply the same layout to WinCompose itself.
        IntPtr hwnd = NativeMethods.GetForegroundWindow();
        uint pid, tid = NativeMethods.GetWindowThreadProcessId(hwnd, out pid);
        IntPtr active_layout = NativeMethods.GetKeyboardLayout(tid);

        Window.IsGtk = false;
        Window.IsNPPOrLO = false;
        Window.IsOffice = false;
        Window.IsOtherDesktop = false;

        const int len = 256;
        StringBuilder buf = new StringBuilder(len);
        if (NativeMethods.GetClassName(hwnd, buf, len) > 0)
        {
            string wclass = buf.ToString();

            if (wclass == "gdkWindowToplevel" || wclass == "xchatWindowToplevel"
                 || wclass == "hexchatWindowToplevel")
                Window.IsGtk = true;

            /* Notepad++ or LibreOffice */
            if (wclass == "Notepad++" || wclass == "SALFRAME")
                Window.IsNPPOrLO = true;

            if (wclass == "rctrl_renwnd32" || wclass == "OpusApp")
                Window.IsOffice = true;

            if (Regex.Match(wclass, "^(SynergyDesk|cygwin/x.*)$").Success)
                Window.IsOtherDesktop = true;
        }

        if (active_layout != m_current_layout)
        {
            m_current_layout = active_layout;

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
        if (m_possible_altgr_keys.TryGetValue(key.ToString(), out var result))
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

        if (str_if_dead != " ")
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
        int ret = NativeMethods.ToUnicode(vk, sc, keystate, buf, buflen, flags);
        if (ret > 0 && ret < buflen)
        {
            return Encoding.Unicode.GetString(buf, 0, ret * 2);
        }
        return "";
    }

    public struct WindowProperties
    {
        public bool IsGtk { get; set; }
        public bool IsNPPOrLO { get; set; }
        public bool IsOffice { get; set; }
        public bool IsOtherDesktop { get; set; }
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
}

}
