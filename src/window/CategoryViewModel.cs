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

namespace WinCompose
{
    public class CategoryViewModel : ViewModelBase
    {
        private bool isSelected;
        private bool isEmpty = true;

        public CategoryViewModel(string name, int start, int end)
        {
            Name = name;
            RangeStart = start;          
            RangeEnd = end;
        }

        public string Name { get; private set; }

        public int RangeStart { get; private set; }
        
        public int RangeEnd { get; private set; }

        public bool IsSelected { get { return isSelected; } set { SetValue(ref isSelected, value, "IsSelected", RefreshFilter); } }

        public bool IsEmpty { get { return isEmpty; } set { SetValue(ref isEmpty, value, "IsEmpty"); } }

        private static void RefreshFilter(bool obj)
        {
            RootViewModel.Instance.RefreshFilters();
        }
    }
}
