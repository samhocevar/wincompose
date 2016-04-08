//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2016 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

#include <windows.h>

//
// This utility DLL simply spawns a background thread and sends a KEYUP
// message to a given window at a regular interval. We use it in our
// installer because InnoSetup does not support background tasks or timers.
//

HWND g_hwnd = 0;
HANDLE g_thread = 0;
unsigned int g_milliseconds = 0;

DWORD WINAPI thread_func(void* data)
{
    while (g_hwnd != 0)
    {
        // Use 0x88 because it’s marked as “unassigned” in the MS docs.
        PostMessage(g_hwnd, WM_KEYUP, 0x88, 0);
        Sleep(g_milliseconds);
    }

    return 0;
}

extern "C" __declspec(dllexport)
void __cdecl trampoline(HWND hwnd, unsigned int milliseconds)
{
    g_hwnd = hwnd;
    g_milliseconds = milliseconds;

    if (hwnd == 0)
        g_thread = 0;
    else if (g_thread == 0)
        g_thread = CreateThread(nullptr, 0, thread_func, nullptr, 0, nullptr);
}

