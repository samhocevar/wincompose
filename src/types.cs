// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//   This program is free software; you can redistribute it and/or
//   modify it under the terms of the Do What The Fuck You Want To
//   Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.using System;

using System;

namespace WinCompose
{

/* Enums from winuser.h */
public enum HC : int
{
    ACTION      = 0,
    GETNEXT     = 1,
    SKIP        = 2,
    NOREMOVE    = 3,
    NOREM       = 3,
    SYSMODALON  = 4,
    SYSMODALOFF = 5,
};

public enum WH : int
{
    KEYBOARD    = 2,
    KEYBOARD_LL = 13,
};

public enum WM : int
{
    INPUTLANGCHANGEREQUEST = 0x50,
    KEYDOWN    = 0x100,
    KEYUP      = 0x101,
    SYSKEYDOWN = 0x104,
    SYSKEYUP   = 0x105,
};

public enum VK : int
{
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

public enum SC : uint
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

}
