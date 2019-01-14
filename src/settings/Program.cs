//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
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
    static class Program
    {
        [STAThread]
        static void Main()
        {
            NativeMethods.PostMessage(HWND.BROADCAST, WM_WINCOMPOSE.SETTINGS, 0, 0);
        }
    }
}

