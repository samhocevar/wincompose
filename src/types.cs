// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//   This program is free software; you can redistribute it and/or
//   modify it under the terms of the Do What The Fuck You Want To
//   Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.using System;

namespace wincompose
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
    KEYDOWN    = 0x100,
    KEYUP      = 0x101,
    SYSKEYDOWN = 0x104,
    SYSKEYUP   = 0x105,
};

public enum VK : int
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

public enum SC : uint
{
};

}
