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
using System.Globalization;
using System.Threading;

namespace WinCompose
{
    public class SequenceViewModel
    {
        public static Key SpaceKey = new Key(" ");

        public SequenceViewModel(CategoryViewModel category, SequenceDescription desc)
        {
            Category = category;
            Category.IsEmpty = false;
            Result = desc.Result;
            Description = desc.Description;
            Sequence = desc.Sequence;
            Utf32 = desc.Utf32;
        }

        public CategoryViewModel Category { get; private set; }

        /// <summary>
        /// Return the sequence result in an UTF-16 string
        /// </summary>
        public string Result { get; private set; }

        /// <summary>
        /// Return the sequence Unicode codepoint. If the sequence contains
        /// zero, two or more characters, return -1.
        /// </summary>
        public int Utf32 { get; private set; }

        public int UnicodeCategory { get { return Utf32 == -1 ? -1 : (int)CharUnicodeInfo.GetUnicodeCategory(Result, 0); } }

        public string Description { get; private set; }

        public KeySequence Sequence { get; set; }

        public bool Match(SearchTokens searchText)
        {
            if (searchText.IsEmpty)
                return true;

            var compareInfo = Thread.CurrentThread.CurrentCulture.CompareInfo;
            foreach (var token in searchText.Tokens)
            {
                if (compareInfo.IndexOf(Description, token, CompareOptions.IgnoreCase) != -1)
                    return true;
            }

            foreach (var number in searchText.Numbers)
            {
                if (Utf32 == number)
                    return true;
            }
            return false;
        }
    }
}
