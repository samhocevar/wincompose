//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2015 Sam Hocevar <sam@hocevar.net>
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
using System.ComponentModel;
using System.Globalization;

namespace WinCompose
{

public class KeyConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        var strValue = value as string;
        if (strValue != null)
        {
            if (strValue.StartsWith("VK."))
            {
                try
                {
                    var enumValue = Enum.Parse(typeof(VK), strValue.Substring(3));
                    return new Key((VK)enumValue);
                }
                catch
                {
                    // Silently catch parsing exception.
                }
            }
            return new Key(strValue);
        }
        return base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        return destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}

/// <summary>
/// The Key class describes anything that can be done on the keyboard,
/// so either a printable string or a virtual key code.
/// </summary>
[TypeConverter(typeof(KeyConverter))]
public class Key
{
    private static readonly Dictionary<VK, string> m_key_labels = new Dictionary<VK, string>
    {
        { VK.UP,    "▲" },
        { VK.DOWN,  "▼" },
        { VK.LEFT,  "◀" },
        { VK.RIGHT, "▶" },
    };

    /// <summary>
    /// A list of keys for which we have a friendly name. This is used in
    /// the GUI, where the user can choose which key acts as the compose
    /// key. It needs to be lazy-initialised, because we create Key objects
    /// way before the application language is set, and we need the
    /// translated version.
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
            { new Key(VK.SCROLL), i18n.Text.KeyScroll },
            { new Key(VK.INSERT), i18n.Text.KeyInsert },

            { new Key(" "),    i18n.Text.KeySpace },
            { new Key("\r"),   i18n.Text.KeyReturn },
            { new Key("\x1b"), i18n.Text.KeyEscape },
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
    /// A friendly name that we can put in e.g. a dropdown menu
    /// </summary>
    public string FriendlyName
    {
        get
        {
            if (m_key_names == null)
                m_key_names = GetKeyNames();
            string ret;
            if (m_key_names.TryGetValue(this, out ret))
                return ret;
            return m_str ?? string.Format("VK.{0}", m_vk);
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

            return m_str ?? string.Format("VK.{0}", m_vk);
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

    public override int GetHashCode()
    {
        return m_str != null ? m_str.GetHashCode() : ((int)m_vk).GetHashCode();
    }
};

/// <summary>
/// The KeySequence class describes a sequence of keys, which can be
/// compared with other lists of keys.
/// </summary>
public class KeySequence : List<Key>
{
    public KeySequence() : base(new List<Key>()) {}

    public KeySequence(List<Key> val) : base(val) {}

    public override bool Equals(object o)
    {
        if (!(o is KeySequence))
            return false;

        if (Count != (o as KeySequence).Count)
            return false;

        for (int i = 0; i < Count; ++i)
            if (this[i] != (o as KeySequence)[i])
                return false;

        return true;
    }

    public new KeySequence GetRange(int start, int count)
    {
        return new KeySequence(base.GetRange(start, count));
    }

    public override int GetHashCode()
    {
        int hash = 0x2d2816fe;
        for (int i = 0; i < Count; ++i)
            hash = hash * 31 + this[i].GetHashCode();
        return hash;
    }
};

/*
 * This data structure is used for communication with the GUI
 */

public class SequenceDescription : IComparable<SequenceDescription>
{
    public KeySequence Sequence = new KeySequence();
    public string Description = "";
    public string Result = "";
    public int Utf32 = -1;

    public int CompareTo(SequenceDescription other)
    {
        // If any sequence leads to a single character, compare actual
        // Unicode codepoints rather than strings
        if (Utf32 != -1 || other.Utf32 != -1)
            return Utf32.CompareTo(other.Utf32);
        return Result.CompareTo(other.Result);
    }
};

/*
 * The SequenceTree class contains a tree of all valid sequences, where
 * each child is indexed by the sequence key.
 */

public class SequenceTree
{
    public void Add(KeySequence sequence, string result, int utf32, string desc)
    {
        if (sequence.Count == 0)
        {
            m_result = result;
            m_utf32 = utf32;
            m_description = desc;
            return;
        }

        if (!m_children.ContainsKey(sequence[0]))
            m_children.Add(sequence[0], new SequenceTree());

        var subsequence = sequence.GetRange(1, sequence.Count - 1);
        m_children[sequence[0]].Add(subsequence, result, utf32, desc);
    }

    public bool IsValidPrefix(KeySequence sequence, bool ignore_case)
    {
        Search flags = Search.Prefixes;
        if (ignore_case)
            flags |= Search.IgnoreCase;
        return GetSubtree(sequence, flags) != null;
    }

    public bool IsValidSequence(KeySequence sequence, bool ignore_case)
    {
        Search flags = Search.Sequences;
        if (ignore_case)
            flags |= Search.IgnoreCase;
        return GetSubtree(sequence, flags) != null;
    }

    public string GetSequenceResult(KeySequence sequence, bool ignore_case)
    {
        Search flags = Search.Sequences;
        if (ignore_case)
            flags |= Search.IgnoreCase;
        SequenceTree node = GetSubtree(sequence, flags);
        return node == null ? "" : node.m_result;
    }

    /// <summary>
    /// List all possible sequences for the GUI.
    /// </summary>
    public List<SequenceDescription> GetSequenceDescriptions()
    {
        List<SequenceDescription> ret = new List<SequenceDescription>();
        BuildSequenceDescriptions(ret, new KeySequence());
        ret.Sort();
        return ret;
    }

    [Flags]
    private enum Search
    {
        Sequences  = 1,
        Prefixes   = 2,
        IgnoreCase = 4,
    };

    /// <summary>
    /// If the first key of <see cref="sequences"/> matches a known child,
    /// return that child. Otherwise, return null.
    /// If <see cref="non_terminal"/> is true, but we are a rule with no
    /// children, we return null. This lets us check for strict prefixes
    /// in addition to full secquences.
    /// </summary>
    private SequenceTree GetSubtree(KeySequence sequence, Search flags)
    {
        if (sequence.Count == 0)
        {
            if ((flags & Search.Prefixes) != 0 && m_children.Count == 0)
                return null;
            if ((flags & Search.Sequences) != 0 && m_result == null)
                return null;
            return this;
        }

        KeySequence keys = new KeySequence{ sequence[0] };
        if ((flags & Search.IgnoreCase) != 0 && sequence[0].IsPrintable())
        {
            Key upper = new Key(sequence[0].ToString().ToUpper());
            if (upper != sequence[0])
                keys.Add(upper);

            Key lower = new Key(sequence[0].ToString().ToLower());
            if (lower != sequence[0])
                keys.Add(lower);
        }

        foreach (Key k in keys)
        {
            if (!m_children.ContainsKey(k))
                continue;

            var subsequence = sequence.GetRange(1, sequence.Count - 1);
            var node = m_children[k].GetSubtree(subsequence, flags);
            if (node != null)
                return node;
        }

        return null;
    }

    /// <summary>
    /// Build a list of sequence descriptions by recursively adding our
    /// children to <see cref="list"/>. The output structure is suitable
    /// for the GUI.
    /// </summary>
    private void BuildSequenceDescriptions(List<SequenceDescription> list,
                                           KeySequence path)
    {
        if (m_result != null)
        {
            var item = new SequenceDescription();
            item.Sequence = path;
            item.Result = m_result;
            item.Description = m_description;
            item.Utf32 = m_utf32;
            list.Add(item);
        }

        foreach (var pair in m_children)
        {
            var newpath = new KeySequence(path);
            newpath.Add(pair.Key);
            pair.Value.BuildSequenceDescriptions(list, newpath);
        }
    }

    private Dictionary<Key, SequenceTree> m_children
        = new Dictionary<Key, SequenceTree>();
    private string m_result;
    private string m_description;
    private int m_utf32;
};

}
