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
/*
 * The Key class describes anything that can be done on the keyboard,
 * so either a printable string or a virtual key code.
 */
[TypeConverter(typeof(KeyConverter))]
public class Key
{
    public Key(string str) { m_str = str; }

    public Key(VK vk) { m_vk = vk; }

    public bool IsPrintable()
    {
        return m_str != null;
    }

    public override string ToString()
    {
        if (m_vk != VK.NONE)
        {
            return string.Format("VK.{0}", m_vk);
        }

        string ret;
        if (m_key_symbols.TryGetValue(m_vk, out ret))
            return ret;
        return m_str ?? "";
    }

    public override bool Equals(object o)
    {
        return o is Key && this == (o as Key);
    }

    public static bool operator ==(Key a, Key b)
    {
        bool is_a_null = object.ReferenceEquals(a, null);
        bool is_b_null = object.ReferenceEquals(b, null);
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

    private VK m_vk;
    private string m_str;

    private static readonly Dictionary<VK, string> m_key_symbols
     = new Dictionary<VK, string>()
    {
        { VK.UP,    "▲" },
        { VK.DOWN,  "▼" },
        { VK.LEFT,  "◀" },
        { VK.RIGHT, "▶" },
    };
};

/*
 * This data structure is used for communication with the GUI
 */

public class SequenceDescription : IComparable<SequenceDescription>
{
    public List<Key> Sequence = new List<Key>();
    public string Result = "";
    public string Description = "";

    public int CompareTo(SequenceDescription other)
    {
        // Compare actual Unicode codepoints rather than characters
        int ch1 = Result.Length > 0 ? (int)(char)Result[0] : 0;
        int ch2 = other.Result.Length > 0 ? (int)(char)other.Result[0] : 0;
        return ch1.CompareTo(ch2);
    }
};

/*
 * The SequenceTree class contains a tree of all valid sequences, where
 * each child is indexed by the sequence key.
 */

public class SequenceTree
{
    public void Add(List<Key> sequence, string result, string description)
    {
        if (sequence.Count == 0)
        {
            m_result = result;
            m_description = description;
            return;
        }

        if (!m_children.ContainsKey(sequence[0]))
            m_children.Add(sequence[0], new SequenceTree());

        var subsequence = sequence.GetRange(1, sequence.Count - 1);
        m_children[sequence[0]].Add(subsequence, result, description);
    }

    public bool IsValidPrefix(List<Key> sequence)
    {
        SequenceTree subtree = GetSubtree(sequence);
        return subtree != null;
    }

    public bool IsValidSequence(List<Key> sequence)
    {
        SequenceTree subtree = GetSubtree(sequence);
        return subtree != null && subtree.m_result != null;
    }

    public string GetSequenceResult(List<Key> sequence)
    {
        SequenceTree tree = GetSubtree(sequence);
        return tree == null ? "" : tree.m_result == null ? "" : tree.m_result;
    }

    public SequenceTree GetSubtree(List<Key> sequence)
    {
        if (sequence.Count == 0)
            return this;
        if (!m_children.ContainsKey(sequence[0]))
            return null;
        var subsequence = sequence.GetRange(1, sequence.Count - 1);
        return m_children[sequence[0]].GetSubtree(subsequence);
    }

    public List<SequenceDescription> GetSequenceDescriptions()
    {
        List<SequenceDescription> ret = new List<SequenceDescription>();
        BuildSequenceDescriptions(ret, new List<Key>());
        ret.Sort();
        return ret;
    }

    private void BuildSequenceDescriptions(List<SequenceDescription> list,
                                           List<Key> path)
    {
        if (m_result != null)
        {
            var item = new SequenceDescription();
            item.Sequence = path;
            item.Result = m_result;
            item.Description = m_description;
            list.Add(item);
        }

        foreach (var pair in m_children)
        {
            var newpath = new List<Key>(path);
            newpath.Add(pair.Key);
            pair.Value.BuildSequenceDescriptions(list, newpath);
        }
    }

    private Dictionary<Key, SequenceTree> m_children
        = new Dictionary<Key, SequenceTree>();
    private string m_result;
    private string m_description;
};

}
