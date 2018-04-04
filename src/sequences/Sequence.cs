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
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WinCompose
{

/// <summary>
/// The KeySequenceConverter class allows to convert a string or a string-like
/// object to a Key object and back.
/// </summary>
public class KeySequenceConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context,
                                        Type src_type)
    {
        if (src_type != typeof(string))
            return base.CanConvertFrom(context, src_type);

        return true;
    }

    public override object ConvertFrom(ITypeDescriptorContext context,
                                       CultureInfo culture, object val)
    {
        var list_str = val as string;
        if (list_str == null)
            return base.ConvertFrom(context, culture, val);

        KeySequence ret = new KeySequence();
        foreach (string str in Array.ConvertAll(list_str.Split(','), x => x.Trim()))
        {
            Key k = new Key(str);
            if (str.StartsWith("VK."))
            {
                try
                {
                    var enum_val = Enum.Parse(typeof(VK), str.Substring(3));
                    k = new Key((VK)enum_val);
                }
                catch { } // Silently catch parsing exception.
            }
            ret.Add(k);
        }

        return ret;
    }

    public override object ConvertTo(ITypeDescriptorContext context,
                                     CultureInfo culture, object val,
                                     Type dst_type)
    {
        if (dst_type != typeof(string))
            return base.ConvertTo(context, culture, val, dst_type);

        return (val as KeySequence).ToString();
    }
}

/// <summary>
/// The KeySequence class describes a sequence of keys, which can be
/// compared with other sequences of keys.
/// </summary>
[TypeConverter(typeof(KeySequenceConverter))]
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

    /// <summary>
    /// Serialize sequence to a printable string.
    /// </summary>
    public override string ToString()
    {
        return string.Join(", ", Array.ConvertAll(ToArray(), x => x.ToString()));
    }

    public string FriendlyName
    {
        get { return string.Join(", ", Array.ConvertAll(ToArray(), x => x.FriendlyName)); }
    }

    /// <summary>
    /// Get a subsequence of the current sequence.
    /// </summary>
    public new KeySequence GetRange(int start, int count)
    {
        return new KeySequence(base.GetRange(start, count));
    }

    /// <summary>
    /// Hash sequence by combining the hashcodes of all its composing keys.
    /// </summary>
    public override int GetHashCode()
    {
        int hash = 0x2d2816fe;
        foreach (Key ch in this)
            hash = hash * 31 + ch.GetHashCode();
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

    /// <summary>
    /// Sequence comparison routine. Use to sort sequences alphabetically or
    /// numerically in the GUI.
    /// </summary>
    public int CompareTo(SequenceDescription other)
    {
        // If either sequence results in a single character, compare actual
        // Unicode codepoints. Otherwise, compare sequences alphabetically.
        if (Utf32 != -1 || other.Utf32 != -1)
            return Utf32.CompareTo(other.Utf32);
        return Result.CompareTo(other.Result);
    }
};

/// <summary>
/// The SequenceTree class contains a tree of all valid sequences, where
/// each child is indexed by the sequence key.
/// Some functions such as <see cref="IsValidPrefix"/> also work to query
/// the special Unicode entry mode.
/// </summary>
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

        // First check if the sequence prefix exists in our tree
        if (GetSubtree(sequence, flags) != null)
            return true;

        // Otherwise, check for generic Unicode entry prefix
        if (Settings.UnicodeInput)
        {
            var sequenceString = sequence.ToString().Replace(", ", "").ToLower(CultureInfo.InvariantCulture);
            return Regex.Match(sequenceString, @"^u[0-9a-f]{0,4}$").Success
                && !Regex.Match(sequenceString, @"^u[03-9a-d]...$").Success;
        }

        return false;
    }

    public bool IsValidSequence(KeySequence sequence, bool ignore_case)
    {
        Search flags = Search.Sequences;
        if (ignore_case)
            flags |= Search.IgnoreCase;

        // First check if the sequence exists in our tree
        if (GetSubtree(sequence, flags) != null)
            return true;

        // Otherwise, check for generic Unicode sequence
        if (Settings.UnicodeInput)
        {
            var sequenceString = sequence.ToString().Replace(", ", "").ToLower(CultureInfo.InvariantCulture);
            return Regex.Match(sequenceString, @"^u[0-9a-f]{2,5}$").Success
                    && !Regex.Match(sequenceString, @"^ud[89a-f]..$").Success;
        }

        return false;
    }

    public string GetSequenceResult(KeySequence sequence, bool ignore_case)
    {
        Search flags = Search.Sequences;
        if (ignore_case)
            flags |= Search.IgnoreCase;

        // First check if the sequence exists in our tree
        SequenceTree subtree = GetSubtree(sequence, flags);
        if (subtree != null && subtree.m_result != "")
            return subtree.m_result;

        // Otherwise, check for a generic Unicode sequence
        if (Settings.UnicodeInput)
        {
            var sequenceString = sequence.ToString().Replace(", ", "").ToLower(CultureInfo.InvariantCulture);
            var m = Regex.Match(sequenceString, @"^u([0-9a-f]{2,5})$");
            if (m.Success)
            {
                int codepoint = Convert.ToInt32(m.Groups[1].Value, 16);
                return char.ConvertFromUtf32(codepoint);
            }
        }

        return "";
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
    /// If <see cref="flags"/> has the <see cref="Search.Prefixes"/> flag
    /// and we are a rule with no children, we return null. This lets us
    /// check for strict prefixes.
    /// If <see cref="flags"/> has the <see cref="Search.Sequences"/> flag
    /// and we are not a rule (just a node in the tree), we do not return
    /// the current node (but we do return children sequences).
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
            var subtree = m_children[k].GetSubtree(subsequence, flags);
            if (subtree != null)
                return subtree;
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
