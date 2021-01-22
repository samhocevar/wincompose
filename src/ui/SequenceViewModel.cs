//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2021 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace WinCompose
{
    public class SequenceViewModel : ViewModelBase
    {
        public static Key SpaceKey = new Key(" ");

        public SequenceViewModel(SequenceDescription desc) => m_desc = desc;

        public CategoryViewModel UnicodeCategoryVM { get; set; }
        public CategoryViewModel EmojiCategoryVM { get; set; }

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

        public string RichDescription => (IsFavorite ? "⭐ " : "") + m_desc.Description;

        public IEnumerable<Key> FullSequence
            => new Key("♦").Yield().Concat(m_desc.Sequence);

        public Visibility AddToFavoritesVisibility
            => IsFavorite ? Visibility.Collapsed : Visibility.Visible;

        public Visibility RemoveFromFavoritesVisibility
            => IsFavorite ? Visibility.Visible : Visibility.Collapsed;

        public void ToggleFavorite()
        {
            Metadata.ToggleFavorite(m_desc.Sequence, Result);
            OnPropertyChanged(nameof(RichDescription));
            OnPropertyChanged(nameof(AddToFavoritesVisibility));
            OnPropertyChanged(nameof(RemoveFromFavoritesVisibility));
        }

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
                if (m_desc.Sequence.Contains(new Key(token.Text)))
                    continue;

                return false;
            }

            return true;
        }

        public bool IsMacro => UnicodeCategoryVM == null && EmojiCategoryVM == null;

        public bool IsFavorite => Metadata.IsFavorite(m_desc.Sequence, Result);

        private SequenceDescription m_desc;
    }
}
