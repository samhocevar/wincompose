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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Data;

namespace WinCompose
{
    public class RootViewModel : ViewModelBase
    {
        private string m_search_text;
        private SearchTokens m_search_tokens = new SearchTokens(null);

        public RootViewModel()
        {
            var categories = new List<CategoryViewModel>();
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
                    categories.Add(new CategoryViewModel(name, start, end));
                }
            }
            categories.Add(new CategoryViewModel(i18n.Text.UserMacros, -1, -1));

            categories.Sort((x, y) => string.Compare(x.Name, y.Name, Thread.CurrentThread.CurrentCulture, CompareOptions.StringSort));

            var sortedCategories = new SortedList<int, CategoryViewModel>();
            foreach (var category in categories)
            {
                sortedCategories.Add(category.RangeEnd, category);
            }

            // FIXME: make this a utility function AddSequence() that also
            // creates and sorts categories on the fly.
            var sequences = new List<SequenceViewModel>();
            foreach (var desc in Settings.GetSequenceDescriptions())
            {
                // TODO: optimize me
                foreach (var category in sortedCategories)
                {
                    if (category.Key >= desc.Utf32)
                    {
                        sequences.Add(new SequenceViewModel(category.Value, desc));
                        break;
                    }
                }
            }

            var nonEmptyCategories = new List<CategoryViewModel>();
            foreach (var category in categories)
                if (!category.IsEmpty)
                    nonEmptyCategories.Add(category);

            Categories = nonEmptyCategories;

            Sequences = sequences;
            Instance = this;

            RefreshFilters();
        }

        public static RootViewModel Instance { get; private set; }

        private bool[] m_active_category_array = { true, false, false, false };
        public bool[] ActiveCategoryArray => m_active_category_array;
        public int ActiveCategory => Array.IndexOf(ActiveCategoryArray, true);

        public IEnumerable<CategoryViewModel> Categories { get; private set; }

        public IEnumerable<SequenceViewModel> Sequences { get; private set; }

        public string SearchText { get => m_search_text; set => SetValue(ref m_search_text, value, "SearchText", RefreshSearch); }

        public void RefreshFilters()
        {
            var collection_view = CollectionViewSource.GetDefaultView(Sequences);
            collection_view.Filter = FilterFunc;
            collection_view.Refresh();
        }

        private void RefreshSearch(string text)
        {
            m_search_tokens = new SearchTokens(text);
            RefreshFilters();
        }

        private bool FilterFunc(object obj)
        {
            var sequence = (SequenceViewModel)obj;
            if (m_search_tokens.IsEmpty)
                return sequence.Category.IsSelected;

            return sequence.Match(m_search_tokens);
        }
    }
}
