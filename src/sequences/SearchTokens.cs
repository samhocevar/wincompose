//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2017 Sam Hocevar <sam@hocevar.net>
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
    public class SearchTokens
    {
        private static readonly IList<char> Digits = new List<char>("0123456789");
        private static readonly IList<char> HexDigits = new List<char>("0123456789ABCDEFabcdef");

        private readonly string[] m_tokens;
        private readonly IList<int> m_numbers = new List<int>();

        public SearchTokens(string searchText)
        {
            ExactSearchString = searchText;

            // string tokens
            m_tokens = searchText.Split(" \t\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // base 10 numbers and base 16 numbers
            ParseNumbers(searchText, Digits, 10, ref m_numbers);
            ParseNumbers(searchText, HexDigits, 16, ref m_numbers);
        }

        public bool IsEmpty => m_tokens.Length == 0;

        public string ExactSearchString { get; private set; }
        public IEnumerable<string> Tokens => m_tokens;
        public IEnumerable<int> Numbers => m_numbers;

        private static void ParseNumbers(string text, ICollection<char> digits, int numberBase, ref IList<int> resultList)
        {
            int numberStart = int.MinValue;
            for (int i = 0; i < text.Length; ++i)
            {
                if (digits.Contains(text[i]))
                {
                    if (numberStart == int.MinValue)
                        numberStart = i;
                }
                else if (numberStart != int.MinValue)
                {
                    var token = text.Substring(numberStart, i - numberStart);
                    resultList.Add(Convert.ToInt32(token, numberBase));
                    numberStart = int.MinValue;
                }

            }
            if (numberStart != int.MinValue)
            {
                var token = text.Substring(numberStart, text.Length - numberStart);
                resultList.Add(Convert.ToInt32(token, numberBase));
            }
        }
    }
}
