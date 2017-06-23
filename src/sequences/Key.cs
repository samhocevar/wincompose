//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2016 Sam Hocevar <sam@hocevar.net>
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

namespace WinCompose
{

/// <summary>
/// The Key class describes anything that can be hit on the keyboard,
/// resulting in either a printable string or a virtual key code.
/// </summary>
public class Key
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
    };

    /// <summary>
    /// A dictionary of non-trivial keysyms and the corresponding
    /// Key object. Trivial (one-character) keysyms are not needed.
    /// </summary>
    private static readonly Dictionary<string, Key> m_keysyms
        = new Dictionary<string, Key>
    {
        // ASCII-mapped keysyms
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
        { "Multi_key", new Key(VK.COMPOSE) },
        { "Up",        new Key(VK.UP) },
        { "Down",      new Key(VK.DOWN) },
        { "Left",      new Key(VK.LEFT) },
        { "Right",     new Key(VK.RIGHT) },
    };

    /// <summary>
    /// A dictionary of keysyms and the corresponding Key object
    /// </summary>
    public static Key FromKeySym(string keysym)
    {
        if (m_keysyms.ContainsKey(keysym))
            return m_keysyms[keysym];

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

    private static Dictionary<Key, string> GetKeyNames()
    {
        return new Dictionary<Key, string>
        {
            { new Key(VK.LMENU), i18n.Text.KeyLMenu },
            { new Key(VK.RMENU), i18n.Text.KeyRMenu },
            { new Key(VK.LCONTROL), i18n.Text.KeyLControl },
            { new Key(VK.RCONTROL), i18n.Text.KeyRControl },
            { new Key(VK.LWIN), i18n.Text.KeyLWin },
            { new Key(VK.RWIN), i18n.Text.KeyRWin },
            { new Key(VK.CAPITAL), i18n.Text.KeyCapital },
            { new Key(VK.NUMLOCK), i18n.Text.KeyNumLock },
            { new Key(VK.PAUSE), i18n.Text.KeyPause },
            { new Key(VK.APPS), i18n.Text.KeyApps },
            { new Key(VK.ESCAPE), i18n.Text.KeyEscape },
            { new Key(VK.CONVERT), i18n.Text.KeyConvert },
            { new Key(VK.NONCONVERT), i18n.Text.KeyNonConvert },
            { new Key(VK.SCROLL), i18n.Text.KeyScroll },
            { new Key(VK.INSERT), i18n.Text.KeyInsert },

            { new Key(" "),    i18n.Text.KeySpace },
            { new Key("\r"),   i18n.Text.KeyReturn },
            { new Key("\x1b"), i18n.Text.KeyEscape },

            { new Key(VK.F1), "F1" },
            { new Key(VK.F2), "F2" },
            { new Key(VK.F3), "F3" },
            { new Key(VK.F4), "F4" },
            { new Key(VK.F5), "F5" },
            { new Key(VK.F6), "F6" },
            { new Key(VK.F7), "F7" },
            { new Key(VK.F8), "F8" },
            { new Key(VK.F9), "F9" },
            { new Key(VK.F10), "F10" },
            { new Key(VK.F11), "F11" },
            { new Key(VK.F12), "F12" },
            { new Key(VK.F13), "F13" },
            { new Key(VK.F14), "F14" },
            { new Key(VK.F15), "F15" },
            { new Key(VK.F16), "F16" },
            { new Key(VK.F17), "F17" },
            { new Key(VK.F18), "F18" },
            { new Key(VK.F19), "F19" },
            { new Key(VK.F20), "F20" },
            { new Key(VK.F21), "F21" },
            { new Key(VK.F22), "F22" },
            { new Key(VK.F23), "F23" },
            { new Key(VK.F24), "F24" },
        };
    }

    private readonly VK m_vk;

    private readonly string m_str;

    public Key(string str) { m_str = str; }

    public Key(VK vk) { m_vk = vk; }

    public VK VirtualKey { get { return m_vk; } }

    public bool IsPrintable()
    {
        return m_str != null;
    }

    /// <summary>
    /// Return whether a key is usable in a compose sequence
    /// </summary>
    public bool IsUsable()
    {
        return IsPrintable() || m_keysyms.ContainsValue(this);
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
            // Lazy initialisation of m_key_names (see above)
            if (m_key_names == null)
                m_key_names = GetKeyNames();

            string ret;
            if (m_key_names.TryGetValue(this, out ret))
                return ret;
            return ToString();
        }
    }

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
        return m_str ?? string.Format("VK.{0}", m_vk);
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
