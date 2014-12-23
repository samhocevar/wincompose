using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Data;
using System.Xml;

namespace WinCompose.gui
{
    public class RootViewModel : ViewModelBase
    {
        private string searchText;
        private SearchTokens searchTokens = new SearchTokens(null);

        public RootViewModel()
        {
            var categories = new List<CategoryViewModel>();
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
            foreach (var property in typeof(UnicodeBlocks).GetProperties(flags))
            {
                if (property.Name == "ResourceManager" || property.Name == "Culture")
                    continue;

                var name = (string)property.GetValue(null, null);
                var range = property.Name.Split(new[] { 'U' });
                var start = Convert.ToInt32(range[1], 16);
                var end = Convert.ToInt32(range[2], 16);
                categories.Add(new CategoryViewModel(name, start, end));
            }
            categories.Sort((x, y) => string.Compare(x.Name, y.Name, Thread.CurrentThread.CurrentCulture, CompareOptions.StringSort));

            var sortedCategories = new SortedList<int, CategoryViewModel>();
            foreach (var category in categories)
            {
                sortedCategories.Add(category.RangeEnd, category);
            }

            var sequences = new List<SequenceViewModel>();
            foreach (var sequence in Settings.GetSequences().Values)
            {
                // TODO: optimize me
                foreach (var category in sortedCategories)
                {
                    if (category.Key > sequence.m_result[0])
                    {
                        sequences.Add(new SequenceViewModel(category.Value, sequence));
                        break;
                    }
                }
            }
            var nonEmptyCategories = new List<CategoryViewModel>();
            foreach (var category in categories)
            {
                if (!category.IsEmpty)
                    nonEmptyCategories.Add(category);
            }
            Categories = nonEmptyCategories;

            Sequences = sequences;
            Instance = this;
            var collectionView = CollectionViewSource.GetDefaultView(Sequences);
            collectionView.Filter = FilterFunc;
        }

        public static RootViewModel Instance { get; private set; }

        public IEnumerable<CategoryViewModel> Categories { get; private set; }

        public IEnumerable<SequenceViewModel> Sequences { get; private set; }

        public string SearchText { get { return searchText; } set { SetValue(ref searchText, value, "SearchText", RefreshFilters); } }

        public void RefreshFilters()
        {
            var collectionView = CollectionViewSource.GetDefaultView(Sequences);
            collectionView.Refresh();
        }

        private void RefreshFilters(string text)
        {
            searchTokens = new SearchTokens(text);
            RefreshFilters();
        }

        private bool FilterFunc(object obj)
        {
            var sequence = (SequenceViewModel)obj;
            return sequence.Category.IsSelected && sequence.Match(searchTokens);
        }
    }
}