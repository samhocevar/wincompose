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
using System.Runtime.InteropServices;
using WinForms = System.Windows.Forms;

namespace WinCompose
{

static class KeyboardHook
{
    public static void Init()
    {
        m_active = Environment.OSVersion.Platform == PlatformID.Win32NT
                || Environment.OSVersion.Platform == PlatformID.Win32S
                || Environment.OSVersion.Platform == PlatformID.Win32Windows
                || Environment.OSVersion.Platform == PlatformID.WinCE;

        // Create a timer to regularly check our hook
        m_update_timer = new WinForms.Timer();
        m_update_timer.Tick += new EventHandler(CheckHook);
        m_update_timer.Interval = 2000;
        m_update_timer.Start();
    }

    public static void Fini()
    {
        m_update_timer.Stop();

        m_active = false;
        CheckHook(null, null);
    }

    private static void CheckHook(object obj, EventArgs args)
    {
        HOOK old_hook = m_hook;

        // Reinstall the hook in case Windows disabled it without telling us. It’s
        // OK to have two hooks installed for a short time, because we check for
        // recursive calls to ourselves in OnKey().
        if (m_active)
        {
            m_hook = NativeMethods.SetWindowsHookEx(WH.KEYBOARD_LL, m_callback,
                                   NativeMethods.LoadLibrary("user32.dll"), 0);
            if (m_hook == HOOK.INVALID)
            {
                Log.Debug("Unable to install hook: {0}",
                          new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }
        else
        {
            m_hook = HOOK.INVALID;
        }

        if (old_hook != HOOK.INVALID)
        {
            // XXX: this will crash if the hook is not removed from the same
            // thread that installed it.
            int ret = NativeMethods.UnhookWindowsHookEx(old_hook);

            if (ret == 0)
            {
                Log.Debug("Unable to uninstall hook: {0}",
                          new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }
    }

    private static WinForms.Timer m_update_timer;
    private static bool m_active = false;

    // Keep an explicit reference on the CALLBACK object created because
    // SetWindowsHookEx will not prevent it from being GCed.
    private static CALLBACK m_callback = new CALLBACK(OnKey);

    private static HOOK m_hook = HOOK.INVALID;

    // Check whether OnKey is called twice for the same event
    private static int m_recursive = 0;

    private static int OnKey(HC nCode, WM wParam, IntPtr lParam)
    {
        bool is_key = (wParam == WM.KEYDOWN || wParam == WM.SYSKEYDOWN
                        || wParam == WM.KEYUP || wParam == WM.SYSKEYUP);

        if (m_recursive != 0)
        {
            // Do nothing. We can only get here if a key is pressed during
            // the very short time where we have two hooks installed, i.e.
            // practically never, but it’s better to handle this properly.
        }
        else if (nCode == HC.ACTION && is_key)
        {
            // Retrieve key event data from native structure
            var data = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam,
                                                      typeof(KBDLLHOOKSTRUCT));
            bool is_injected = (data.flags & LLKHF.INJECTED) != 0;
            bool ignore = is_injected && !Settings.AllowInjected.Value;

            Log.Debug("{0}{1}: OnKey(HC.{2}, WM.{3}, [vk:0x{4:X02} ({7}) sc:0x{5:X02} flags:{6}])",
                      ignore ? "Ignored " : "", is_injected ? "Injected Event" : "Event",
                      nCode, wParam, (int)data.vk, (int)data.sc, data.flags, new Key(data.vk));

            if (!ignore)
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

        // Call next hook but guard against re-doing our own work in case we
        // were installed twice.
        ++m_recursive;
        int ret = NativeMethods.CallNextHookEx(m_hook, nCode, wParam, lParam);
        --m_recursive;

        return ret;
    }
}

}
