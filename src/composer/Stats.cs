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

namespace WinCompose
{

static class Stats
{
    /// <summary>
    /// Add a pressed key to the stats
    /// </summary>
    /// <param name="key"></param>
    public static void AddKey(Key key)
    {
        int count = 0;
        m_key_stats.TryGetValue(key, out count);
        m_key_stats[key] = count + 1;
    }

    /// <summary>
    /// Add a pair of pressed keys to the stats
    /// </summary>
    /// <param name="key_a"></param>
    /// <param name="key_b"></param>
    public static void AddPair(Key key_a, Key key_b)
    {
        int count = 0;
        KeySequence s = new KeySequence() { key_a, key_b };
        m_pair_stats.TryGetValue(s, out count);
        m_pair_stats[s] = count + 1;
    }

    /// <summary>
    /// Add a successfully typed sequence to the stats
    /// </summary>
    /// <param name="sequence"></param>
    public static void AddSequence(KeySequence sequence)
    {
        int count = 0;
        m_sequence_stats.TryGetValue(sequence, out count);
        m_sequence_stats[sequence] = count + 1;
    }

    public static void DumpStats()
    {
        /* TODO */
    }

    private static Dictionary<Key, int> m_key_stats
        = new Dictionary<Key, int>();

    private static Dictionary<KeySequence, int> m_pair_stats
        = new Dictionary<KeySequence, int>();

    private static Dictionary<KeySequence, int> m_sequence_stats
        = new Dictionary<KeySequence, int>();
}

}
