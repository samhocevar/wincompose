//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2016 Sam Hocevar <sam@hocevar.net>
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
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace WinCompose
{

/// <summary>
/// A convenience class that can be fed either scancodes (<see cref="SC"/>)
/// or virtual keys (<see cref="VK"/>), then uses the Win32 API function
/// <see cref="SendInput"/> to send all these events in one single call.
/// </summary>
class InputSequence
{
    public void Send()
    {
        NativeMethods.SendInput((uint)m_input.Count, m_input.ToArray(),
                                Marshal.SizeOf(typeof(INPUT)));
    }

    public void AddInput(ScanCodeShort sc)
    {
        AddInput((VirtualKeyShort)0, sc);
    }

    public void AddInput(VirtualKeyShort vk)
    {
        AddInput(vk, (ScanCodeShort)0);
    }

    private List<INPUT> m_input = new List<INPUT>();

    private void AddInput(VirtualKeyShort vk, ScanCodeShort sc)
    {
        INPUT tmp = new INPUT();
        tmp.type = EINPUT.KEYBOARD;
        tmp.U.ki.wVk = vk;
        tmp.U.ki.wScan = sc;
        tmp.U.ki.time = 0;
        tmp.U.ki.dwFlags = KEYEVENTF.UNICODE;
        tmp.U.ki.dwExtraInfo = UIntPtr.Zero;
        m_input.Add(tmp);

        tmp.U.ki.dwFlags |= KEYEVENTF.KEYUP;
        m_input.Add(tmp);
    }
}

/// <summary>
/// The main composer class. It gets input from the keyboard hook, and
/// acts depending on the global configuration and current keyboard state.
/// </summary>
static class Composer
{
    /// <summary>
    /// Initialize the composer.
    /// </summary>
    public static void Init()
    {
        StartMonitoringKeyboardLeds();
        CheckKeyboardLayout();
    }

    /// <summary>
    /// Terminate the composer.
    /// </summary>
    public static void Fini()
    {
        StopMonitoringKeyboardLeds();
    }

    /// <summary>
    /// Get input from the keyboard hook; return true if the key was handled
    /// and needs to be removed from the input chain.
    /// </summary>
    public static bool OnKey(WM ev, VK vk, SC sc, LLKHF flags)
    {
        // Remember when the user touched a key for the last time
        m_last_key_time = DateTime.Now;

        // Do nothing if we are disabled; NOTE: this disables stats, too
        if (Settings.Disabled.Value)
        {
            return false;
        }

        // We need to check the keyboard layout before we save the dead
        // key, otherwise we may be saving garbage.
        CheckKeyboardLayout();

        int dead_key = SaveDeadKey();
        bool ret = OnKeyInternal(ev, vk, sc, flags);
        RestoreDeadKey(dead_key);

        return ret;
    }

    private static bool OnKeyInternal(WM ev, VK vk, SC sc, LLKHF flags)
    {
        bool is_keydown = (ev == WM.KEYDOWN || ev == WM.SYSKEYDOWN);
        bool is_keyup = !is_keydown;
        bool add_to_sequence = is_keydown;
        bool is_capslock_hack = false;

        bool has_shift = (NativeMethods.GetKeyState(VK.SHIFT) & 0x80) != 0;
        bool has_altgr = (NativeMethods.GetKeyState(VK.LCONTROL) &
                          NativeMethods.GetKeyState(VK.RMENU) & 0x80) != 0;
        bool has_lrshift = (NativeMethods.GetKeyState(VK.LSHIFT) &
                            NativeMethods.GetKeyState(VK.RSHIFT) & 0x80) != 0;
        bool has_capslock = NativeMethods.GetKeyState(VK.CAPITAL) != 0;

        // Guess what the system would print if we weren’t interfering. If
        // a printable representation exists, use that. Otherwise, default
        // to its virtual key code.
        Key key = VkToKey(vk, sc, flags, has_shift, has_altgr, has_capslock);

        // If Caps Lock is on, and the Caps Lock hack is enabled, we check
        // whether this key without Caps Lock gives a non-ASCII alphabetical
        // character. If so, we replace “result” with the lowercase or
        // uppercase variant of that character.
        if (has_capslock && Settings.CapsLockCapitalizes.Value)
        {
            Key alt_key = VkToKey(vk, sc, flags, has_shift, has_altgr, false);

            if (alt_key.IsPrintable() && alt_key.ToString()[0] > 0x7f)
            {
                string str_upper = alt_key.ToString().ToUpper();
                string str_lower = alt_key.ToString().ToLower();

                // Hack for German keyboards: it seems that ToUpper() does
                // not properly change ß into ẞ.
                if (str_lower == "ß")
                    str_upper = "ẞ";

                if (str_upper != str_lower)
                {
                    key = new Key(has_shift ? str_lower : str_upper);
                    is_capslock_hack = true;
                }
            }
        }

        // Update statistics
        if (is_keydown)
        {
            // Update single key statistics
            Stats.AddKey(key);

            // Update key pair statistics if applicable
            if (DateTime.Now < m_last_key_time.AddMilliseconds(2000)
                 && m_last_key != null)
            {
                Stats.AddPair(m_last_key, key);
            }

            // Remember when we pressed a key for the last time
            m_last_key_time = DateTime.Now;
            m_last_key = key;
        }

        // If the special Synergy window has focus, we’re actually sending
        // keystrokes to another computer; disable WinCompose. Same if it is
        // a Cygwin X window.
        if (m_window_is_other_desktop)
        {
            return false;
        }

        // If we receive a keyup for the compose key while in emulation
        // mode, we’re done. Send a KeyUp event and exit emulation mode.
        if (is_keyup && CurrentState == State.Combination
             && key == m_current_compose_key)
        {
            Log.Debug("Combination Off");
            bool compose_key_was_altgr = m_compose_key_is_altgr;
            CurrentState = State.Idle;
            m_current_compose_key = new Key(VK.NONE);
            m_compose_key_is_altgr = false;
            m_compose_counter = 0;

            // If relevant, send an additional KeyUp for the opposite
            // key; experience indicates that it helps unstick some
            // applications such as mintty.exe.
            switch (m_current_compose_key.VirtualKey)
            {
                case VK.LMENU: SendKeyUp(VK.RMENU); break;
                case VK.RMENU: SendKeyUp(VK.LMENU);
                               // If keyup is RMENU and we have AltGr on this
                               // keyboard layout, send LCONTROL up too.
                               if (compose_key_was_altgr)
                                   SendKeyUp(VK.LCONTROL);
                               break;
                case VK.LSHIFT: SendKeyUp(VK.RSHIFT); break;
                case VK.RSHIFT: SendKeyUp(VK.LSHIFT); break;
                case VK.LCONTROL: SendKeyUp(VK.RCONTROL); break;
                case VK.RCONTROL: SendKeyUp(VK.LCONTROL); break;
            }

            return false;
        }

        // If this is the compose key and we’re idle, enter Sequence mode
        if (is_keydown && Settings.ComposeKeys.Value.Contains(key)
             && m_compose_counter == 0 && CurrentState == State.Idle)
        {
            Log.Debug("Now Composing");
            CurrentState = State.Sequence;
            m_current_compose_key = key;
            m_compose_key_is_altgr = key.VirtualKey == VK.RMENU &&
                                     m_possible_altgr_keys.Count > 0;
            ++m_compose_counter;

            // Lauch the sequence reset expiration thread
            // FIXME: do we need to launch a new thread each time the
            // compose key is pressed? Let's have a dormant thread instead
            if (Settings.ResetDelay.Value > 0)
            {
                new Thread(() =>
                {
                    while (CurrentState == State.Sequence &&
                            DateTime.Now < m_last_key_time.AddMilliseconds(Settings.ResetDelay.Value))
                        Thread.Sleep(50);
                    ResetSequence();
                }).Start();
            }

            return true;
        }

        // If this is a compose key KeyDown event and it’s already down, or it’s
        // a KeyUp and it’s already up, eat this event without forwarding it.
        if (key == m_current_compose_key
             && is_keydown == ((m_compose_counter & 1) != 0))
        {
            return true;
        }

        // Escape and backspace cancel the current sequence
        if (is_keydown && CurrentState == State.Sequence
             && (key.VirtualKey == VK.ESCAPE || key.VirtualKey == VK.BACK))
        {
            // FIXME: if a sequence was in progress, maybe print it!
            Log.Debug("No Longer Composing");
            ResetSequence();
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

        // If we are not currently composing a sequence, do nothing unless
        // one of our hacks forces us to send the key as a string (for
        // instance the Caps Lock capitalisation feature).
        if (CurrentState != State.Sequence)
        {
            if (is_capslock_hack && is_keydown)
            {
                SendString(key.ToString());
                return true;
            }

            // If this was a dead key, it will be completely ignored. But
            // it’s okay since we stored it.
            Log.Debug("Forwarding Key {0} to System (not composing)",
                      key.FriendlyName);
            return false;
        }

        //
        // From this point we know we are composing
        //

        // If the compose key is down and the user pressed a new key, maybe
        // instead of composing they want to do a key combination, such as
        // Alt+Tab or Windows+Up. So we abort composing and send the KeyDown
        // event for the Compose key that we previously discarded. The same
        // goes for characters that need AltGr when AltGr is the compose key.
        //
        // Never do this if the event is KeyUp.
        // Never do this if we already started a sequence
        // Never do this if the key is a modifier key such as shift or alt.
        if (m_compose_counter == 1 && is_keydown
             && m_sequence.Count == 0 && !key.IsModifier())
        {
            bool keep_original = Settings.KeepOriginalKey.Value;
            bool key_unusable = !key.IsUsable();
            bool altgr_combination = m_compose_key_is_altgr &&
                        m_possible_altgr_keys.ContainsKey(key.ToString());

            if (keep_original || key_unusable || altgr_combination)
            {
                Log.Debug("Combination On");
                bool compose_key_was_altgr = m_compose_key_is_altgr;
                ResetSequence();
                if (compose_key_was_altgr)
                {
                    // It’s necessary to use KEYEVENTF_EXTENDEDKEY otherwise the system
                    // does not understand that we’re sending AltGr.
                    SendKeyDown(VK.LCONTROL);
                    SendKeyDown(VK.RMENU, KEYEVENTF.EXTENDEDKEY);
                }
                else
                {
                    SendKeyDown(m_current_compose_key.VirtualKey);
                }
                CurrentState = State.Combination;
                return false;
            }
        }

        // If this is the compose key again, use our custom virtual key
        // FIXME: we don’t properly support compose keys that also normally
        // print stuff, such as `.
        if (key == m_current_compose_key)
        {
            ++m_compose_counter;
            key = new Key(VK.COMPOSE);

            // If the compose key is AltGr, we only add it to the sequence when
            // it’s a KeyUp event, otherwise we may be adding Multi_key to the
            // sequence while the user actually wants to enter an AltGr char.
            if (m_compose_key_is_altgr && m_compose_counter > 2)
                add_to_sequence = is_keyup;
        }

        // If the compose key is AltGr and it’s down, check whether the current
        // key needs translating.
        if (m_compose_key_is_altgr && (m_compose_counter & 1) != 0)
        {
            string altgr_variant;
            if (m_possible_altgr_keys.TryGetValue(key.ToString(), out altgr_variant))
            {
                key = new Key(altgr_variant);
                // Do as if we already released Compose, otherwise the next KeyUp
                // event will cause VK.COMPOSE to be added to the sequence…
                ++m_compose_counter;
            }
        }

        // If the key can't be used in a sequence, just ignore it.
        if (!key.IsUsable())
        {
            Log.Debug("Forwarding Key {0} to System (no possible sequence uses it)",
                      key.FriendlyName);
            return false;
        }

        // If we reached this point, everything else ignored this key, so it
        // is a key we must add to the current sequence.
        if (add_to_sequence)
        {
            Log.Debug("Adding to Sequence: {0}", key.FriendlyName);
            return AddToSequence(key);
        }

        return true;
    }

    /// <summary>
    /// Add a key to the sequence currently being built. If necessary, output
    /// the finished sequence or trigger actions for invalid sequences.
    /// </summary>
    private static bool AddToSequence(Key key)
    {
        KeySequence old_sequence = new KeySequence(m_sequence);
        m_sequence.Add(key);

        // We try the following, in this order:
        //  1. if m_sequence + key is a valid prefix, it means the user
        //     could type other characters to build a longer sequence,
        //     so just append key to m_sequence.
        //  2. if m_sequence + key is a valid sequence, we can't go further,
        //     we append key to m_sequence and output the result.
        //  3. if m_sequence is a valid sequence, the user didn't type a
        //     valid key, so output the m_sequence result _and_ process key.
        //  4. (optionally) try again 1. 2. and 3. ignoring case.
        //  5. none of the characters make sense, output all of them as if
        //     the user didn't press Compose.
        foreach (bool ignore_case in Settings.CaseInsensitive.Value ?
                              new bool[]{ false, true } : new bool[]{ false })
        {
            if (Settings.IsValidPrefix(m_sequence, ignore_case))
            {
                // Still a valid prefix, continue building sequence
                return true;
            }

            if (Settings.IsValidSequence(m_sequence, ignore_case))
            {
                string tosend = Settings.GetSequenceResult(m_sequence,
                                                           ignore_case);
                Stats.AddSequence(m_sequence);
                Log.Debug("Valid sequence! Sending {0}", tosend);
                ResetSequence();
                SendString(tosend);
                return true;
            }

            // Some code duplication with the above block, but this way
            // what we are doing is more clear.
            if (Settings.IsValidSequence(old_sequence, ignore_case))
            {
                string tosend = Settings.GetSequenceResult(old_sequence,
                                                           ignore_case);
                Stats.AddSequence(old_sequence);
                Log.Debug("Sending previously valid sequence {0}", tosend);
                ResetSequence();
                SendString(tosend);
                return false;
            }
        }

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

        ResetSequence();
        return true;
    }

    private static Key VkToKey(VK vk, SC sc, LLKHF flags, bool has_shift,
                               bool has_altgr, bool has_capslock)
    {
        byte[] keystate = new byte[256];
        NativeMethods.GetKeyboardState(keystate);
        keystate[(int)VK.SHIFT] = (byte)(has_shift ? 0x80 : 0x00);
        keystate[(int)VK.CONTROL] = (byte)(has_altgr ? 0x80 : 0x00);
        keystate[(int)VK.MENU] = (byte)(has_altgr ? 0x80 : 0x00);
        keystate[(int)VK.CAPITAL] = (byte)(has_capslock ? 0x01 : 0x00);

        // These two calls must be done together and in this order.
        string str_if_normal = VkToUnicode(vk, sc, keystate, flags);
        string str_if_dead = VkToUnicode(VK.SPACE);

        if (str_if_dead != " ")
            return new Key(str_if_dead);

        // Special case: we don't consider characters such as Esc as printable
        // otherwise they are not properly serialised in the config file.
        if (str_if_normal == "" || str_if_normal[0] < ' ')
            return new Key(vk);

        return new Key(str_if_normal);
    }

    private static string VkToUnicode(VK vk)
    {
        return VkToUnicode(vk, (SC)0, new byte[256], (LLKHF)0);
    }

    private static string VkToUnicode(VK vk, SC sc, byte[] keystate, LLKHF flags)
    {
        const int buflen = 4;
        byte[] buf = new byte[2 * buflen];
        int ret = NativeMethods.ToUnicode(vk, sc, keystate, buf, buflen, flags);
        if (ret > 0 && ret < buflen)
        {
            return Encoding.Unicode.GetString(buf, 0, ret * 2);
        }
        return "";
    }

    /// <summary>
    /// Check whether a string contains any Unicode surrogate characters.
    /// </summary>
    private static bool HasSurrogates(string str)
    {
        foreach (char ch in str)
            if (ch >= 0xd800 && ch < 0xe000)
                return true;
        return false;
    }

    private static void SendString(string str)
    {
        List<VK> modifiers = new List<VK>();

        /* HACK: GTK+ applications behave differently with Unicode, and some
         * applications such as XChat for Windows rename their own top-level
         * window, so we parse through the names we know in order to detect
         * a GTK+ application. */
        bool use_gtk_hack = m_window_is_gtk;

        /* HACK: Notepad++ is unable to output high plane Unicode characters,
         * so we rely on clipboard hacking when the composed string contains
         * such characters. */
        bool use_clipboard_hack = m_window_is_notepadpp && HasSurrogates(str);

        /* HACK: in MS Office, some symbol insertions change the text font
         * without returning to the original font. To avoid this, we output
         * a space character, then go left, insert our actual symbol, then
         * go right and backspace. */
        /* These are the actual window class names for Outlook and Word…
         * TODO: PowerPoint ("PP(7|97|9|10)FrameClass") */
        bool use_office_hack = m_window_is_office && Settings.InsertZwsp.Value;

        /* Clear keyboard modifiers if we need one of our custom hacks */
        if (use_gtk_hack || use_office_hack)
        {
            VK[] all_modifiers = new VK[]
            {
                VK.LSHIFT, VK.RSHIFT,
                VK.LCONTROL, VK.RCONTROL,
                VK.LMENU, VK.RMENU,
                /* Needs to be released, too, otherwise Caps Lock + é on
                 * a French keyboard will print garbage if Caps Lock is
                 * not released soon enough. See note below. */
                VK.CAPITAL,
            };

            foreach (VK vk in all_modifiers)
                if ((NativeMethods.GetKeyState(vk) & 0x80) == 0x80)
                    modifiers.Add(vk);

            foreach (VK vk in modifiers)
                SendKeyUp(vk);
        }

        if (use_gtk_hack)
        {
            /* XXX: We need to disable caps lock because GTK’s Shift-Ctrl-U
             * input mode (see below) doesn’t work when Caps Lock is on. */
            bool has_capslock = NativeMethods.GetKeyState(VK.CAPITAL) != 0;
            if (has_capslock)
                SendKeyPress(VK.CAPITAL);

            foreach (var ch in str)
            {
                if (false)
                {
                    /* FIXME: there is a possible optimisation here where we do
                     * not have to send the whole unicode sequence for regular
                     * ASCII characters. However, SendKeyPress() needs a VK, so
                     * we need an ASCII to VK conversion method, together with
                     * the proper keyboard modifiers. Maybe not worth it.
                     * Also, we cannot use KeySequence because GTK+ seems to
                     * ignore SendInput(). */
                    //SendKeyPress((VK)char.ToUpper(ch));
                }
                else
                {
                    /* Wikipedia says Ctrl+Shift+u, release, then type the four
                     * hex digits, and press Enter.
                     * (http://en.wikipedia.org/wiki/Unicode_input). */
                    SendKeyDown(VK.LCONTROL);
                    SendKeyDown(VK.LSHIFT);
                    SendKeyPress((VK)'U');
                    SendKeyUp(VK.LSHIFT);
                    SendKeyUp(VK.LCONTROL);

                    foreach (var key in String.Format("{0:X04} ", (short)ch))
                        SendKeyPress((VK)key);
                }
            }

            if (has_capslock)
                SendKeyPress(VK.CAPITAL);
        }
        else if (use_clipboard_hack)
        {
            // We do not use Clipboard.GetDataObject because I have been
            // unable to restore the clipboard properly. This is reasonable
            // and has been tested with several clipboard content types.
            var backup_text = Clipboard.GetText();
            var backup_image = Clipboard.GetImage();
            var backup_audio = Clipboard.GetAudioStream();
            var backup_files = Clipboard.GetFileDropList();

            // Use Shift+Insert instead of Ctrl-V because Ctrl-V will misbehave
            // if a Shift key is held down. Using Shift+Insert even works if the
            // compose key is Insert.
            Clipboard.SetText(str);
            SendKeyDown(VK.SHIFT);
            SendKeyPress(VK.INSERT);
            SendKeyUp(VK.SHIFT);
            Clipboard.Clear();

            if (!string.IsNullOrEmpty(backup_text))
                Clipboard.SetText(backup_text);
            if (backup_image != null)
                Clipboard.SetImage(backup_image);
            if (backup_audio != null)
                Clipboard.SetAudio(backup_audio);
            if (backup_files != null && backup_files.Count > 0)
                Clipboard.SetFileDropList(backup_files);
        }
        else
        {
            InputSequence Seq = new InputSequence();

            if (use_office_hack)
            {
                Seq.AddInput((ScanCodeShort)'\u200b');
                Seq.AddInput((VirtualKeyShort)VK.LEFT);
            }

            foreach (char ch in str)
            {
                Seq.AddInput((ScanCodeShort)ch);
            }

            if (use_office_hack)
            {
                Seq.AddInput((VirtualKeyShort)VK.RIGHT);
            }

            Seq.Send();
        }

        /* Restore keyboard modifiers if we needed one of our custom hacks */
        if (use_gtk_hack || use_office_hack)
        {
            foreach (VK vk in modifiers)
                SendKeyDown(vk);
        }
    }

    public static event EventHandler Changed = delegate {};

    /// <summary>
    /// Toggle the disabled state
    /// </summary>
    public static void ToggleDisabled()
    {
        Settings.Disabled.Value = !Settings.Disabled.Value;
        ResetSequence();
        // FIXME: this will no longer be necessary when "Disabled"
        // becomes a composer state of its own.
        Changed(null, new EventArgs());
    }

    /// <summary>
    /// Return whether WinCompose has been disabled
    /// </summary>
    public static bool IsDisabled()
    {
        return Settings.Disabled.Value;
    }

    private static void ResetSequence()
    {
        CurrentState = State.Idle;
        m_current_compose_key = new Key(VK.NONE);
        m_compose_key_is_altgr = false;
        m_compose_counter = 0;
        m_sequence.Clear();
    }

    private static void SendKeyDown(VK vk, KEYEVENTF flags = 0)
    {
        NativeMethods.keybd_event(vk, 0, flags, 0);
    }

    private static void SendKeyUp(VK vk, KEYEVENTF flags = 0)
    {
        NativeMethods.keybd_event(vk, 0, KEYEVENTF.KEYUP | flags, 0);
    }

    private static void SendKeyPress(VK vk, KEYEVENTF flags = 0)
    {
        SendKeyDown(vk, flags);
        SendKeyUp(vk, flags);
    }

    /// <summary>
    /// Attempt to enumerate all dead keys available on the current keyboard
    /// layout and cache the results in <see cref="m_possible_dead_keys"/>.
    /// </summary>
    private static void AnalyzeKeyboardLayout()
    {
        m_possible_dead_keys = new Dictionary<string, int>();
        m_possible_altgr_keys = new Dictionary<string, string>();

        // Try every keyboard key followed by space to see which ones are
        // dead keys. This way, when later we want to know if a dead key is
        // currently buffered, we just call ToUnicode(VK.SPACE) and match
        // the result with what we found here.
        string[] no_altgr = new string[0x200];
        byte[] state = new byte[256];

        for (int i = 0; i < 0x400; ++i)
        {
            VK vk = (VK)(i & 0xff);
            bool has_shift = (i & 0x100) != 0;
            bool has_altgr = (i & 0x200) != 0;

            state[(int)VK.SHIFT] = (byte)(has_shift ? 0x80 : 0x00);
            state[(int)VK.CONTROL] = (byte)(has_altgr ? 0x80 : 0x00);
            state[(int)VK.MENU] = (byte)(has_altgr ? 0x80 : 0x00);

            // First the key we’re interested in, then the space key
            string str_if_normal = VkToUnicode(vk, (SC)0, state, (LLKHF)0);
            string str_if_dead = VkToUnicode(VK.SPACE);

            // If the AltGr gives us a result and it’s different from without
            // AltGr, we need to remember it.
            string str = str_if_dead != " " ? str_if_dead : str_if_normal;
            if (has_altgr)
            {
                if (no_altgr[i - 0x200] != "" && str != "" && no_altgr[i - 0x200] != str)
                {
                    Log.Debug("VK {0} is “{1}” but “{2}” with AltGr",
                              vk.ToString(), no_altgr[i - 0x200], str);
                    m_possible_altgr_keys[no_altgr[i - 0x200]] = str;
                }
            }
            else
            {
                no_altgr[i] = str_if_dead != " " ? str_if_dead : str_if_normal;
            }

            // If the resulting string is not the space character, it means
            // that it was a dead key. Good!
            if (str_if_dead != " ")
                m_possible_dead_keys[str_if_dead] = i;
        }

        // Clean up key buffer
        VkToUnicode(VK.SPACE);
        VkToUnicode(VK.SPACE);
    }

    /// <summary>
    /// Save the dead key if there is one, since we'll be calling ToUnicode
    /// later on. This effectively removes any dead key from the ToUnicode
    /// internal buffer.
    /// </summary>
    private static int SaveDeadKey()
    {
        int dead_key = 0;
        string str = VkToUnicode(VK.SPACE);
        if (str != " ")
            m_possible_dead_keys.TryGetValue(str, out dead_key);
        return dead_key;
    }

    /// <summary>
    /// Restore a previously saved dead key. This should restore the ToUnicode
    /// internal buffer.
    /// </summary>
    private static void RestoreDeadKey(int dead_key)
    {
        if (dead_key == 0)
            return;

        byte[] state = new byte[256];

        VK vk = (VK)(dead_key & 0xff);
        bool has_shift = (dead_key & 0x100) != 0;
        bool has_altgr = (dead_key & 0x200) != 0;

        state[(int)VK.SHIFT] = (byte)(has_shift ? 0x80 : 0x00);
        state[(int)VK.CONTROL] = (byte)(has_altgr ? 0x80 : 0x00);
        state[(int)VK.MENU] = (byte)(has_altgr ? 0x80 : 0x00);

        VkToUnicode(vk, (SC)0, state, (LLKHF)0);
    }

    private static void CheckKeyboardLayout()
    {
        // Detect keyboard layout changes by querying the foreground window,
        // and apply the same layout to WinCompose itself.
        IntPtr hwnd = NativeMethods.GetForegroundWindow();
        uint pid, tid = NativeMethods.GetWindowThreadProcessId(hwnd, out pid);
        IntPtr active_layout = NativeMethods.GetKeyboardLayout(tid);

        m_window_is_gtk = false;
        m_window_is_notepadpp = false;
        m_window_is_office = false;
        m_window_is_other_desktop = false;

        const int len = 256;
        StringBuilder buf = new StringBuilder(len);
        if (NativeMethods.GetClassName(hwnd, buf, len) > 0)
        {
            string wclass = buf.ToString();

            if (wclass == "gdkWindowToplevel" || wclass == "xchatWindowToplevel"
                 || wclass == "hexchatWindowToplevel")
                m_window_is_gtk = true;

            if (wclass == "Notepad++")
                m_window_is_notepadpp = true;

            if (wclass == "rctrl_renwnd32" || wclass == "OpusApp")
                m_window_is_office = true;

            if (Regex.Match(wclass, "^(SynergyDesk|cygwin/x.*)$").Success)
                m_window_is_other_desktop = true;
        }

        if (active_layout != m_current_layout)
        {
            m_current_layout = active_layout;

            Log.Debug("Active window layout tid:{0} handle:0x{1:X} lang:0x{2:X}",
                      tid, (uint)active_layout >> 16, (uint)active_layout & 0xffff);

            if (active_layout != (IntPtr)0)
                NativeMethods.ActivateKeyboardLayout(active_layout, 0);

            tid = NativeMethods.GetCurrentThreadId();
            active_layout = NativeMethods.GetKeyboardLayout(tid);

            Log.Debug("WinCompose process layout tid:{0} handle:0x{1:X} lang:0x{2:X}",
                      tid, (uint)active_layout >> 16, (uint)active_layout & 0xffff);

            // We need to rebuild the list of dead keys
            AnalyzeKeyboardLayout();
        }
    }

    private static void StartMonitoringKeyboardLeds()
    {
        for (ushort i = 0; i < 4; ++i)
        {
            string kbd_name = "dos_kbd" + i.ToString();
            string kbd_class = @"\Device\KeyboardClass" + i.ToString();
            NativeMethods.DefineDosDevice(DDD.RAW_TARGET_PATH, kbd_name, kbd_class);
        }

        Changed += UpdateKeyboardLeds;
    }

    private static void StopMonitoringKeyboardLeds()
    {
        for (ushort i = 0; i < 4; ++i)
        {
            string kbd_name = "dos_kbd" + i.ToString();
            NativeMethods.DefineDosDevice(DDD.REMOVE_DEFINITION, kbd_name, null);
        }
        Changed -= UpdateKeyboardLeds;
    }

    public static void UpdateKeyboardLeds(object sender, EventArgs e)
    {
        var indicators = new KEYBOARD_INDICATOR_PARAMETERS();
        int buffer_size = (int)Marshal.SizeOf(indicators);

        // NOTE: I was unable to make IOCTL.KEYBOARD_QUERY_INDICATORS work
        // properly, but querying state with GetKeyState() seemed more
        // robust anyway. Think of the user setting Caps Lock as their
        // compose key, entering compose state, then suddenly changing
        // the compose key to Shift: the LED state would be inconsistent.
        if (NativeMethods.GetKeyState(VK.CAPITAL) != 0
             || (IsComposing() && m_current_compose_key.VirtualKey == VK.CAPITAL))
            indicators.LedFlags |= KEYBOARD.CAPS_LOCK_ON;

        if (NativeMethods.GetKeyState(VK.NUMLOCK) != 0
             || (IsComposing() && m_current_compose_key.VirtualKey == VK.NUMLOCK))
            indicators.LedFlags |= KEYBOARD.NUM_LOCK_ON;

        if (NativeMethods.GetKeyState(VK.SCROLL) != 0
             || (IsComposing() && m_current_compose_key.VirtualKey == VK.SCROLL))
            indicators.LedFlags |= KEYBOARD.SCROLL_LOCK_ON;

        for (ushort i = 0; i < 4; ++i)
        {
            indicators.UnitId = i;

            using (var handle = NativeMethods.CreateFile(@"\\.\" + "dos_kbd" + i.ToString(),
                           FileAccess.Write, FileShare.Read, IntPtr.Zero,
                           FileMode.Open, FileAttributes.Normal, IntPtr.Zero))
            {
                int bytesReturned;
                NativeMethods.DeviceIoControl(handle, IOCTL.KEYBOARD_SET_INDICATORS,
                                              ref indicators, buffer_size,
                                              IntPtr.Zero, 0, out bytesReturned,
                                              IntPtr.Zero);
            }
        }
    }

    /// <summary>
    /// The sequence being currently typed
    /// </summary>
    private static KeySequence m_sequence = new KeySequence();

    private static Key m_last_key;
    private static DateTime m_last_key_time = DateTime.Now;
    private static Dictionary<string, int> m_possible_dead_keys;
    private static Dictionary<string, string> m_possible_altgr_keys;
    // Initialise with -1 to make sure the above dictionaries are
    // properly initialised even if the layout is found to be 0x0.
    private static IntPtr m_current_layout = new IntPtr(-1);
    private static bool m_window_is_gtk = false;
    private static bool m_window_is_notepadpp = false;
    private static bool m_window_is_office = false;
    private static bool m_window_is_other_desktop = false;

    /// <summary>
    /// How many times we pressed and released compose.
    /// </summary>
    private static int m_compose_counter = 0;

    /// <summary>
    /// The compose key being used; only valid in state “Combination” for now.
    /// </summary>
    private static Key m_current_compose_key = new Key(VK.NONE);

    /// <summary>
    /// Whether the current compose key is AltGr
    /// </summary>
    private static bool m_compose_key_is_altgr = false;

    public enum State
    {
        Idle,
        Sequence,
        /// <summary>
        /// Combination mode: the compose key is held, but not for composing,
        /// more likely for a key combination such as Alt-Tab or Ctrl-Esc.
        /// </summary>
        Combination,
        // TODO: we probably want "Disabled" as another possible state
    };

    public static State CurrentState
    {
        get { return m_state; }

        private set
        {
            bool has_changed = (m_state != value);
            m_state = value;
            if (has_changed)
                Changed(null, new EventArgs());
        }
    }

    private static State m_state;

    /// <summary>
    /// Indicates whether a compose sequence is in progress
    /// </summary>
    public static bool IsComposing()
    {
        return CurrentState == State.Sequence;
    }
}

}
