
;
; Copyright: (c) 2013 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.

#singleinstance force
#escapechar \
#persistent
#noenv


; Compose Key: one of RAlt, LAlt, LControl, RControl, RWin, LWin, Esc,
; Insert, Numlock, Tab
global compose_key := "RAlt"

; Resource files
global compose_file := "res/Compose.txt"
global keys_file := "res/Keys.txt"
global standard_icon := "res/wc.ico"
global active_icon := "res/wca.ico"
global disabled_icon := "res/wcd.ico"

; Reset Delay: milliseconds until reset
global reset_delay := 5000

; Activate debug messages?
global have_debug := false

main()
return

;
; Main entry point
;

main()
{
    setup_ui()
    setup_sequences()
}

;
; Utility functions
;

send_keystroke(keystroke)
{
    static sequence =
    static compose := false
    static active := true

    ; The actual character is the last char of the keystroke
    char := substr(keystroke, strlen(keystroke))

    ; If holding shift, switch letters to uppercase
    if (getkeystate("Capslock", "T") != getkeystate("Shift"))
        if (asc(char) >= asc("a") && asc(char) <= asc("z"))
            char := chr(asc(char) - asc("a") + asc("A"))

    if (!compose && keystroke != "compose")
    {
        if (keystroke != "compose")
            send_raw(char)
        sequence =
        compose := false
        return
    }

    if (keystroke = "compose")
    {
        debug("Compose key pressed")
        sequence =
        compose := !compose
        if (compose)
        {
            settimer, reset_callback, %reset_delay%
            menu, tray, icon, %active_icon%
        }
        else
            menu, tray, icon, %standard_icon%
        return
    }

    sequence .= char

    debug("Sequence: [ " sequence " ]")

    if (has_sequence(sequence))
    {
        tmp := get_sequence(sequence)
        send %tmp%
        sequence =
        compose := false
        menu, tray, icon, %standard_icon%
    }
    else if (!has_prefix(sequence))
    {
        debug("Disabling Dead End Sequence [ " sequence " ]")
        send_raw(sequence)
        sequence =
        compose := false
        menu, tray, icon, %standard_icon%
    }

    return

reset_callback:
    sequence =
    compose := false
    if (active)
        menu, tray, icon, %standard_icon%
    settimer, reset_callback, Off
    return

toggle_callback:
    active := !active
    if (active)
    {
        suspend, off
        menu, tray, uncheck, &Disable
        menu, tray, icon, %standard_icon%
        menu, tray, tip, WinCompose (active)
    }
    else
    {
        menu, tray, check, &Disable
        ; TODO: use icon groups here
        menu, tray, icon, %disabled_icon%, , 1 ; freeze icon
        menu, tray, tip, WinCompose (disabled)
        suspend, on
    }
    return
}

send_raw(string)
{
    loop, parse, string
    {
        if (a_loopfield = " ")
            send {Space}
        else
            sendinput {raw}%a_loopfield%
    }
}

info(string)
{
    traytip, WinCompose, %string%, 10, 1
}

debug(string)
{
    if (have_debug)
        traytip, WinCompose, %string%, 10, 1
}

setup_ui()
{
    ; Build the menu
    menu, tray, click, 1
    menu, tray, NoStandard
    menu, tray, Add, &Disable, toggle_callback
    menu, tray, Add, &Restart, restart_callback
    menu, tray, Add
    if (have_debug)
        menu, tray, Add, Key &History, history_callback
    menu, tray, Add, &About, about_callback
    menu, tray, Add, E&xit, exit_callback
    menu, tray, icon, %standard_icon%
    menu, tray, tip, WinCompose (active)

    ; Activate the compose key for real
    hotkey, %compose_key%, compose_callback

    ; Activate these variants just in case; for instance, Outlook 2010 seems
    ; to automatically remap "Right Alt" to "Left Control + Right Alt".
    hotkey, ^%compose_key%, compose_callback
    hotkey, +%compose_key%, compose_callback
    hotkey, !%compose_key%, compose_callback

    ; Hotkeys for all shifted letters
    chars := "abcdefghijklmnopqrstuvwxyz"
    loop, parse, chars
        hotkey, $+%a_loopfield%, hotkey_callback

    ; Hotkeys for all other ASCII characters, including non-shifted letters
    chars .= "\ !""#$%&'()*+,-./0123456789:;<=>?@[\\]^_`{|}~"
    loop, parse, chars
        hotkey, $%a_loopfield%, hotkey_callback

    return

compose_callback:
    send_keystroke("compose")
    return

hotkey_callback:
    send_keystroke(a_thishotkey)
    return

restart_callback:
    reload
    return

history_callback:
    keyhistory
    return

about_callback:
    msgbox, 64, WinCompose, WinCompose\nby Sam Hocevar <sam@hocevar.net>
    return

exit_callback:
    exitapp
    return
}

; Read key symbols from a key file, then read compose sequences
; from an X11 compose file
setup_sequences()
{
    FileEncoding UTF-8

    ; Regex to match a character or group of characters between quotes after
    ; a colon, i.e. the X in lines such as:
    ;  ... any stuff ... : "X"  # optional comment
    ; XXX: The result is put in either $2 or $3.
    r_right := "^[^"":#]*:[^""#]*""(\\\\(.)|([^\\""]))"".*$"

    ; Regex to match a key sequence between brackets and before a colon,
    ; such as:
    ;  <key> <other_key><j> <more_keys>  : ... any stuff ...
    r_left := "^[ \\t]*(([ \\t]*<[^>]*>)*)([^:]*):.*$"

    keys := {}
    loop read, %keys_file%
    {
        ; Retrieve destination character
        right := regexreplace(a_loopreadline, r_right, "$2$3", ret)
        if (ret != 1)
            continue

        ; Retrieve sequence (in this case, only one key)
        left := regexreplace(a_loopreadline, r_left, "$1", ret)
        if (ret != 1)
            continue
        left := regexreplace(left, "[ \\t]*", "")

        keys[regexreplace(left, "[<>]*", "")] := right
    }

    count := 0
    loop read, %compose_file%
    {
        ; Retrieve destination character
        right := regexreplace(a_loopreadline, r_right, "$2$3", ret)
        if (ret != 1)
            continue

        ; Retrieve sequence
        left := regexreplace(a_loopreadline, r_left, "$1", ret)
        if (ret != 1)
            continue
        left := regexreplace(left, "[ \\t]*", "")

        ; Check that the sequence starts with <Multi_key>
        left := regexreplace(left, "^<Multi_key>", "", ret)
        if (ret != 1)
            continue

        ; Now replace all special key names to build our sequence and
        ; check whether it's valid
        valid := true
        seq := ""
        loop, parse, left, "<>"
        {
            if (strlen(a_loopfield) <= 1)
                seq .= a_loopfield
            else if (keys.haskey(a_loopfield))
                seq .= keys[a_loopfield]
            else
                valid := false
        }

        ; If valid, add it to our list
        if (valid)
        {
            add_sequence(seq, right)
            count += 1
        }
    }

    info("Loaded " count " Sequences")
}

; We need to encode our strings somehow because AutoHotKey objects have
; case-insensitive hash tables. How retarded is that? Also, make sure the
; first character is special
to_hex(str)
{
    hex = *
    loop, parse, str
        hex .= asc(a_loopfield)
    return hex
}

; Register a compose sequence, and add all substring prefixes to our list
; of valid prefixes so that we can cancel invalid sequences early on.
add_sequence(key, val)
{
    global s, p
    if (!s)
    {
        s := {}
        p := {}
    }

    s.insert(to_hex(key), val)
    loop % strlen(key) - 1
        p.insert(to_hex(substr(key, 1, a_index)), true)
}

has_sequence(key)
{
    global s
    return s.haskey(to_hex(key))
}

get_sequence(key)
{
    global s
    return s[to_hex(key)]
}

has_prefix(key)
{
    global p
    return p.haskey(to_hex(key))
}

