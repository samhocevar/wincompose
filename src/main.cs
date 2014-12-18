// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//   This program is free software; you can redistribute it and/or
//   modify it under the terms of the Do What The Fuck You Want To
//   Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.using System;

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
