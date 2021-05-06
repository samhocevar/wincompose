//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2021 Sam Hocevar <sam@hocevar.net>
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
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace WinCompose
{
    public class Category
    {
        public string Name { get; protected set; }
        public string Icon { get; protected set; }

        public int RangeStart { get; protected set; } = -1;
        public int RangeEnd { get; protected set; } = -1;
    }

    public class TopCategory : Category
    {
        public TopCategory(string name)
        {
            Name = name;
        }
    }

    public class EmojiCategory : Category
    {
        public static readonly IEnumerable<EmojiCategory> AllCategories = GetAllCategories();

        public static EmojiCategory FromEmojiString(string str)
        {
            return LUT.TryGetValue(str, out var ret) ? ret : null;
        }

        private static IDictionary<string, EmojiCategory> LUT;

        private static IEnumerable<EmojiCategory> GetAllCategories()
        {
            LUT = new Dictionary<string, EmojiCategory>();

            var match_group = new Regex(@"^# group: (.*)");
            var match_sequence = new Regex(@"^([0-9A-F ]+[0-9A-F]).*");
            var list = new List<EmojiCategory>();

            using (var sr = new GZipResourceStream("emoji-test.txt.gz"))
            {
                EmojiCategory last_category = null;
                string buffer = sr.ReadToEnd();
                foreach (var line in buffer.Split('\r', '\n'))
                {
                    var m = match_group.Match(line);
                    if (m.Success)
                    {
                        last_category = new EmojiCategory() { Name = m.Groups[1].ToString() };
                        list.Add(last_category);
                    }

                    m = match_sequence.Match(line);
                    if (m.Success)
                    {
                        string value = "";
                        foreach (var codepoint in m.Groups[1].ToString().Split(' '))
                            value += char.ConvertFromUtf32(Convert.ToInt32(codepoint, 16));
                        LUT[value] = last_category;

                        // Set icon for this category
                        if (string.IsNullOrEmpty(last_category.Icon))
                            last_category.Icon = value;
                    }
                }
            }

            return list;
        }
    }

    public class CodepointCategory : Category
    {
        public static readonly IEnumerable<CodepointCategory> AllCategories = GetAllCategories();

        private static IEnumerable<CodepointCategory> GetAllCategories()
        {
            var list = new List<CodepointCategory>();

            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            Regex r = new Regex(@"^U([a-fA-F0-9]*)_U([a-fA-F0-9]*)$");
            foreach (var property in typeof(unicode.Block).GetProperties(flags))
            {
                Match m = r.Match(property.Name);
                if (m.Success)
                {
                    var name = (string)property.GetValue(null, null);
                    var start = Convert.ToInt32(m.Groups[1].Value, 16);
                    var end = Convert.ToInt32(m.Groups[2].Value, 16);
                    list.Add(new CodepointCategory() { Name = name, RangeStart = start, RangeEnd = end });
                }
            }

            list.Sort((x, y) => string.Compare(x.Name, y.Name, Thread.CurrentThread.CurrentCulture, CompareOptions.StringSort));
            return list;
        }
    }
}
