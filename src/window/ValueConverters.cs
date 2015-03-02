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
using System.Windows.Data;
using System.Windows.Markup;
using WinCompose.Properties; // for UnicodeCategories

namespace WinCompose
{
    public abstract class ValueConverter<T> : MarkupExtension, IValueConverter where T : class, new()
    {
        private static T instance;

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ?? (instance = new T());
        }
    }

    public abstract class OneWayValueConverter<T> : ValueConverter<T> where T : class, new()
    {
        public sealed override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class UnicodeCategoryConverter : OneWayValueConverter<UnicodeCategoryConverter>
    {
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

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cat = (UnicodeCategory)value;
            string result;
            return Strings.TryGetValue(cat, out result) ? result : cat.ToString();
        }
    }

    public class ComposeKeyValueConverter : OneWayValueConverter<ComposeKeyValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = (Key)value;
            var name = string.Format("Key{0}", key.VirtualKey);
            // We use reflection so we can perform a case-insensitive lookup
            var property = typeof(Resources).GetProperty(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
            return property != null ? property.GetValue(null, new object[0]) : value.ToString();
        }
    }
}
