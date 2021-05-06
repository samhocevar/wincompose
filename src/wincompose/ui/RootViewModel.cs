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
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var category_lut = new Dictionary<Category, CategoryViewModel>();

            // Fill the category tree
            m_favorites = new CategoryViewModel(this, new TopCategory(i18n.Text.Favorites + " ⭐"));
            m_categories.Add(m_favorites);

            var all_unicode_vm = new CategoryViewModel(this, new TopCategory(i18n.Text.UnicodeCharacters + " — ŵ Ǿ à Ꝏ Σ ⍾ ꟿ"));
            foreach (var c in CodepointCategory.AllCategories)
                all_unicode_vm.Children.Add(category_lut[c] = new CategoryViewModel(this, c));
            m_categories.Add(all_unicode_vm);

            var all_emoji_vm = new CategoryViewModel(this, new TopCategory(i18n.Text.Emoji + " ☺🍄🐨"));
            foreach (var c in EmojiCategory.AllCategories)
                all_emoji_vm.Children.Add(category_lut[c] = new CategoryViewModel(this, c));
            m_categories.Add(all_emoji_vm);

            m_macros = new CategoryViewModel(this, new TopCategory(i18n.Text.UserMacros + " ( ͡° ͜ʖ ͡°)"));
            m_categories.Add(m_macros);

            // Compute a list of sorted codepoint categories for faster lookups
            var sorted_categories = new SortedList<int, CategoryViewModel>();
            foreach (var category in all_unicode_vm.Children)
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

                m_sequences.Add(new SequenceViewModel(desc)
                {
                    UnicodeCategoryVM = main_viewmodel,
                    EmojiCategoryVM = emoji_viewmodel,
                });
            }

            RefreshSequenceFilters();
        }

        ~RootViewModel()
        {
            PropertyChanged -= PropertyChangedCallback;
        }

        private CategoryViewModel m_favorites;
        private CategoryViewModel m_macros;

        private void PropertyChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchText))
            {
                m_search_query = new SearchQuery(SearchText);
                RefreshSequenceFilters();
            }
        }

        /// <summary>
        /// A category viewmodel should call this when it gets selected
        /// </summary>
        /// <param name="selected_category"></param>
        public void OnCategorySelected(CategoryViewModel selected_category)
        {
            if (selected_category.Children.Count == 0)
            {
                m_search_query = null;
                RefreshSequenceFilters();
            }
        }

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
        private SearchQuery m_search_query;

        private void RefreshSequenceFilters()
        {
            var sequence_view = CollectionViewSource.GetDefaultView(Sequences);
            sequence_view.Filter = FilterSequences;
            sequence_view.Refresh();
        }

        private bool FilterSequences(object obj)
        {
            var sequence = obj as SequenceViewModel;
            if (m_search_query != null)
                return sequence.Match(m_search_query);
            return (sequence.UnicodeCategoryVM?.IsSelected ?? false)
                || (sequence.EmojiCategoryVM?.IsSelected ?? false)
                || (m_favorites.IsSelected && sequence.IsFavorite)
                || (m_macros.IsSelected && sequence.IsMacro);
        }
    }
}
