//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

#define _CRT_SECURE_NO_WARNINGS
#include <windows.h>

//
// This utility DLL provides native functions for the installer that
// bypass some InnoSetup limitations:
//
//  - keepalive() spawns a background thread and sends a KEYUP message
//    to a given window at a regular interval. InnoSetup does not support
//    background tasks or timers.
//
//  - fix_file() modifies a scheduled task file to bypass the default
//    values stored by schtasks.exe. InnoSetup could do this but its
//    LoadStringsFromFile function has issues with UTF-16le (it reads
//    the file as ANSI then converts it to Unicode).
//

static HWND g_hwnd = 0;
static HANDLE g_thread = 0;
static unsigned int g_milliseconds = 0;

static DWORD WINAPI thread_func(void* data)
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
void __cdecl keepalive(HWND hwnd, unsigned int milliseconds)
{
    g_hwnd = hwnd;
    g_milliseconds = milliseconds;

    if (hwnd == 0)
        g_thread = 0;
    else if (g_thread == 0)
        g_thread = CreateThread(nullptr, 0, thread_func, nullptr, 0, nullptr);
}


static void get_local_user_group_name(wchar_t *buf)
{
    // Build SID S-1-5-32-545 (“Users” group)
    SID sid;
    sid.Revision = 1;
    sid.SubAuthorityCount = 2;
    sid.IdentifierAuthority = SECURITY_NT_AUTHORITY; // 5
    sid.SubAuthority[0] = SECURITY_BUILTIN_DOMAIN_RID; // 32
    sid.SubAuthority[1] = DOMAIN_ALIAS_RID_USERS; // 545

    // Start buffer with “BUILTIN\”
    buf[0] = L'\0';
    wcscat(buf, L"BUILTIN\\");

    // Append user group name to buffer
    BOOL (WINAPI *lookup_sid)(LPCWSTR, PSID, LPWSTR, LPDWORD, LPWSTR, LPDWORD, PSID_NAME_USE)
        = (decltype(lookup_sid))GetProcAddress(GetModuleHandleW(L"advapi32.dll"),
                                               "LookupAccountSidW");
    if (lookup_sid)
    {
        wchar_t dummy[128];
        DWORD name_len = 128, dummy_len = 128;
        SID_NAME_USE type;
        lookup_sid(nullptr, &sid, buf + wcslen(buf), &name_len, dummy, &dummy_len, &type);
    }
    else
    {
        wcscat(buf, L"Users");
    }
}

static void wstring_replace(wchar_t *buf, wchar_t const *src, wchar_t const *dst)
{
    for (wchar_t *p = wcsstr(buf, src); p; p = wcsstr(p, src))
    {
        memmove(p + wcslen(dst), p + wcslen(src), (wcslen(p + wcslen(src)) + 1) * sizeof(wchar_t));
        memcpy(p, dst, wcslen(dst) * sizeof(wchar_t));
    }
}

static void fix_tag(wchar_t *buf, wchar_t const *tag, wchar_t const *value)
{
    wchar_t opening[128] = { 0 }, closing[128] = { 0 };
    wcscat(opening, L"<"); wcscat(opening, tag); wcscat(opening, L">");
    wcscat(closing, L"</"); wcscat(closing, tag); wcscat(closing, L">");

    wchar_t *t1 = wcsstr(buf, opening);
    wchar_t *t2 = t1 ? wcsstr(t1, closing) : nullptr;
    if (t1 && t2)
    {
        memmove(t1 + wcslen(opening) + wcslen(value), t2, (wcslen(t2) + 1) * sizeof(wchar_t));
        memcpy(t1 + wcslen(opening), value, wcslen(value) * sizeof(wchar_t));
    }
}

extern "C" __declspec(dllexport)
void __cdecl fix_file(wchar_t const *path)
{
    // Disable the System32 → System redirection for 32-bit processes
    PVOID old_value = nullptr;
    BOOL (WINAPI *disable_redir)(PVOID *)
        = (decltype(disable_redir))GetProcAddress(GetModuleHandleW(L"kernel32.dll"),
                                                  "Wow64DisableWow64FsRedirection");
    if (disable_redir)
        disable_redir(&old_value);

    // Read file into buffer
    static wchar_t buf[16384];
    auto fd = CreateFileW(path, GENERIC_READ, 0, nullptr, OPEN_EXISTING,
                          FILE_ATTRIBUTE_NORMAL, nullptr);
    if (fd == INVALID_HANDLE_VALUE)
        return;
    DWORD bytes;
    ReadFile(fd, buf, sizeof(buf), &bytes, nullptr);
    CloseHandle(fd);

    // Fix file author
    fix_tag(buf, L"Author", L"Sam Hocevar");

    // Fix runtime credentials:
    //  - use the GroupId for local user group (can’t use builtin user SYSTEM
    //    because that user cannot open GUI programs).
    //  - make sure we specify a GroupId, not a UserId
    //  - remove <LogonType> tag because it’s only relevant for UserIds.
    wchar_t group[128];
    get_local_user_group_name(group);
    wstring_replace(buf, L"UserId", L"GroupId");
    fix_tag(buf, L"GroupId", group); // Can’t use SYSTEM because it can’t open GUIs
    fix_tag(buf, L"LogonType", L"");
    wstring_replace(buf, L"<LogonType></LogonType>", L"");

    // Fix some additional task settings
    fix_tag(buf, L"RunLevel", L"HighestAvailable");
    fix_tag(buf, L"MultipleInstancesPolicy", L"Parallel");
    fix_tag(buf, L"DisallowStartIfOnBatteries", L"false");
    fix_tag(buf, L"StopIfGoingOnBatteries", L"false");
    fix_tag(buf, L"StopOnIdleEnd", L"false");

    fd = CreateFileW(path, GENERIC_WRITE, 0, nullptr, CREATE_ALWAYS,
                     FILE_ATTRIBUTE_NORMAL, nullptr);
    if (fd == INVALID_HANDLE_VALUE)
        return;
    WriteFile(fd, buf, wcslen(buf) * sizeof(wchar_t), &bytes, nullptr);
    CloseHandle(fd);

    // Restore the System32 → System redirection
    BOOL (WINAPI *revert_redir)(PVOID)
        = (decltype(revert_redir))GetProcAddress(GetModuleHandleW(L"kernel32.dll"),
                                                 "Wow64RevertWow64FsRedirection");
    if (revert_redir)
        revert_redir(old_value);
}

