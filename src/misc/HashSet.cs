//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2020 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Collections;
using System.Collections.Generic;

namespace WinCompose
{
    class HashSet<T> : IEnumerable<T>
    {
        public bool Add(T item)
            => !m_dict.ContainsKey(item) && (m_dict[item] = true);

        public void Clear()
            => m_dict.Clear();

        public bool Contains(T item)
            => m_dict.ContainsKey(item);

        public bool Remove(T item)
            => m_dict.Remove(item);

        public IEnumerator<T> GetEnumerator()
            => m_dict.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => (m_dict.Keys as IEnumerable).GetEnumerator();

        private IDictionary<T, bool> m_dict = new Dictionary<T, bool>();
    }
}

