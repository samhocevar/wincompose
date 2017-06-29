//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2017 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace WinCompose
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        public Popup()
        {
            ShowInTaskbar = false;
            InitializeComponent();

            // FIXME: remove Event Handler on destruction!
            Composer.KeyEvent += new EventHandler(OnKey);
        }

        public void OnKey(object sender, EventArgs e)
        {
            IntPtr win = NativeMethods.GetForegroundWindow();
            //uint tpid;
            //NativeMethods.GetWindowThreadProcessId(win, out tpid);
            GUITHREADINFO guiti = new GUITHREADINFO();
            guiti.cbSize = (uint)Marshal.SizeOf(guiti);
            //NativeMethods.GetGUIThreadInfo(tpid, ref guiti);
            NativeMethods.GetGUIThreadInfo(0, ref guiti);
            POINT point = new POINT();
            NativeMethods.ClientToScreen(guiti.hwndCaret, out point);
            int x = guiti.rcCaret.left + point.x;
            int y = guiti.rcCaret.top + point.y;
            int w = guiti.rcCaret.right - guiti.rcCaret.left;
            int h = guiti.rcCaret.bottom - guiti.rcCaret.top;

            PopupText.Text = string.Format("{0}: ({1}, {2}) {3}x{4}", win, x, y, w, h);
            Left = x + 5;
            Top = y + h + 15;
        }
    }
}
