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
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WinCompose
{

static internal class NativeMethods
{
    //
    // for Composer.cs
    //

    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint SendInput(uint nInputs,
        [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern void keybd_event(VK vk, SC sc, KEYEVENTF flags,
                                          int dwExtraInfo);

    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern int ToUnicode(VK wVirtKey, SC wScanCode,
                                       byte[] lpKeyState, byte[] pwszBuff,
                                       int cchBuff, LLKHF flags);
    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern int GetKeyboardState(byte[] lpKeyState);
    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern void SetKeyboardState(byte[] lpKeyState);
    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern short GetKeyState(VK nVirtKey);

    [DllImport("user32", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr GetForegroundWindow();
    [DllImport("user32", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder text, int count);
    [DllImport("user32", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("kernel32")]
    public static extern uint GetCurrentThreadId();
    [DllImport("user32", SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32")]
    public static extern IntPtr GetKeyboardLayout(uint idThread);
    [DllImport("imm32", CharSet = CharSet.Auto)]
    public static extern IntPtr ImmGetDefaultIMEWnd(HandleRef hwnd);

    [DllImport("user32")]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    [DllImport("user32")]
    public static extern uint RegisterWindowMessage(string message);

    [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool DefineDosDevice(DDD dwFlags, string lpDeviceName, string lpTargetPath);
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess,
            FileShare dwShareMode, IntPtr SecurityAttributes, FileMode dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern bool DeviceIoControl(SafeFileHandle hDevice, IOCTL IoControlCode,
            ref KEYBOARD_INDICATOR_PARAMETERS InBuffer, int nInBufferSize, IntPtr OutBuffer,
            int nOutBufferSize, out int pBytesReturned, IntPtr Overlapped);
    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern bool DeviceIoControl(SafeFileHandle hDevice, IOCTL IoControlCode,
            IntPtr InBuffer, int nInBufferSize, out KEYBOARD_INDICATOR_PARAMETERS OutBuffer,
            int nOutBufferSize, out int pBytesReturned, IntPtr Overlapped);

    //
    // for KeyboardHook.cs
    //

    /* Imports from kernel32.dll */
    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

    /* Imports from user32.dll */
    [DllImport("user32", CharSet = CharSet.Auto)]
    public static extern int CallNextHookEx(HOOK hhk, HC nCode, WM wParam,
                                             IntPtr lParam);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern HOOK SetWindowsHookEx(WH idHook, CALLBACK lpfn,
                                                IntPtr hMod, int dwThreadId);
    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int UnhookWindowsHookEx(HOOK hhk);

    //
    // for Settings.cs and SettingsEntry.cs
    //

    [DllImport("kernel32")]
    public static extern long WritePrivateProfileString(string Section,
                                    string Key, string Value, string FilePath);
    [DllImport("kernel32")]
    public static extern int GetPrivateProfileString(string Section, string Key,
              string Default, StringBuilder RetVal, int Size, string FilePath);
};

}

