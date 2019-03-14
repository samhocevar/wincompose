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

using System.Globalization;
using System.Threading;

namespace WinCompose
{
    public class SequenceViewModel
    {
        public static Key SpaceKey = new Key(" ");

        public SequenceViewModel(SequenceDescription desc) => m_desc = desc;

        public CategoryViewModel Category { get; set; }
        public CategoryViewModel EmojiCategory { get; set; }

        /// <summary>
        /// Return the sequence result in an UTF-16 string
        /// </summary>
        public string Result => m_desc.Result;

        /// <summary>
        /// Return the sequence Unicode codepoint. If the sequence contains
        /// zero, two or more characters, return -1.
        /// </summary>
        public string CodePoint => (m_desc.Utf32 == -1) ? "" : $"U+{(m_desc.Utf32):X04}";

        public int UnicodeCategory => m_desc.Utf32 == -1 ? -1 : (int)CharUnicodeInfo.GetUnicodeCategory(Result, 0);

        public string Description => m_desc.Description;

        public KeySequence Sequence => m_desc.Sequence;

        public bool Match(SearchQuery query)
        {
            if (query.IsEmpty)
                return true;

            if (query.ExactSearchString == Result)
                return true;

            var compare_info = Thread.CurrentThread.CurrentCulture.CompareInfo;

            // Ensure this sequence matches all the tokens (implicit AND)
            foreach (var token in query.Tokens)
            {
                if (token.Num == m_desc.Utf32 || token.HexNum == m_desc.Utf32)
                    continue;
                if (compare_info.IndexOf(Description, token.Text, CompareOptions.IgnoreCase) != -1)
                    continue;
                if (Sequence.ToString().Contains(token.Text))
                    continue;

                return false;
            }

            return true;
        }

        private SequenceDescription m_desc;
    }
}
