//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2018 Sam Hocevar <sam@hocevar.net>
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace WinCompose
{
    public class RootViewModel : ViewModelBase
    {
        public RootViewModel()
        {
            PropertyChanged += PropertyChangedCallback;

            ActiveCategoryArray = new ObservableCollection<bool>(new List<bool>() { true, false, false, false });
            ActiveCategoryArray.CollectionChanged += (o, e)
                => PropertyChangedCallback(o, new PropertyChangedEventArgs(nameof(ActiveCategoryArray)));

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var category_lut = new Dictionary<Category, CategoryViewModel>();

            // Fill category list
            foreach (var category in CodepointCategory.AllCategories)
                m_categories.Add(category_lut[category] = new CategoryViewModel(this, category));

            foreach (var category in EmojiCategory.AllCategories)
                m_categories.Add(category_lut[category] = new CategoryViewModel(this, category));

            var macro_viewmodel = new CategoryViewModel(this, new MacroCategory("User Macros"));
            m_categories.Add(macro_viewmodel);

            // Compute a list of sorted codepoint categories for faster lookups
            var sorted_categories = new SortedList<int, CategoryViewModel>();
            foreach (var category in m_categories)
                if (category.RangeEnd > 0)
                    sorted_categories.Add(category.RangeEnd, category);

            // Fill sequence list and assign them a category
            foreach (var desc in Settings.GetSequenceDescriptions())
            {
                CategoryViewModel main_viewmodel = null, emoji_viewmodel = null;

                var emoji_category = EmojiCategory.FromEmojiString(desc.Result);
                if (emoji_category != null)
                {
                    category_lut.TryGetValue(emoji_category, out emoji_viewmodel);
                    emoji_viewmodel.IsEmpty = false;
                }

                // TODO: optimize me
                if (desc.Utf32 != -1)
                {
                    foreach (var kv in sorted_categories)
                        if (kv.Key >= desc.Utf32)
                        {
                            main_viewmodel = kv.Value;
                            main_viewmodel.IsEmpty = false;
                            break;
                        }
                }
                else if (emoji_category == null)
                {
                    macro_viewmodel.IsEmpty = false;
                    main_viewmodel = macro_viewmodel;
                }

                m_sequences.Add(new SequenceViewModel(desc)
                {
                    Category = main_viewmodel,
                    EmojiCategory = emoji_viewmodel,
                });
            }

            RefreshCategoryFilters();
            RefreshSequenceFilters();
        }

        ~RootViewModel()
        {
            PropertyChanged -= PropertyChangedCallback;
        }

        private void PropertyChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchText))
            {
                ActiveCategoryArray[(int)ActiveCategory] = false;
                ActiveCategoryArray[3] = true;
                RefreshSequenceFilters();
            }
            else if (e.PropertyName == nameof(ActiveCategoryArray))
            {
                ActiveCategory = (CategoryFilter)ActiveCategoryArray.IndexOf(true);
                RefreshCategoryFilters();
            }
        }

        /// <summary>
        /// A category viewmodel should call this when it gets selected
        /// </summary>
        /// <param name="selected_category"></param>
        public void OnCategorySelected(CategoryViewModel selected_category)
        {
            // Deselect any previously selected category
            foreach (var category in m_categories)
                if (category != selected_category)
                    category.IsSelected = false;

            RefreshSequenceFilters();
        }

        /// <summary>
        /// This pattern handles a 4-state radio button array
        /// </summary>
        public ObservableCollection<bool> ActiveCategoryArray { get; set; }
        public CategoryFilter ActiveCategory { get; set; } = 0;

        public enum CategoryFilter : int
        {
            Unicode = 0,
            Emoji = 1,
            Macros = 2,
            Search = 3,
        };

        public IEnumerable<CategoryViewModel> Categories => m_categories;
        public IEnumerable<SequenceViewModel> Sequences => m_sequences;

        private IList<CategoryViewModel> m_categories = new ObservableCollection<CategoryViewModel>();
        private IList<SequenceViewModel> m_sequences = new ObservableCollection<SequenceViewModel>();

        public string SearchText
        {
            get => m_search_text;
            set => SetValue(ref m_search_text, value, nameof(SearchText));
        }

        private string m_search_text = "";
        private SearchTokens m_search_tokens;

        private void RefreshCategoryFilters()
        {
            var category_view = CollectionViewSource.GetDefaultView(Categories);
            category_view.Filter = (o) =>
            {
                var category = o as CategoryViewModel;
                switch (ActiveCategory)
                {
                    case CategoryFilter.Unicode: return category.IsUnicode;
                    case CategoryFilter.Emoji: return category.IsEmoji;
                    case CategoryFilter.Macros: return category.IsMacro;
                    case CategoryFilter.Search: return false;
                }
                return false;
            };
            category_view.Refresh();
        }

        private void RefreshSequenceFilters()
        {
            var sequence_view = CollectionViewSource.GetDefaultView(Sequences);
            m_search_tokens = new SearchTokens(SearchText);
            sequence_view.Filter = FilterSequences;
            sequence_view.Refresh();
        }

        private bool FilterSequences(object obj)
        {
            var sequence = obj as SequenceViewModel;
            switch (ActiveCategory)
            {
                case CategoryFilter.Unicode:
                    return !(sequence.Category?.IsMacro ?? false) && (sequence.Category?.IsSelected ?? false);
                case CategoryFilter.Emoji:
                    return sequence.EmojiCategory?.IsSelected ?? false;
                case CategoryFilter.Macros:
                    return (sequence.Category?.IsMacro ?? false) && (sequence.Category?.IsSelected ?? false);
                case CategoryFilter.Search:
                    return sequence.Match(m_search_tokens);
                default:
                    return false;
            }
        }
    }
}
