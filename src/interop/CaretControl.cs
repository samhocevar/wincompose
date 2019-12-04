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
using System.Windows.Controls;

namespace WinCompose
{
    public class CaretControl
    {
        public static Rect GetInfo(Control ctrl)
        {
            var rect = GetNativeCaret() ?? GetAutomationCaret();

            if (rect == null)
                return new Rect();

            var r = (Rect)rect;

            // Position popup near the cursor
            var ps = PresentationSource.FromVisual(ctrl);
            var mat = ps.CompositionTarget.TransformFromDevice;
            var p1 = mat.Transform(new Point(r.X, r.Y));
            var p2 = mat.Transform(new Point(r.X + r.Width, r.Y + r.Height));
            return new Rect(p1, p2);
        }

        private static Rect? GetNativeCaret()
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
                return null;

            NativeMethods.GetWindowRect(guiti.hwndCaret, out var r);
            return new Rect(r.left, r.top, r.right - r.left, r.bottom - r.top);

            // Caret position in screen coordinates
            POINT p1 = new POINT() { x = guiti.rcCaret.left, y = guiti.rcCaret.top };
            POINT p2 = new POINT() { x = guiti.rcCaret.right, y = guiti.rcCaret.bottom };
            NativeMethods.ClientToScreen(guiti.hwndCaret, ref p1);
            NativeMethods.ClientToScreen(guiti.hwndCaret, ref p2);
            Log.Debug($"Caret {guiti.rcCaret.left},{guiti.rcCaret.top} -> {p1.x},{p1.y}");

            return new Rect(p1.x, p1.y, p2.x - p1.x, p2.y - p1.y);
        }

        private static Rect? GetAutomationCaret()
        {
            return GetCaret(AutomationElement.FocusedElement);
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
