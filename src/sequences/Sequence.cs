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
using System.ComponentModel;
using System.Globalization;

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

        return KeySequence.FromString(list_str);
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
        return string.Join(",", Array.ConvertAll(ToArray(), x => x.ToString()));
    }

    /// <summary>
    /// Convert sequence to a reader-friendly string.
    /// </summary>
    public string FriendlyName
        => string.Join(", ", Array.ConvertAll(ToArray(), x => x.FriendlyName));

    /// <summary>
    /// Convert sequence to a printable string. Non-printable characters are omitted
    /// </summary>
    public string PrintableResult
        => string.Join("", Array.ConvertAll(ToArray(), x => x.PrintableResult));

    /// <summary>
    /// Convert sequence to a unique string representation that can
    /// be put in an XML attribute among other things.
    /// </summary>
    public string AsXmlAttr
        => string.Join("", Array.ConvertAll(ToArray(), x => x.AsXmlAttr));

    /// <summary>
    /// Construct a key sequence from a serialized string.
    /// </summary>
    public static KeySequence FromString(string str)
    {
        KeySequence ret = new KeySequence();
        // Be sure to call Trim() because older WinCompose versions would add a
        // space after the comma.
        foreach (string s in Array.ConvertAll(str.Split(','), x => x.Trim()))
            ret.Add(Key.FromString(s));
        return ret;
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

}
