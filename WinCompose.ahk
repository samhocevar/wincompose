
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

; Global state, one of WAITING, TYPING, or DISABLED
global state := "WAITING"

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

    if (state == "DISABLED")
    {
        ; This hould not happen; do nothing
    }
    else if (state == "WAITING")
    {
        ; Enter typing state if compose was pressed; otherwise we
        ; should not be here but send the character anyway.
        if (keystroke = "compose")
        {
            settimer, reset_callback, %reset_delay%
            state := "TYPING"
        }
        else
            send_raw(char)
        sequence := ""
    }
    else if (state == "TYPING")
    {
        if (keystroke = "compose")
        {
            sequence := ""
            state := "WAITING"
        }
        else
        {
            ; The actual character is the last char of the keystroke
            char := substr(keystroke, strlen(keystroke))

            ; If holding shift, switch letters to uppercase
            if (asc(char) >= asc("a") && asc(char) <= asc("z"))
                if (getkeystate("Capslock", "T") != getkeystate("Shift"))
                    char := chr(asc(char) - asc("a") + asc("A"))

            sequence .= char
            debug("Sequence: [ " sequence " ]")

            if (has_sequence(sequence))
            {
                tmp := get_sequence(sequence)
                send %tmp%
                state := "WAITING"
                sequence := ""
            }
            else if (!has_prefix(sequence))
            {
                debug("Disabling Dead End Sequence [ " sequence " ]")
                send_raw(sequence)
                state := "WAITING"
                sequence := ""
            }
        }
    }

    refresh_systray()
    return

reset_callback:
    settimer, reset_callback, off
    sequence := ""
    if (state == "TYPING")
        state := "WAITING"
    refresh_systray()
    return

toggle_callback:
    if (state == "DISABLED")
        state := "WAITING"
    else
        state := "DISABLED"
    refresh_systray()
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
    menu, tray, add, Show &Sequences…, sequences_callback
    menu, tray, add, &Disable, toggle_callback
    menu, tray, add, &Restart, restart_callback
    menu, tray, add
    if (have_debug)
        menu, tray, add, Key &History, history_callback
    menu, tray, add, &About, about_callback
    menu, tray, add, E&xit, exit_callback

    refresh_systray()

    ; Build the sequence list window
    static my_listbox, my_button
    gui font, s11
    gui font, s11, Courier New
    gui font, s11, Lucida Console
    gui font, s11, Consolas
    gui add, listview, vmy_listbox w800 r24, Sequence|Char|Unicode|Description
    gui font
    gui add, button, vmy_button w80 x730 default, Close

    ; Activate the compose key for real
    hotkey %compose_key%, compose_callback

    ; Activate these variants just in case; for instance, Outlook 2010 seems
    ; to automatically remap "Right Alt" to "Left Control + Right Alt".
    hotkey ^%compose_key%, compose_callback
    hotkey +%compose_key%, compose_callback
    hotkey !%compose_key%, compose_callback

    ; Hotkeys for all shifted letters
    chars := "abcdefghijklmnopqrstuvwxyz"
    loop, parse, chars
        hotkey $+%a_loopfield%, key_callback

    ; Hotkeys for all other ASCII characters, including non-shifted letters
    chars .= "\ !""#$%&'()*+,-./0123456789:;<=>?@[\\]^_`{|}~"
    loop, parse, chars
        hotkey $%a_loopfield%, key_callback

    ; Disable hotkeys; we only want them on during a compose sequence
    suspend on

    return

compose_callback:
    suspend ; We're not affected by suspend
    if (state != "DISABLED")
        send_keystroke("compose")
    return

key_callback:
    send_keystroke(a_thishotkey)
    return

sequences_callback:
    loop % 4
        lv_modifycol(a_index, "autohdr")
    lv_modifycol(2, "center") ; center the character column
    lv_modifycol(3, "sort")   ; sort the Unicode column
    gui show, autosize, WinCompose - List of Sequences
    return

restart_callback:
    reload
    return

history_callback:
    keyhistory
    return

about_callback:
    msgbox 64, WinCompose, WinCompose\nby Sam Hocevar <sam@hocevar.net>
    return

exit_callback:
    exitapp
    return

buttonclose:
    gui hide
    return

guiclose:
guiescape:
    gui hide
    return
}

refresh_systray()
{
    if (state == "WAITING")
    {
        suspend on
        menu, tray, uncheck, &Disable
        menu, tray, icon, %standard_icon%, , 1
        menu, tray, tip, WinCompose (active)
    }
    else if (state == "TYPING")
    {
        suspend off
        menu, tray, uncheck, &Disable
        menu, tray, icon, %active_icon%
        menu, tray, tip, WinCompose (typing)
    }
    else if (state == "DISABLED")
    {
        suspend on
        menu, tray, check, &Disable
        ; TODO: use icon groups here
        menu, tray, icon, %disabled_icon%, , 1
        menu, tray, tip, WinCompose (disabled)
    }
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

        ; Retrieve comment, if any
        comment := regexreplace(a_loopreadline, "^.*#[^""][ \t]*(.*)$", "$1", ret)
        if (ret != 1)
            comment := ""

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
            add_sequence(seq, right, comment)
            count += 1
        }
    }

    info("Loaded " count " Sequences")
}

; We need to encode our strings somehow because AutoHotKey objects have
; case-insensitive hash tables. How retarded is that? Also, make sure the
; first character is special
string_to_hex(str)
{
    hex = *
    loop, parse, str
        hex .= num_to_hex(asc(a_loopfield), 2)
    return hex
}

; Convert a number to a hexadecimal string with a minimum number of digits
num_to_hex(x, mindigits)
{
    chars := "0123456789ABCDEF"
    ret := ""
    if (x == 0)
        return "0"
    while (x > 0)
    {
        ret := substr(chars, 1 + mod(x, 16), 1) . ret
        x /= 16
    }
    while (strlen(ret) < mindigits)
        ret := "0" . ret
    return ret
}

; Register a compose sequence, and add all substring prefixes to our list
; of valid prefixes so that we can cancel invalid sequences early on.
add_sequence(key, val, desc)
{
    global s, p, listview
    if (!s)
    {
        s := {}
        p := {}
    }

    ; Insert into our lookup table
    s.insert(string_to_hex(key), val)
    loop % strlen(key) - 1
        p.insert(string_to_hex(substr(key, 1, a_index)), true)

    ; Insert into the GUI
    sequence := "♦" . regexreplace(key, "(.)", " $1")
    sequence := regexreplace(sequence, "  ", " {spc}")
    result := val
    uni := "U+" . num_to_hex(asc(val), 4)
    stringlower desc, desc
    lv_add("", sequence, val, uni, desc)
}

has_sequence(key)
{
    global s
    return s.haskey(string_to_hex(key))
}

get_sequence(key)
{
    global s
    return s[string_to_hex(key)]
}

has_prefix(key)
{
    global p
    return p.haskey(string_to_hex(key))
}

