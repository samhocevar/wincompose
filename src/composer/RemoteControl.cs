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
    public class RemoteControl : Window
    {
        public RemoteControl()
        {
            // Cannot set ShowInTaskbar = false, or WndProc will not be handled
            // correctly (maybe because we use HWND_BROADCAST).
            ShowActivated = false;
            Width = Height = 0;
            WindowState = WindowState.Minimized;
            WindowStyle = WindowStyle.None;

            SourceInitialized += (o, e) =>
                (PresentationSource.FromVisual(this) as HwndSource).AddHook(WndProc);

            Show();
            Hide();
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        protected IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_WINCOMPOSE.DISABLE)
            {
                if (Process.GetCurrentProcess().Id != (int)wParam)
                    DisableEvent?.Invoke();
                handled = true;
            }
            else if (msg == WM_WINCOMPOSE.EXIT)
            {
                if (Process.GetCurrentProcess().Id != (int)wParam)
                    ExitEvent?.Invoke();
                handled = true;
            }
            else if (msg == WM_WINCOMPOSE.SETTINGS)
            {
                SettingsEvent?.Invoke();
                handled = true;
            }
            else if (msg == WM_WINCOMPOSE.SEQUENCES)
            {
                SequencesEvent?.Invoke();
                handled = true;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Send a message to all other processes to ask them to disable any
        /// WinCompose hooks they may have installed.
        /// </summary>
        public void BroadcastDisableEvent()
        {
            NativeMethods.PostMessage(HWND.BROADCAST, WM_WINCOMPOSE.DISABLE,
                                      Process.GetCurrentProcess().Id, 0);
        }

        public event Action DisableEvent;
        public event Action ExitEvent;
        public event Action SettingsEvent;
        public event Action SequencesEvent;
    }
}

