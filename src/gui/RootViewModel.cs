using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace WinCompose.gui
{
    public class RootViewModel
    {
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
            categories.Sort((x, y) => string.Compare(x.Name, y.Name));
            Categories = categories;

            var sequences = new List<SequenceViewModel>();
            foreach (var sequence in Settings.GetSequences().Values)
            {
                // TODO: optimize me
                CategoryViewModel category = null;
                foreach (var cat in Categories)
                {
                    if (cat.RangeStart < sequence.m_result[0])
                    {
                        category = cat;
                        break;
                    }
                }
                sequences.Add(new SequenceViewModel(category, sequence));
            }
            Sequences = sequences;
        }

        public IEnumerable<CategoryViewModel> Categories { get; private set; }

        public IEnumerable<SequenceViewModel> Sequences { get; private set; } 
    }
}