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
using System.Collections.Generic;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WinCompose
{

static class Composer
{
    // Get input from the keyboard hook; return true if the key was handled
    // and needs to be removed from the input chain.
    public static bool OnKey(WM ev, VK vk, SC sc, LLKHF flags)
    {
        CheckKeyboardLayout();

        bool is_keydown = (ev == WM.KEYDOWN || ev == WM.SYSKEYDOWN);
        bool is_keyup = !is_keydown;

        bool has_shift = (NativeMethods.GetKeyState(VK.SHIFT) & 0x80) == 0x80;
        bool has_altgr = (NativeMethods.GetKeyState(VK.LCONTROL) &
                          NativeMethods.GetKeyState(VK.RMENU) & 0x80) == 0x80;
        bool has_lrshift = (NativeMethods.GetKeyState(VK.LSHIFT) &
                            NativeMethods.GetKeyState(VK.RSHIFT) & 0x80) == 0x80;
        bool has_capslock = NativeMethods.GetKeyState(VK.CAPITAL) != 0;

        // If we can not find a printable representation for the key, use its
        // virtual key code instead.
        Key key = new Key(vk);

        // Generate a keystate suitable for ToUnicode()
        NativeMethods.GetKeyboardState(m_keystate);
        m_keystate[(int)VK.SHIFT] = (byte)(has_shift ? 0x80 : 0x00);
        m_keystate[(int)VK.CONTROL] = (byte)(has_altgr ? 0x80 : 0x00);
        m_keystate[(int)VK.MENU] = (byte)(has_altgr ? 0x80 : 0x00);
        m_keystate[(int)VK.CAPITAL] = (byte)(has_capslock ? 0x01 : 0x00);
        int buflen = 4;
        byte[] buf = new byte[2 * buflen];
        int ret = NativeMethods.ToUnicode(vk, sc, m_keystate, buf, buflen, flags);
        if (ret > 0 && ret < buflen)
        {
            // FIXME: if using dead keys, we may receive two keys from GetString!
            buf[ret * 2] = buf[ret * 2 + 1] = 0x00; // Null-terminate the string
            key = new Key(Encoding.Unicode.GetString(buf, 0, ret + 1));
        }

        // FIXME: we don’t properly support compose keys that also normally
        // print stuff, such as `.
        if (Settings.IsComposeKey(key))
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
                    m_sequence = new List<Key>();
            }
            return true;
        }

        // Feature: emulate capslock key with both shift keys, and optionally
        // disable capslock using only one shift key.
        if (key.VirtualKey == VK.LSHIFT || key.VirtualKey == VK.RSHIFT)
        {
            if (is_keyup && has_lrshift && Settings.EmulateCapsLock.Value)
            {
                SendKeyPress(VK.CAPITAL);
                return false;
            }

            if (is_keydown && has_capslock && Settings.ShiftDisablesCapsLock.Value)
            {
                SendKeyPress(VK.CAPITAL);
                return false;
            }
        }

        // If we are not currently composing a sequence, do nothing
        if (!m_composing)
        {
            return false;
        }

        if (!Settings.IsUsableKey(key))
        {
            // FIXME: if compose key is down, maybe there is a key combination
            // going on, such as Alt+Tab or Windows+Up
            return false;
        }

        // Finally, this is a key we must add to our compose sequence and
        // decide whether we'll eventually output a character.
        if (is_keydown)
        {
            m_sequence.Add(key);

            // FIXME: we don’t support case-insensitive yet
            if (Settings.IsValidSequence(m_sequence))
            {
                // Sequence finished, print it
                SendString(Settings.GetSequenceResult(m_sequence));
                m_composing = false;
                m_sequence = new List<Key>();
            }
            else if (Settings.IsValidPrefix(m_sequence))
            {
                // Still a valid prefix, continue building sequence
            }
            else
            {
                // Unknown characters for sequence, print them if necessary
                if (!Settings.DiscardOnInvalid.Value)
                {
                    foreach (Key k in m_sequence)
                    {
                        // FIXME: what if the key is e.g. left arrow?
                        if (k.IsPrintable())
                            SendString(k.ToString());
                    }
                }

                if (Settings.BeepOnInvalid.Value)
                    SystemSounds.Beep.Play();

                m_composing = false;
                m_sequence = new List<Key>();
            }
        }

        return true;
    }

    private static void SendString(string str)
    {
        List<VK> modifiers = new List<VK>();
        bool use_gtk_hack = false, use_office_hack = false;

        const int len = 256;
        StringBuilder buf = new StringBuilder(len);
        var hwnd = NativeMethods.GetForegroundWindow();
        if (NativeMethods.GetClassName(hwnd, buf, len) > 0)
        {
            string wclass = buf.ToString();

            /* HACK: GTK+ applications behave differently with Unicode, and some
             * applications such as XChat for Windows rename their own top-level
             * window, so we parse through the names we know in order to detect
             * a GTK+ application. */
            if (wclass == "gdkWindowToplevel" || wclass == "xchatWindowToplevel")
                use_gtk_hack = true;

            /* HACK: in MS Office, some symbol insertions change the text font
             * without returning to the original font. To avoid this, we output
             * a space character, then go left, insert our actual symbol, then
             * go right and backspace. */
            /* These are the actual window class names for Outlook and Word…
             * TODO: PowerPoint ("PP(7|97|9|10)FrameClass") */
            if (wclass == "rctrl_renwnd32" || wclass == "OpusApp")
                use_office_hack = true && Settings.InsertZwsp.Value;
        }

        /* Clear keyboard modifiers if we need one of our custom hacks */
        if (use_gtk_hack || use_office_hack)
        {
            VK[] all_modifiers = new VK[]
            {
                VK.LSHIFT, VK.RSHIFT,
                VK.LCONTROL, VK.RCONTROL,
                VK.LMENU, VK.RMENU,
            };

            foreach (VK vk in all_modifiers)
                if ((NativeMethods.GetKeyState(vk) & 0x80) == 0x80)
                    modifiers.Add(vk);

            foreach (VK vk in modifiers)
                SendKeyUp(vk);
        }

        if (use_gtk_hack)
        {
            /* Wikipedia says Ctrl+Shift+u, release, then type the four hex
             * digits, and press Enter.
             * (http://en.wikipedia.org/wiki/Unicode_input). */
            SendKeyDown(VK.LCONTROL);
            SendKeyDown(VK.LSHIFT);
            SendKeyPress((VK)'U');
            SendKeyUp(VK.LSHIFT);
            SendKeyUp(VK.LCONTROL);

            foreach (var ch in str)
                foreach (var key in String.Format("{0:X04} ", (short)ch))
                    SendKeyPress((VK)key);
        }
        else
        {
            List<INPUT> input = new List<INPUT>();

            if (use_office_hack)
            {
                input.Add(NewInputKey((ScanCodeShort)'\u200b'));
                input.Add(NewInputKey((VirtualKeyShort)VK.LEFT));
            }

            for (int i = 0; i < str.Length; i++)
            {
                input.Add(NewInputKey((ScanCodeShort)str[i]));
            }

            if (use_office_hack)
            {
                input.Add(NewInputKey((VirtualKeyShort)VK.RIGHT));
            }

            NativeMethods.SendInput((uint)input.Count, input.ToArray(),
                                    Marshal.SizeOf(typeof(INPUT)));
        }

        /* Restore keyboard modifiers if we needed one of our custom hacks */
        if (use_gtk_hack || use_office_hack)
        {
            foreach (VK vk in modifiers)
                SendKeyDown(vk);
        }
    }

    public static bool IsComposing()
    {
        return m_composing;
    }

    private static INPUT NewInputKey(VirtualKeyShort vk)
    {
        INPUT ret = NewInputKey();
        ret.U.ki.wVk = vk;
        return ret;
    }

    private static INPUT NewInputKey(ScanCodeShort sc)
    {
        INPUT ret = NewInputKey();
        ret.U.ki.wScan = sc;
        return ret;
    }

    private static INPUT NewInputKey()
    {
        INPUT ret = new INPUT();
        ret.type = EINPUT.KEYBOARD;
        ret.U.ki.wVk = (VirtualKeyShort)0;
        ret.U.ki.wScan = (ScanCodeShort)0;
        ret.U.ki.time = 0;
        ret.U.ki.dwFlags = KEYEVENTF.UNICODE;
        ret.U.ki.dwExtraInfo = UIntPtr.Zero;
        return ret;
    }

    private static void SendKeyDown(VK vk)
    {
        NativeMethods.keybd_event(vk, 0, 0, 0);
    }

    private static void SendKeyUp(VK vk)
    {
        NativeMethods.keybd_event(vk, 0, KEYEVENTF.KEYUP, 0);
    }

    private static void SendKeyPress(VK vk)
    {
        SendKeyDown(vk);
        SendKeyUp(vk);
    }

    // FIXME: this is useless for now
    private static void CheckKeyboardLayout()
    {
        // FIXME: the foreground window doesn't seem to notice keyboard
        // layout changes caused by the Win+Space combination.
        IntPtr hwnd = NativeMethods.GetForegroundWindow();
        uint pid, tid = NativeMethods.GetWindowThreadProcessId(hwnd, out pid);
        IntPtr active_layout = NativeMethods.GetKeyboardLayout(tid);
        //Console.WriteLine("Active layout is {0:X}", (int)active_layout);

        tid = NativeMethods.GetCurrentThreadId();
        IntPtr my_layout = NativeMethods.GetKeyboardLayout(tid);
        //Console.WriteLine("WinCompose layout is {0:X}", (int)my_layout);
    }

    private static byte[] m_keystate = new byte[256];
    private static List<Key> m_sequence = new List<Key>();
    private static bool m_compose_down = false;
    private static bool m_composing = false;
}

}
