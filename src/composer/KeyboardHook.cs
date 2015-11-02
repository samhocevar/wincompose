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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinCompose
{

static class KeyboardHook
{
    public static void Init()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT
             || Environment.OSVersion.Platform == PlatformID.Win32S
             || Environment.OSVersion.Platform == PlatformID.Win32Windows
             || Environment.OSVersion.Platform == PlatformID.WinCE)
        {
            m_callback = OnKey; // Keep a reference on OnKey
            m_hook = NativeMethods.SetWindowsHookEx(WH.KEYBOARD_LL, m_callback,
                                   NativeMethods.LoadLibrary("user32.dll"), 0);
            if (m_hook == HOOK.INVALID)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public static void Fini()
    {
        // XXX: this will crash if called from the GC Finalizer Thread because
        // the hook needs to be removed from the same thread that installed it.
        if (m_hook != HOOK.INVALID)
        {
            int ret = NativeMethods.UnhookWindowsHookEx(m_hook);
            if (ret == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            m_hook = HOOK.INVALID;
        }
        m_callback = null;
    }

    private static CALLBACK m_callback;
    private static HOOK m_hook;

    private static int OnKey(HC nCode, WM wParam, IntPtr lParam)
    {
        bool is_key = (wParam == WM.KEYDOWN || wParam == WM.SYSKEYDOWN
                        || wParam == WM.KEYUP || wParam == WM.SYSKEYUP);

        if (nCode == HC.ACTION && is_key)
        {
            // Retrieve key event data from native structure
            var data = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam,
                                                      typeof(KBDLLHOOKSTRUCT));
            bool is_injected = (data.flags & LLKHF.INJECTED) != 0;

            Log.Debug("{0}: OnKey(HC.{1}, WM.{2}, [vk:0x{3:X02} sc:0x{4:X02} flags:{5}])",
                      is_injected ? "Ignored Event" : "Event",
                      nCode, wParam, (int)data.vk, (int)data.sc, data.flags);

            if (!is_injected)
            {
                if (Composer.OnKey(wParam, data.vk, data.sc, data.flags))
                {
                    // Do not process further: that key was for us.
                    return -1;
                }
            }
        }
        else
        {
            Log.Debug("Ignored Event: OnKey({0}, {1})", nCode, wParam);
        }

        return NativeMethods.CallNextHookEx(m_hook, nCode, wParam, lParam);
    }
}

}
