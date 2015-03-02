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
using System.Runtime.InteropServices;

namespace WinCompose
{

/* Enums from winuser.h */
internal enum HC : int
{
    ACTION      = 0,
    GETNEXT     = 1,
    SKIP        = 2,
    NOREMOVE    = 3,
    NOREM       = 3,
    SYSMODALON  = 4,
    SYSMODALOFF = 5,
};

internal enum WH : int
{
    KEYBOARD    = 2,
    KEYBOARD_LL = 13,
};

internal enum WM : int
{
    INPUTLANGCHANGEREQUEST = 0x50,
    KEYDOWN    = 0x100,
    KEYUP      = 0x101,
    SYSKEYDOWN = 0x104,
    SYSKEYUP   = 0x105,
};

public enum VK : int
{
    NONE     = 0x00,
    BACK     = 0x08,
    SHIFT    = 0x10,
    CONTROL  = 0x11,
    PAUSE    = 0x13,
    CAPITAL  = 0x14,
    ESCAPE   = 0x1b,
    LEFT     = 0x25,
    UP       = 0x26,
    RIGHT    = 0x27,
    DOWN     = 0x28,
    LWIN     = 0x5b,
    RWIN     = 0x5c,
    APPS     = 0x5d,
    NUMLOCK  = 0x90,
    SCROLL   = 0x91,
    LSHIFT   = 0xa0,
    RSHIFT   = 0xa1,
    LCONTROL = 0xa2,
    RCONTROL = 0xa3,
    LMENU    = 0xa4,
    RMENU    = 0xa5,
};

internal enum SC : uint
{
    // Not needed
};

[Flags]
internal enum LLKHF : uint
{
    EXTENDED          = 0x01,
    LOWER_IL_INJECTED = 0x02,
    INJECTED          = 0x10,
    ALTDOWN           = 0x20,
    UP                = 0x80,
};

internal enum EINPUT : uint
{
    MOUSE    = 0,
    KEYBOARD = 1,
    HARDWARE = 2,
}

[Flags]
internal enum KEYEVENTF : uint
{
    EXTENDEDKEY = 0x0001,
    KEYUP       = 0x0002,
    UNICODE     = 0x0004,
    SCANCODE    = 0x0008,
}

[Flags]
internal enum MOUSEEVENTF : uint
{
    // Not needed
}

internal enum VirtualKeyShort : short
{
}

internal enum ScanCodeShort : short
{
}

[StructLayout(LayoutKind.Sequential)]
internal struct INPUT
{
    internal EINPUT type;
    internal UINPUT U;
}

[StructLayout(LayoutKind.Explicit)]
internal struct UINPUT
{
    // All union members need to be included, because they contribute
    // to the final size of struct INPUT.
    [FieldOffset(0)]
    internal MOUSEINPUT mi;
    [FieldOffset(0)]
    internal KEYBDINPUT ki;
    [FieldOffset(0)]
    internal HARDWAREINPUT hi;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MOUSEINPUT
{
    internal int dx, dy, mouseData;
    internal MOUSEEVENTF dwFlags;
    internal uint time;
    internal UIntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct KEYBDINPUT
{
    internal VirtualKeyShort wVk;
    internal ScanCodeShort wScan;
    internal KEYEVENTF dwFlags;
    internal int time;
    internal UIntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct HARDWAREINPUT
{
    internal int uMsg;
    internal short wParamL, wParamH;
}

/* Low-level keyboard input event.
 *   http://msdn.microsoft.com/en-us/library/windows/desktop/ms644967%28v=vs.85%29.aspx
 */
[StructLayout(LayoutKind.Sequential)]
internal struct KBDLLHOOKSTRUCT
{
    internal VK vk;
    public SC sc;
    public LLKHF flags;
    public int time;
    public IntPtr dwExtraInfo;
}

internal enum HOOK : int
{
    INVALID = 0,
};

internal delegate int CALLBACK(HC nCode, WM wParam, IntPtr lParam);

}
