//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2015 Sam Hocevar <sam@hocevar.net>
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
using WinForms = System.Windows.Forms;

namespace WinCompose
{
    public class RemoteControl : WinForms.Form
    {
        public RemoteControl()
        {
            // Forcing access to the window handle will let us receive messages
            var unused = this.Handle;
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        protected override void WndProc(ref WinForms.Message m)
        {
            if (m.Msg == WM_WINCOMPOSE_DISABLE)
            {
                if (Process.GetCurrentProcess().Id != (int)m.WParam)
                    DisableEvent(null, new EventArgs());
                return;
            }
            else if (m.Msg == WM_WINCOMPOSE_EXIT)
            {
                if (Process.GetCurrentProcess().Id != (int)m.WParam)
                    ExitEvent(null, new EventArgs());
                return;
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Send a message to all other processes to ask them to disable any
        /// WinCompose hooks they may have installed.
        /// </summary>
        public void TriggerDisableEvent()
        {
            NativeMethods.PostMessage((IntPtr)0xffff, WM_WINCOMPOSE_DISABLE,
                                      Process.GetCurrentProcess().Id, 0);
        }

        public event EventHandler DisableEvent = delegate {};
        public event EventHandler ExitEvent = delegate {};

        /// <summary>
        /// A custom message ID used to kill other WinCompose instances
        /// </summary>
        private static readonly uint WM_WINCOMPOSE_EXIT
            = NativeMethods.RegisterWindowMessage("WM_WINCOMPOSE_EXIT");

        /// <summary>
        /// A custom message ID used to disable other WinCompose instances
        /// </summary>
        private static readonly uint WM_WINCOMPOSE_DISABLE
            = NativeMethods.RegisterWindowMessage("WM_WINCOMPOSE_DISABLE");
    }
}

