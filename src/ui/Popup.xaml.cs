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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;

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

            // Seems to be the simplest way to implement an async event
            m_timer = new DispatcherTimer();
            m_timer.Tick += OnKeyInternal;

            Loaded += (o, e) => Composer.Key += OnKey;
            Closed += (o, e) => Composer.Key -= OnKey;
        }

        public void OnKey() => m_timer.Start();

        private void OnKeyInternal(object sender, EventArgs e)
        {
            m_timer.Stop();

            Rect caret;
            if (!Composer.IsComposing || (caret = GetCaretInfo()).IsEmpty)
            {
                Hide();
                return;
            }

            // Position popup near the cursor
            var ps = PresentationSource.FromVisual(this);
            var mat = ps.CompositionTarget.TransformFromDevice;
            var pos = mat.Transform(new Point(caret.Left - 5, caret.Bottom + 5));
            Left = pos.X;
            Top = pos.Y;

            PopupText.Text = string.Format("({0}, {1}) {2}x{3}",
                    caret.Left, caret.Top, caret.Width, caret.Height);
            Show();
        }

        private DispatcherTimer m_timer;

        private Rect GetCaretInfo()
        {
            List<uint> tid_list = new List<uint>();

#if false
            // This code tries to list all possible threads in case one of
            // them has an hwndCaret, but it doesn’t really improve things
            // with Visual Studio or Qt applications.
            IntPtr win = NativeMethods.GetForegroundWindow();
            uint pid;
            NativeMethods.GetWindowThreadProcessId(win, out pid);
            IntPtr th32s = NativeMethods.CreateToolhelp32Snapshot(TH32CS.SNAPTHREAD, pid);
            if (th32s != IntPtr.Zero)
            {
                THREADENTRY32 te = new THREADENTRY32();
                te.dwSize = (uint)Marshal.SizeOf(te);
                if (NativeMethods.Thread32First(th32s, out te))
                {
                    do
                    {
                        if (te.th32OwnerProcessID == pid)
                        {
                            tid_list.Add(te.th32ThreadID);
                        }
                        te.dwSize = (uint)Marshal.SizeOf(te);
                    }
                    while (NativeMethods.Thread32Next(th32s, out te));
                }
                NativeMethods.CloseHandle(th32s);
            }
#else
            tid_list.Add(0);
#endif

            GUITHREADINFO guiti = new GUITHREADINFO();
            guiti.cbSize = (uint)Marshal.SizeOf(guiti);

            foreach (var tid in tid_list)
            {
                NativeMethods.GetGUIThreadInfo(tid, ref guiti);
                if (guiti.hwndCaret != IntPtr.Zero)
                    break;
            }

            if (guiti.hwndCaret == IntPtr.Zero)
            {
#if false
                foreach (var tid in tid_list)
                {
                    NativeMethods.GetGUIThreadInfo(tid, ref guiti);
                    Log.Debug($"tid {tid}: hwnd {guiti.hwndFocus}");
                    var root = AutomationElement.FromHandle(guiti.hwndFocus);
                    var ctrl = root.FindAll(TreeScope.Descendants, new Condi
                }
#endif

                return GetCaret(AutomationElement.FocusedElement) ?? new Rect();
            }

            // Window position in screen coordinates
            POINT window_pos = new POINT();
            NativeMethods.ClientToScreen(guiti.hwndCaret, out window_pos);

            var x = guiti.rcCaret.left + window_pos.x;
            var y = guiti.rcCaret.top + window_pos.y;
            var w = guiti.rcCaret.right - guiti.rcCaret.left;
            var h = guiti.rcCaret.bottom - guiti.rcCaret.top;

            return new Rect(x, y, w, h);
        }

        private static Rect? GetCaret(AutomationElement elem)
        {
            Log.Debug($"  elem: {elem}");
            var current = elem.Current;
            Log.Debug($"  name: {current.Name}");
            Log.Debug($"  type: {current.ControlType}");
            foreach (var prop in elem.GetSupportedProperties())
            {
                //if (prop.ProgrammaticName.Contains("Keyboard"))
                if (prop.ProgrammaticName.Contains("Value") || prop.ProgrammaticName.Contains("Bounding"))
                    Log.Debug($"    prop: {prop.ProgrammaticName} = {elem.GetCurrentPropertyValue(prop)}");
            }

            // Find an edit control in there
            // This is ultra slow! Find something else.
            // See discussion here: https://social.msdn.microsoft.com/Forums/SECURITY/en-US/485b5ea0-ca07-4b02-9efc-3d7354c585d2/performance-issue-on-findfirst-and-findall-calls-of-systemwindowsautomation-while-finding-the?forum=netfxbcl
            var l = elem.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
            foreach (AutomationElement e in l)
            {
                var b = GetCaret(e);
                if (b != null)
                    return b;
            }

            if (elem != null)
            {
                var bbox = elem.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty, true);
                if (bbox != AutomationElement.NotSupported)
                    return (Rect)bbox;
            }

            return null;
        }
    }
}
