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

namespace wincompose
{

class keyboardhook
{
    public static void init()
    {
        m_callback = key_callback;
        m_hook = SetWindowsHookEx(WH.KEYBOARD_LL, m_callback,
                                  LoadLibrary("user32.dll"), 0);
        if (m_hook == HOOK.INVALID)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public static void fini()
    {
        // FIXME: this will crash if called from the GC Finalizer Thread
        int ret = UnhookWindowsHookEx(m_hook);
        if (ret == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        m_hook = HOOK.INVALID;
        m_callback = null;
    }

    private delegate int callback_t(HC nCode, WM wParam, IntPtr lParam);

    private static byte[] m_keystate = new byte[256];
    private static callback_t m_callback;
    private static HOOK m_hook;

    private static int key_callback(HC nCode, WM wParam, IntPtr lParam)
    {
        if (nCode != HC.ACTION)
            return CallNextHookEx(m_hook, nCode, wParam, lParam);

        // Retrieve event data from native structure
        var data = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam,
                                                     typeof(KBDLLHOOKSTRUCT));

        // Pass to other applications or keep it to ourselves?
        bool eat_key = false;

        if (wParam == WM.KEYDOWN || wParam == WM.SYSKEYDOWN)
        {
            Keys keyData = (Keys)data.vkCode;
        }

        if (wParam == WM.KEYUP || wParam == WM.SYSKEYUP)
        {
            Keys keyData = (Keys)data.vkCode;
        }

        if (wParam == WM.KEYDOWN)
        {
            GetKeyboardState(m_keystate);

            bool has_shift = (GetKeyState(VK.SHIFT) & 0x80) == 0x80;
            bool has_capslock = GetKeyState(VK.CAPITAL) != 0;

            byte[] buf = new byte[2];
            int ret = ToUnicode(data.vkCode, data.scanCode, m_keystate,
                                buf, 2, data.flags);
            if (ret != 0)
                Console.WriteLine("Got Key: {0}", (char)buf[0]);
        }

        return eat_key ? 0 : CallNextHookEx(m_hook, nCode, wParam, lParam);
    }

    private enum HOOK : int
    {
        INVALID = 0,
    };

    /* Enums from winuser.h */
    private enum HC : int
    {
        ACTION      = 0,
        GETNEXT     = 1,
        SKIP        = 2,
        NOREMOVE    = 3,
        NOREM       = 3,
        SYSMODALON  = 4,
        SYSMODALOFF = 5,
    };

    private enum WH : int
    {
        KEYBOARD    = 2,
        KEYBOARD_LL = 13,
    };

    private enum WM : int
    {
        KEYDOWN    = 0x100,
        KEYUP      = 0x101,
        SYSKEYDOWN = 0x104,
        SYSKEYUP   = 0x105,
    };

    private enum VK : int
    {
        SHIFT    = 0x10,
        CONTROL  = 0x11,
        CAPITAL  = 0x14,
        NUMLOCK  = 0x90,
        LSHIFT   = 0xa0,
        RSHIFT   = 0xa1,
        LCONTROL = 0xa2,
        RCONTROL = 0xa3,
        LMENU    = 0xa4,
        RMENU    = 0xa5,
    };

    /* Low-level keyboard input event.
     *   http://msdn.microsoft.com/en-us/library/windows/desktop/ms644967%28v=vs.85%29.aspx
     */
    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public VK vkCode;
        public uint scanCode, flags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    /* Imports from kernel32.dll */
    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

    /* Imports from user32.dll */
    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int CallNextHookEx(HOOK hhk, HC nCode, WM wParam,
                                             IntPtr lParam);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern HOOK SetWindowsHookEx(WH idHook, callback_t lpfn,
                                                IntPtr hMod, int dwThreadId);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int UnhookWindowsHookEx(HOOK hhk);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int ToUnicode(VK wVirtKey, uint wScanCode,
                                        byte[] lpKeyState, byte[] pwszBuff,
                                        int cchBuff, uint wFlags);
    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int GetKeyboardState(byte[] lpKeyState);
    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern short GetKeyState(VK nVirtKey);
}

}
