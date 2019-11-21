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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinCompose
{

public static class KeyboardLeds
{
    public static void StartMonitoring()
    {
        lock (m_kbd_devices)
        {
            // Try to create up to 4 keyboard devices
            for (ushort id = 0; id < 4; ++id)
            {
                if (NativeMethods.DefineDosDevice(DDD.RAW_TARGET_PATH, $"dos_kbd{id}",
                                                  $@"\Device\KeyboardClass{id}"))
                    m_kbd_devices.Add(id);
            }
        }

        // Use a standard task timer to avoid blocking the composer thread
        EnableTimer();
        Composer.Changed += EnableTimer;
    }

    public static void StopMonitoring()
    {
        Composer.Changed -= EnableTimer;
        DisableTimer();

        lock (m_kbd_devices)
        {
            foreach (ushort id in m_kbd_devices)
                NativeMethods.DefineDosDevice(DDD.REMOVE_DEFINITION, $"dos_kbd{id}", null);
            m_kbd_devices.Clear();
        }
    }

    private static void EnableTimer()
        => m_update_timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5));

    private static void DisableTimer()
        => m_update_timer.Change(TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(-1));

    private static Timer m_update_timer = new Timer(Refresh);

    private static IList<ushort> m_kbd_devices = new List<ushort>();

    private static readonly IDictionary<VK, KEYBOARD> m_vk_to_flag = new Dictionary<VK, KEYBOARD>()
    {
        { VK.CAPITAL, KEYBOARD.CAPS_LOCK_ON },
        { VK.NUMLOCK, KEYBOARD.NUM_LOCK_ON },
        { VK.PAUSE,   KEYBOARD.SCROLL_LOCK_ON },
    };

    private static void Refresh(object o)
    {
        var indicators = new KEYBOARD_INDICATOR_PARAMETERS();
        int buffer_size = (int)Marshal.SizeOf(indicators);

        var led_vk = Settings.LedKey.Value[0].VirtualKey;

        // NOTE: I was unable to make IOCTL.KEYBOARD_QUERY_INDICATORS work
        // properly, but querying state with GetKeyState() seemed more
        // robust anyway. Think of the user setting Caps Lock as their
        // compose key, entering compose state, then suddenly changing
        // the compose key to Shift: the LED state would be inconsistent.
        foreach (var kv in m_vk_to_flag)
        {
            var vk = kv.Key;
            var flag = kv.Value;
            if (NativeMethods.GetKeyState(vk) != 0)
                indicators.LedFlags |= flag;
            else if (Composer.IsComposing)
            {
                if (Composer.CurrentComposeKey.VirtualKey == vk && led_vk == VK.COMPOSE
                     || led_vk == vk)
                    indicators.LedFlags |= flag;
            }
        }

        lock (m_kbd_devices)
        {
            foreach (ushort id in m_kbd_devices)
            {
                indicators.UnitId = id;

                using (var handle = NativeMethods.CreateFile($@"\\.\dos_kbd{id}",
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
    }
}

}

