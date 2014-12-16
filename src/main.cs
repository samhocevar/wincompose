// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//   This program is free software; you can redistribute it and/or
//   modify it under the terms of the Do What The Fuck You Want To
//   Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.using System;

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace wincompose
{
    static class program
    {
        [STAThread]
        static void Main()
        {
            keyboardhook.init();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var gui = new gui();
            Application.Run();
            gui.Dispose();

            keyboardhook.fini();
        }
    }
}
