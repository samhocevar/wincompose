using System;
using System.Collections.Generic;
using System.Reflection;

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
                var start = Convert.ToInt32(property.Name.Substring(1, 4), 16);
                var end = Convert.ToInt32(property.Name.Substring(6, 4), 16);
                categories.Add(new CategoryViewModel(name, start, end));
            }
            categories.Sort((x, y) => string.Compare(x.Name, y.Name));
            Categories = categories;

            var seq = new List<SequenceViewModel>();
            for (char c = ' '; c < 'Ǘ'; ++c)
            {
                seq.Add(new SequenceViewModel(categories[0], c, "Lorem ipsum Lorem ipsum Lorem ipsum Lorem ipsum " + c));
            }
            Sequences = seq;
        }

        public IEnumerable<CategoryViewModel> Categories { get; private set; }

        public IEnumerable<SequenceViewModel> Sequences { get; private set; } 
    }
}