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

HWND g_hwnd = 0;
HANDLE g_thread = 0;

DWORD WINAPI thread_func(void* data)
{
    while (g_hwnd != 0)
    {
        PostMessage(g_hwnd, WM_LBUTTONDOWN, 0, 0);
        PostMessage(g_hwnd, WM_LBUTTONUP, 0, 0);
        Sleep(2000);
    }

    return 0;
}

extern "C" __declspec(dllexport) void __cdecl trampoline(HWND hwnd)
{
    g_hwnd = hwnd;

    if (hwnd == 0)
        g_thread = 0;
    else if (g_thread == 0)
        g_thread = CreateThread(nullptr, 0, thread_func, nullptr, 0, nullptr);
}

