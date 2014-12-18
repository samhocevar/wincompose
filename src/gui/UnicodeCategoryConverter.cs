using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;

namespace WinCompose.gui
{
    public class UnicodeCategoryConverter : MarkupExtension, IValueConverter
    {
        private static UnicodeCategoryConverter instance;
        private static readonly Dictionary<UnicodeCategory, string> Strings = new Dictionary<UnicodeCategory, string>();

        static UnicodeCategoryConverter()
        {
            foreach (var value in Enum.GetValues(typeof(UnicodeCategory)))
            {
                var name = value.ToString();
                var prop = typeof(UnicodeCategories).GetProperty(name, BindingFlags.Static | BindingFlags.Public);
                var desc = prop.GetValue(null, null);
                Strings.Add((UnicodeCategory)value, (string)desc);
            }
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new UnicodeCategoryConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cat = (UnicodeCategory)value;
            string result;
            return Strings.TryGetValue(cat, out result) ? result : cat.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}