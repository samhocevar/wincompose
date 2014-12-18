// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//   This program is free software; you can redistribute it and/or
//   modify it under the terms of the Do What The Fuck You Want To
//   Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.using System;

using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinCompose
{

static class KeyboardHook
{
    public static void Install()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT
             || Environment.OSVersion.Platform == PlatformID.Win32S
             || Environment.OSVersion.Platform == PlatformID.Win32Windows
             || Environment.OSVersion.Platform == PlatformID.WinCE)
        {
            m_callback = OnKey; // Keep a reference on OnKey
            m_hook = SetWindowsHookEx(WH.KEYBOARD_LL, m_callback,
                                      LoadLibrary("user32.dll"), 0);
            if (m_hook == HOOK.INVALID)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public static void Uninstall()
    {
        // XXX: this will crash if called from the GC Finalizer Thread because
        // the hook needs to be removed from the same thread that installed it.
        if (m_hook != HOOK.INVALID)
        {
            int ret = UnhookWindowsHookEx(m_hook);
            if (ret == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            m_hook = HOOK.INVALID;
        }
        m_callback = null;
    }

    private delegate int CALLBACK(HC nCode, WM wParam, IntPtr lParam);

    private static CALLBACK m_callback;
    private static HOOK m_hook;

    private static int OnKey(HC nCode, WM wParam, IntPtr lParam)
    {
        if (nCode == HC.ACTION)
        {
            // Retrieve event data from native structure
            var data = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam,
                                                      typeof(KBDLLHOOKSTRUCT));

            bool is_key = (wParam == WM.KEYDOWN || wParam == WM.SYSKEYDOWN
                            || wParam == WM.KEYUP || wParam == WM.SYSKEYUP);
            bool is_injected = (data.flags & LLKHF.INJECTED) == 0;

            if (is_key && !is_injected)
            {
                if (Compose.OnKey(wParam, data.vk, data.sc, data.flags))
                {
                    // Do not process further: that key was for us.
                    return -1;
                }
            }
        }

        return CallNextHookEx(m_hook, nCode, wParam, lParam);
    }

    /* Low-level keyboard input event.
     *   http://msdn.microsoft.com/en-us/library/windows/desktop/ms644967%28v=vs.85%29.aspx
     */
    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public VK vk;
        public SC sc;
        public LLKHF flags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    private enum HOOK : int
    {
        INVALID = 0,
    };

    /* Imports from kernel32.dll */
    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

    /* Imports from user32.dll */
    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int CallNextHookEx(HOOK hhk, HC nCode, WM wParam,
                                             IntPtr lParam);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern HOOK SetWindowsHookEx(WH idHook, CALLBACK lpfn,
                                                IntPtr hMod, int dwThreadId);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int UnhookWindowsHookEx(HOOK hhk);
}

}
