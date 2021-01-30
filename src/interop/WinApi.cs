//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2021 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WinCompose
{

static internal class NativeMethods
{
    public static bool EXISTS(string dll_name, string proc_name)
    {
        var dll = NativeMethods.LoadLibrary(dll_name);
        var proc = NativeMethods.GetProcAddress(dll, proc_name);
        return proc != UIntPtr.Zero;
    }

    //
    // for Composer.cs
    //

    [DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern uint SendInput(uint nInputs,
        [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
    [DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern void keybd_event(VK vk, SC sc, KEYEVENTF flags,
                                          int dwExtraInfo);

    [DllImport("user32", CharSet=CharSet.Auto)]
    public static extern int ToUnicode(VK wVirtKey, SC wScanCode,
                                       byte[] lpKeyState, byte[] pwszBuff,
                                       int cchBuff, LLKHF flags);
    // Use IntPtr instead of HKL because we can’t have an IntPtr-based enum
    [DllImport("user32", CharSet=CharSet.Auto)]
    public static extern int ToUnicodeEx(VK wVirtKey, SC wScanCode,
                                         byte[] lpKeyState, byte[] pwszBuff,
                                         int cchBuff, LLKHF flags,
                                         IntPtr dwhkl);
    [DllImport("user32", CharSet=CharSet.Auto)]
    public static extern int GetKeyboardState(byte[] lpKeyState);
    [DllImport("user32", CharSet=CharSet.Auto)]
    public static extern void SetKeyboardState(byte[] lpKeyState);
    [DllImport("user32", CharSet=CharSet.Auto)]
    public static extern short GetKeyState(VK nVirtKey);

    [DllImport("kernel32")]
    public static extern uint GetCurrentThreadId();
    [DllImport("user32", SetLastError=true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32")]
    public static extern IntPtr GetKeyboardLayout(uint idThread);
    // Use IntPtr instead of HKL because we can’t have an IntPtr-based enum
    [DllImport("user32", SetLastError=true)]
    public static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, uint Flags);
    [DllImport("imm32", CharSet=CharSet.Auto)]
    public static extern IntPtr ImmGetDefaultIMEWnd(HandleRef hwnd);

    [DllImport("user32")]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    [DllImport("user32")]
    public static extern uint RegisterWindowMessage(string message);

    [DllImport("kernel32", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern bool DefineDosDevice(DDD dwFlags, string lpDeviceName, string lpTargetPath);
    [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
    public static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess,
            FileShare dwShareMode, IntPtr SecurityAttributes, FileMode dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("Kernel32.dll", SetLastError=true)]
    public static extern bool DeviceIoControl(SafeFileHandle hDevice, IOCTL IoControlCode,
            ref KEYBOARD_INDICATOR_PARAMETERS InBuffer, int nInBufferSize, IntPtr OutBuffer,
            int nOutBufferSize, out int pBytesReturned, IntPtr Overlapped);
    [DllImport("Kernel32.dll", SetLastError=true)]
    public static extern bool DeviceIoControl(SafeFileHandle hDevice, IOCTL IoControlCode,
            IntPtr InBuffer, int nInBufferSize, out KEYBOARD_INDICATOR_PARAMETERS OutBuffer,
            int nOutBufferSize, out int pBytesReturned, IntPtr Overlapped);

    //
    // for KeyboardHook.cs
    //

    /* Imports from kernel32.dll */
    [DllImport("kernel32", CharSet=CharSet.Ansi, SetLastError=true)]
    public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
    [DllImport("kernel32", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
    public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

    /* Imports from user32.dll */
    [DllImport("user32", CharSet=CharSet.Auto)]
    public static extern int CallNextHookEx(HOOK hhk, HC nCode, WM wParam,
                                            IntPtr lParam);
    [DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern HOOK SetWindowsHookEx(WH idHook, CALLBACK lpfn,
                                               IntPtr hMod, int dwThreadId);
    [DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern int UnhookWindowsHookEx(HOOK hhk);

    //
    // for KeyboardLayout.cs
    //

    [DllImport("user32", SetLastError=true, CharSet=CharSet.Auto)]
    public static extern IntPtr GetForegroundWindow();
    [DllImport("user32", SetLastError=true, CharSet=CharSet.Auto)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder text, int count);
    [DllImport("user32", SetLastError=true, CharSet=CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    public static uint MAKELANG(LANG p, SUBLANG s) => ((uint)s << 10) | (uint)p;

    //
    // for RemoteControl.cs
    //

    [DllImport("user32", SetLastError=true)]
    public static extern bool ChangeWindowMessageFilter(uint msg, MSGFLT flags);

    //
    // for KeySelector.xaml.cs
    //

    [DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern IntPtr MB_GetString(DialogBoxCommandID strId);

    //
    // for Settings.cs and SettingsEntry.cs
    //

    [DllImport("kernel32")]
    public static extern long WritePrivateProfileString(string Section,
                                    string Key, string Value, string FilePath);
    [DllImport("kernel32")]
    public static extern int GetPrivateProfileString(string Section, string Key,
              string Default, StringBuilder RetVal, int Size, string FilePath);

    //
    // for Settings.cs
    //

    [DllImport("shlwapi", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern HRESULT AssocQueryString(ASSOCF flags, ASSOCSTR str,
              string pszAssoc, string pszExtra, [Out] StringBuilder pszOut,
              ref uint pcchOut);

    //
    // for NotificationIcon.xaml.cs
    //

    [DllImport("user32", CharSet=CharSet.Auto, SetLastError=true)]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle,
        IntPtr childAfter, string className, string windowTitle);
    [DllImport("user32")]
    public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    //
    // for SchTask.cs
    //

    [DllImport("advapi32", CharSet=CharSet.Auto, SetLastError = true)]
    public static extern bool LookupAccountSid(string lpSystemName,
        [MarshalAs(UnmanagedType.LPArray)] byte[] Sid, StringBuilder lpName,
        ref uint cchName, StringBuilder ReferencedDomainName,
        ref uint cchReferencedDomainName, out SID_NAME_USE peUse);
};

}

