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
using System.Diagnostics;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Interop;

namespace WinCompose
{
    public class WM_WINCOMPOSE
    {
        /// <summary>
        /// A custom message ID used to kill other WinCompose instances
        /// </summary>
        public static readonly uint EXIT
            = NativeMethods.RegisterWindowMessage("WM_WINCOMPOSE_EXIT");

        /// <summary>
        /// A custom message ID used to disable other WinCompose instances
        /// </summary>
        public static readonly uint DISABLE
            = NativeMethods.RegisterWindowMessage("WM_WINCOMPOSE_DISABLE");

        /// <summary>
        /// A custom message ID used to open the WinCompose settings window
        /// </summary>
        public static readonly uint SETTINGS
            = NativeMethods.RegisterWindowMessage("WM_WINCOMPOSE_SETTINGS");
    }
}

