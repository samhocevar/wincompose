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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WinCompose
{

/// <summary>
/// The Key class describes anything that can be hit on the keyboard,
/// resulting in either a printable string or a virtual key code.
/// </summary>
public partial class Key
{
    /// <summary>
    /// A dictionary of symbols that we use for some non-printable key labels.
    /// </summary>
    private static readonly Dictionary<VK, string> m_key_labels
        = new Dictionary<VK, string>
    {
        { VK.COMPOSE, "♦" },
        { VK.UP,      "▲" },
        { VK.DOWN,    "▼" },
        { VK.LEFT,    "◀" },
        { VK.RIGHT,   "▶" },
        { VK.HOME,    "Home" },
        { VK.END,     "End" },
        { VK.BACK,    "⌫" },
        { VK.DELETE,  "␡" },
        { VK.TAB,     "↹" },
    };

    /// <summary>
    /// A dictionary of non-printable keysyms and the corresponding virtual
    /// key (VK) value.
    /// </summary>
    private static readonly Dictionary<string, VK> m_extra_keysyms
        = new Dictionary<string, VK>
    {
        { "Multi_key", VK.COMPOSE },
        { "Up",        VK.UP },
        { "Down",      VK.DOWN },
        { "Left",      VK.LEFT },
        { "Right",     VK.RIGHT },
        { "Home",      VK.HOME },
        { "End",       VK.END },
        { "BackSpace", VK.BACK },
        { "Delete",    VK.DELETE },
        { "Tab",       VK.TAB },
        { "Return",    VK.RETURN },
    };

    /// <summary>
    /// A dictionary of printable keysyms and the corresponding string. This list
    /// is directly read from a header from the Xorg project.
    /// </summary>
    private static readonly Dictionary<string, string> m_keysyms
        = ReadXorgKeySyms();

    private static Dictionary<string, string> ReadXorgKeySyms()
    {
        Dictionary<string, string> ret = new Dictionary<string, string>();
        using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("3rdparty.keysymdef.h"))
        using (StreamReader reader = new StreamReader(s))
        {
            Regex r = new Regex(@"^#define XK_([^ ]*).* U\+([A-Za-z0-9]+)");
            for (string l = reader.ReadLine(); l != null; l = reader.ReadLine())
            {
                Match m = r.Match(l);
                if (m.Success)
                {
                    int codepoint = int.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
                    ret[m.Groups[1].Value] = char.ConvertFromUtf32(codepoint);
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// A dictionary of keysyms and the corresponding Key object
    /// </summary>
    public static Key FromKeySym(string keysym)
    {
        if (m_keysyms.ContainsKey(keysym))
            return new Key(m_keysyms[keysym]);

        if (m_extra_keysyms.ContainsKey(keysym))
            return new Key(m_extra_keysyms[keysym]);

        if (keysym.Length > 1 && keysym[0] == 'U' &&
            int.TryParse(keysym.Substring(1), NumberStyles.HexNumber, null, out var codepoint))
            return new Key(char.ConvertFromUtf32(codepoint));

        if (keysym.Length == 1)
            return new Key(keysym);

        return null;
    }

    /// <summary>
    /// A list of keys for which we have a friendly name. This is used in
    /// the GUI, where the user can choose which key acts as the compose
    /// key. It needs to be lazy-initialised, because if we create Key objects
    /// before the application language is set, the descriptions will not be
    /// properly translated.
    /// </summary>
    private static Dictionary<Key, string> m_key_names = null;

    private static Dictionary<Key, string> KeyNames
    {
        get
        {
            // Lazy initialisation of m_key_names (see above)
            if (m_key_names == null)
            {
                m_key_names = new Dictionary<Key, string>
                {
                    { new Key(VK.DISABLED),   i18n.Text.KeyDisabled },
                    { new Key(VK.LMENU),      i18n.Text.KeyLMenu },
                    { new Key(VK.RMENU),      i18n.Text.KeyRMenu },
                    { new Key(VK.LCONTROL),   i18n.Text.KeyLControl },
                    { new Key(VK.RCONTROL),   i18n.Text.KeyRControl },
                    { new Key(VK.LWIN),       i18n.Text.KeyLWin },
                    { new Key(VK.RWIN),       i18n.Text.KeyRWin },
                    { new Key(VK.CAPITAL),    i18n.Text.KeyCapital },
                    { new Key(VK.NUMLOCK),    i18n.Text.KeyNumLock },
                    { new Key(VK.PAUSE),      i18n.Text.KeyPause },
                    { new Key(VK.APPS),       i18n.Text.KeyApps },
                    { new Key(VK.ESCAPE),     i18n.Text.KeyEscape },
                    { new Key(VK.CONVERT),    i18n.Text.KeyConvert },
                    { new Key(VK.NONCONVERT), i18n.Text.KeyNonConvert },
                    { new Key(VK.SCROLL),     i18n.Text.KeyScroll },
                    { new Key(VK.INSERT),     i18n.Text.KeyInsert },
                    { new Key(VK.SNAPSHOT),   i18n.Text.KeyPrint },
                    { new Key(VK.TAB),        i18n.Text.KeyTab },
                    { new Key(VK.HOME),       i18n.Text.KeyHome },
                    { new Key(VK.END),        i18n.Text.KeyEnd },

                    { new Key(" "),    i18n.Text.KeySpace },
                    { new Key("\r"),   i18n.Text.KeyReturn },

                    // This should not be necessary because we build these
                    // key objects using their VirtualKey.
                    { new Key("\t"),   i18n.Text.KeyTab },
                    { new Key("\x1b"), i18n.Text.KeyEscape },
                };

                /* Append F1—F24 */
                for (VK vk = VK.F1; vk <= VK.F24; ++vk)
                    m_key_names.Add(new Key(vk), vk.ToString());
            }

            return m_key_names;
        }
    }

    private readonly VK m_vk;
    private readonly string m_str;

    public Key(string str) { m_str = str; }

    public Key(VK vk) { m_vk = vk; }

    public VK VirtualKey => m_vk;
    public bool IsPrintable => m_str != null;

    /// <summary>
    /// Return whether a key is usable in a compose sequence
    /// </summary>
    public bool IsUsable()
    {
        return IsPrintable || m_extra_keysyms.ContainsValue(m_vk);
    }

    /// <summary>
    /// Return whether a key is a modifier (shift, ctrl, alt)
    /// </summary>
    public bool IsModifier()
    {
        switch (m_vk)
        {
            case VK.LCONTROL:
            case VK.RCONTROL:
            case VK.CONTROL:
            case VK.LSHIFT:
            case VK.RSHIFT:
            case VK.SHIFT:
            case VK.LMENU:
            case VK.RMENU:
            case VK.MENU:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// A friendly name that we can put in e.g. a dropdown menu
    /// </summary>
    public string FriendlyName
    {
        get
        {
            string ret;
            if (KeyNames.TryGetValue(this, out ret))
                return ret;
            return ToString();
        }
    }

    /// <summary>
    /// This should be part of a Key viewmodel class
    /// </summary>
    public double Opacity => m_vk == VK.DISABLED ? 0.5 : 1.0;

    /// <summary>
    /// A label that we can print on keycap icons
    /// </summary>
    public string KeyLabel
    {
        get
        {
            string ret;
            if (m_key_labels.TryGetValue(m_vk, out ret))
                return ret;
            return ToString();
        }
    }

    /// <summary>
    /// Serialize key to a printable string we can parse back into
    /// a <see cref="Key"/> object
    /// </summary>
    public override string ToString()
    {
        return m_str ?? $"VK.{m_vk}";
    }

    public override bool Equals(object o)
    {
        return o is Key && this == (o as Key);
    }

    public static bool operator ==(Key a, Key b)
    {
        bool is_a_null = ReferenceEquals(a, null);
        bool is_b_null = ReferenceEquals(b, null);
        if (is_a_null || is_b_null)
            return is_a_null == is_b_null;
        return a.m_str != null ? a.m_str == b.m_str : a.m_vk == b.m_vk;
    }

    public static bool operator !=(Key a, Key b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Hash key by returning its printable representation’s hashcode or, if
    /// unavailable, its virtual key code’s hashcode.
    /// </summary>
    public override int GetHashCode()
    {
        return m_str != null ? m_str.GetHashCode() : ((int)m_vk).GetHashCode();
    }
};

}
