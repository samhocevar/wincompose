//
// WinCompose — a compose key for Windows
//
// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//                     2014 Benjamin Litzelmann
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What the Fuck You Want
//   to Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.

using System;
using WinCompose.gui;

namespace WinCompose
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Settings.LoadConfig();
            Settings.LoadSequences();
            KeyboardHook.Install();

            try
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
            finally
            {
                KeyboardHook.Uninstall();
                Settings.SaveConfig();
            }
        }
    }
}
