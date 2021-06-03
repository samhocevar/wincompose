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
using System.Globalization;
using System.Text.RegularExpressions;

namespace WinCompose
{
    public class SearchToken
    {
        public SearchToken(string token)
        {
            if (token.StartsWith("\"") && token.EndsWith("\""))
                token = token.Trim(new[] { '"' });
            Text = token;

            // Interpret token as key, if applicable
            if (token.StartsWith("key:"))
                Key = Key.FromString(token.Substring(4));

            // Interpret token as decimal number, if applicable
            if (int.TryParse(token, out var num))
                Num = num;

            // Interpret token as hexadecimal number or Unicode codepoint, if applicable
            if (token.ToLower().StartsWith("u+") || token.ToLower().StartsWith("0x"))
                token = token.Substring(2);
            if (int.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexnum)
                 && hexnum != Num)
                HexNum = hexnum;
        }

        public string Text { get; private set; }
        public int Num { get; private set; } = int.MinValue;
        public int HexNum { get; private set; } = int.MinValue;
        public Key Key { get; private set; } = new Key(VK.NONE);
    }

    public class SearchQuery
    {
        public SearchQuery(string text)
        {
            ExactSearchString = text;

            foreach (Match m in m_split.Matches(text))
                m_tokens.Add(new SearchToken(m.Groups[1].ToString()));
        }

        public bool IsEmpty => m_tokens.Count == 0;
        public string ExactSearchString { get; private set; }
        public IEnumerable<SearchToken> Tokens => m_tokens;

        private readonly IList<SearchToken> m_tokens = new List<SearchToken>();

        // Split string along spaces except when inside ""
        private static readonly Regex m_split = new Regex(" *(([^ \"]|\"[^\"]*\"|\")+) *");
    }
}

