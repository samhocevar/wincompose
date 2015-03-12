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
    public static AddSequence(KeySequence sequence)
    {
        int count = 0;
        m_stats.TryGetValue(sequence, out count);
        m_stats[sequence] = count + 1;
    }

    private static Dictionary<KeySequence, int> m_stats
        = new Dictionary<KeySequence, int>();
}

}
