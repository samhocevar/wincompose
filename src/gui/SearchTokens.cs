using System;
using System.Collections.Generic;

namespace WinCompose.gui
{
    public class SearchTokens
    {
        private static readonly List<char> HexaDigits = new List<char>("ABCDEFabcdef");
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
            int numberStart = int.MinValue;
            for (int i = 0; i < searchText.Length; ++i)
            {
                if (char.IsDigit(searchText[i]))
                {
                    if (numberStart == int.MinValue)
                        numberStart = i;
                }
                else
                {
                    if (numberStart != int.MinValue)

                    {
                        numberStart = i;
                        var token = searchText.Substring(numberStart, i - numberStart + 1);
                        numbers.Add(Convert.ToInt32(token, 10));
                        numberStart = int.MinValue;
                    }
                }
            }

            // base 16 numbers
            numberStart = int.MinValue;
            for (int i = 0; i < searchText.Length; ++i)
            {
                if (char.IsDigit(searchText[i]) || HexaDigits.Contains(searchText[i]))
                {
                    if (numberStart == int.MinValue)
                        numberStart = i;
                }
                else
                {
                    if (numberStart != int.MinValue)
                    {
                        numberStart = i;
                        var token = searchText.Substring(numberStart, i - numberStart + 1);
                        numbers.Add(Convert.ToInt32(token, 10));
                        numberStart = int.MinValue;
                    }
                }
            }
        }

        public bool IsEmpty { get; private set; }

        public IEnumerable<string> Tokens { get { return tokens; } }

        public IEnumerable<int> Numbers { get { return numbers; } }
    }
}