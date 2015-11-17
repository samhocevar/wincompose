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
    public class SearchTokens
    {
        private static readonly List<char> Digits = new List<char>("0123456789");
        private static readonly List<char> HexDigits = new List<char>("0123456789ABCDEFabcdef");

        private readonly string[] tokens;
        private readonly List<int> numbers = new List<int>();

        public SearchTokens(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                IsEmpty = true;
                tokens = new string[0];
                return;

            }
            // string tokens
            tokens = searchText.Split(" \t\r\n".ToCharArray());

            // base 10 numbers
            ParseNumbers(searchText, Digits, 10, ref numbers);

            // base 16 numbers
            ParseNumbers(searchText, HexDigits, 16, ref numbers);
        }

        public bool IsEmpty { get; private set; }

        public IEnumerable<string> Tokens { get { return tokens; } }

        public IEnumerable<int> Numbers { get { return numbers; } }

        private static void ParseNumbers(string text, ICollection<char> digits, int numberBase, ref List<int> resultList)
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
