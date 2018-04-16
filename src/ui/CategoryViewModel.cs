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

using System.ComponentModel;
using System.Windows;

namespace WinCompose
{
    public class CategoryViewModel : ViewModelBase
    {
        public CategoryViewModel(RootViewModel root, Category category)
        {
            m_root = root;
            m_category = category;
            PropertyChanged += PropertyChangedCallback;
        }

        ~CategoryViewModel()
        {
            PropertyChanged -= PropertyChangedCallback;
        }

        public Visibility Visibility => IsEmpty ? Visibility.Collapsed : Visibility.Visible;

        public string Name => m_category.Name;
        public string Icon => m_category.Icon;
        public int RangeStart => m_category.RangeStart;
        public int RangeEnd => m_category.RangeEnd;

        public bool IsUnicode => RangeEnd > 0;
        public bool IsEmoji => m_category is EmojiCategory;
        public bool IsMacro => m_category is MacroCategory;

        public bool IsSelected { get { return m_is_selected; } set { SetValue(ref m_is_selected, value, nameof(IsSelected)); } }
        public bool IsEmpty { get { return m_is_empty; } set { SetValue(ref m_is_empty, value, nameof(IsEmpty)); } }

        private bool m_is_selected;
        private bool m_is_empty = true;
        private RootViewModel m_root;
        private Category m_category;

        private void PropertyChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsSelected))
                m_root.RefreshSequenceFilters();
        }
    }
}
