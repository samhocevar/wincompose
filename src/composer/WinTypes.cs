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
    NONE       = 0x00,
    LBUTTON    = 0x01,
    RBUTTON    = 0x02,
    CANCEL     = 0x03,
    MBUTTON    = 0x04,
    XBUTTON1   = 0x05,
    XBUTTON2   = 0x06,
    BACK       = 0x08,
    TAB        = 0x09,
    CLEAR      = 0x0c,
    RETURN     = 0x0d,
    SHIFT      = 0x10,
    CONTROL    = 0x11,
    MENU       = 0x12,
    PAUSE      = 0x13,
    CAPITAL    = 0x14,
    KANA       = 0x15,
    HANGUEL    = 0x15,
    HANGUL     = 0x15,
    JUNJA      = 0x17,
    FINAL      = 0x18,
    HANJA      = 0x19,
    KANJI      = 0x19,
    ESCAPE     = 0x1b,
    CONVERT    = 0x1c,
    NONCONVERT = 0x1d,
    ACCEPT     = 0x1e,
    MODECHANGE = 0x1f,
    SPACE      = 0x20,
    PRIOR      = 0x21,
    NEXT       = 0x22,
    END        = 0x23,
    HOME       = 0x24,
    LEFT       = 0x25,
    UP         = 0x26,
    RIGHT      = 0x27,
    DOWN       = 0x28,
    SELECT     = 0x29,
    PRINT      = 0x2a,
    EXECUTE    = 0x2b,
    SNAPSHOT   = 0x2c,
    INSERT     = 0x2d,
    DELETE     = 0x2e,
    HELP       = 0x2f,
    /* 0x30 … 0x39: 0—9 */
    /* 0x41 … 0x5a: a—z */
    LWIN       = 0x5b,
    RWIN       = 0x5c,
    APPS       = 0x5d,
    SLEEP      = 0x5f,
    /* 0x60 … 0x69: numpad 0—9 */
    MULTIPLY   = 0x6a,
    ADD        = 0x6b,
    SEPARATOR  = 0x6c,
    SUBTRACT   = 0x6d,
    DECIMAL    = 0x6e,
    DIVIDE     = 0x6f,
    /* 0x70 … 0x87: F1—F24 */
    NUMLOCK    = 0x90,
    SCROLL     = 0x91,
    /* 0x92 … 0x96: OEM specific */
    LSHIFT     = 0xa0,
    RSHIFT     = 0xa1,
    LCONTROL   = 0xa2,
    RCONTROL   = 0xa3,
    LMENU      = 0xa4,
    RMENU      = 0xa5,
    /* 0xa6 … 0xb7: browser, volume, media, launch */
    OEM_1      = 0xba,
    OEM_PLUS   = 0xbb,
    OEM_COMMA  = 0xbc,
    OEM_MINUS  = 0xbd,
    OEM_PERIOD = 0xbe,
    OEM_2      = 0xbf,
    OEM_3      = 0xc0,
    OEM_4      = 0xdb,
    OEM_5      = 0xdc,
    OEM_6      = 0xdd,
    OEM_7      = 0xde,
    OEM_8      = 0xdf,
    /* 0xe1: OEM specific */
    OEM_102    = 0xe2,
    /* 0xe3, 0xe4: OEM specific */
    PROCESSKEY = 0xe5,
    /* 0xe6: OEM specific */
    PACKET     = 0xe7,
    /* 0xe9 … 0xf5: OEM specific */
    ATTN       = 0xf6,
    CRSEL      = 0xf7,
    EXSEL      = 0xf8,
    EREOF      = 0xf9,
    PLAY       = 0xfa,
    ZOOM       = 0xfb,
    NONAME     = 0xfc,
    PA1        = 0xfd,
    OEM_CLEAR  = 0xfe,
};

internal enum SC : uint
{
    // Not needed
};

[Flags]
internal enum DDD : uint
{
    RAW_TARGET_PATH       = 0x00000001,
    REMOVE_DEFINITION     = 0x00000002,
    EXACT_MATCH_ON_REMOVE = 0x00000004,
    NO_BROADCAST_SYSTEM   = 0x00000008,
};

internal enum IOCTL : uint
{
    // ControlCode(FILE_DEVICE_KEYBOARD, 0x0002, METHOD_BUFFERED, FILE_ANY_ACCESS
    KEYBOARD_SET_INDICATORS   = 0x000b0008,
    // ControlCode(FILE_DEVICE_KEYBOARD, 0x0010, METHOD_BUFFERED, FILE_ANY_ACCESS
    KEYBOARD_QUERY_INDICATORS = 0x000b0040,
};

[Flags]
internal enum KEYBOARD : ushort
{
    SCROLL_LOCK_ON = 0x0001,
    NUM_LOCK_ON    = 0x0002,
    CAPS_LOCK_ON   = 0x0004,
    KANA_LOCK_ON   = 0x0008,
    SHADOW         = 0x4000,
    LED_INJECTED   = 0x8000,
};

internal struct KEYBOARD_INDICATOR_PARAMETERS
{
    public ushort UnitId;
    public KEYBOARD LedFlags;
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
