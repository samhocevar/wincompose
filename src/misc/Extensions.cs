//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2020 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;

namespace WinCompose
{
    internal static class Extensions
    {
        /// <summary>
        /// Return whether array contains a given element
        /// </summary>
        public static bool Contains<T>(this T[] array, T element)
            where T : IComparable<T>
            => Array.Find(array, e => element.CompareTo(e) == 0) != null;
    }
}
