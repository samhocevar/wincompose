// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//   This program is free software; you can redistribute it and/or
//   modify it under the terms of the Do What The Fuck You Want To
//   Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.using System;

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace wincompose
{

static class compose
{
    // Get input from the keyboard hook; return true if the key was handled
    // and needs to be removed from the input chain.
    public static bool on_key(WM ev, VK vk, SC sc, uint flags)
    {
        bool is_keydown = (ev == WM.KEYDOWN || ev == WM.SYSKEYDOWN);
        bool is_keyup = !is_keydown;
        bool is_compose = (vk == VK.RMENU);

        if (is_compose)
        {
            if (is_keyup)
            {
                m_compose_down = false;
            }
            else if (is_keydown && !m_compose_down)
            {
                m_compose_down = true;
                m_composing = !m_composing;
                if (!m_composing)
                    m_sequence = "";
            }
            return true;
        }

        if (!m_composing)
            return false;

        string str = null;

        // Generate a keystate suitable for ToUnicode()
        // FIXME: this code is missing AltGr for now
        GetKeyboardState(m_keystate);
        bool has_shift = (GetKeyState(VK.SHIFT) & 0x80) == 0x80;
        bool has_capslock = GetKeyState(VK.CAPITAL) != 0;
        m_keystate[0x10] = has_shift ? (byte)0x80 : (byte)0x00;
        m_keystate[0x14] = has_capslock ? (byte)0x01 : (byte)0x00;

        int buflen = 4;
        byte[] buf = new byte[2 * buflen];
        int ret = ToUnicode(vk, sc, m_keystate, buf, buflen, flags);
        if (ret > 0 && ret < buflen)
        {
            buf[ret * 2] = buf[ret * 2 + 1] = 0x00; // Null-terminate the string
            str = Encoding.Unicode.GetString(buf, 0, ret + 1);
        }

        // Don't know what to do with these for now, but we need to handle some
        // of them, for instance the arrow keys
        if (str == null)
            return false;

        // Finally, this is a key we must add to our compose sequence and
        // decide whether we'll eventually output a character.
        if (is_keydown)
        {
            m_sequence += str;

            if (m_sequence == ":")
            {
                // Do nothing, sequence in progress
            }
            else if (m_sequence == ":)")
            {
                // Sequence finished, print it
                send_str("☺");
                m_composing = false;
                m_sequence = "";
            }
            else
            {
                // Unknown characters for sequence, print them
                send_str(m_sequence);
                m_composing = false;
                m_sequence = "";
            }
        }

        return true;
    }

    private static void send_str(string str)
    {
        INPUT[] input = new INPUT[str.Length];

        for (int i = 0; i < input.Length; i++)
        {
            input[i] = new INPUT();
            input[i].type = EINPUT.KEYBOARD;
            input[i].U.ki.wVk = 0;
            input[i].U.ki.wScan = (ScanCodeShort)str[i];
            input[i].U.ki.time = 0;
            input[i].U.ki.dwFlags = KEYEVENTF.UNICODE;
            input[i].U.ki.dwExtraInfo = UIntPtr.Zero;
        }

        SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(INPUT)));
    }

    private static byte[] m_keystate = new byte[256];
    private static string m_sequence = "";
    private static bool m_compose_down = false;
    private static bool m_composing = false;

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
    public struct INPUT
    {
        internal EINPUT type;
        internal UINPUT U;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct UINPUT
    {
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

    [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint SendInput(uint nInputs,
        [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int ToUnicode(VK wVirtKey, SC wScanCode,
                                        byte[] lpKeyState, byte[] pwszBuff,
                                        int cchBuff, uint wFlags);
    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int GetKeyboardState(byte[] lpKeyState);
    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern short GetKeyState(VK nVirtKey);
}

}
