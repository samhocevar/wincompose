
;
; Copyright: (c) 2013 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.

#SingleInstance force
#EscapeChar \
#Persistent
#NoEnv

; Compose Key: one of RAlt, LAlt, LControl, RControl, RWin, LWin, Esc,
; Insert, Numlock, Tab
global compose_key := "RAlt"

; Resource files
global compose_file := "res/Compose"
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
            sendraw %a_loopfield%
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

    ; Activate hotkeys for all ASCII characters, including shift for letters
    chars := "abcdefghijklmnopqrstuvwxyz"
    loop, parse, chars
        hotkey, $+%a_loopfield%, hotkey_callback

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

; Read compose sequences from an X11 compose file
setup_sequences()
{
    FileEncoding UTF-8
    count := 0
    loop read, %compose_file%
    {
        ; Check whether we get a character between quotes after a colon,
        ; that's our destination character.
        r_right := "^[^"":#]*:[^""#]*""(\\.|[^\\""])*"".*$"
        if (!regexmatch(a_loopreadline, r_right))
            continue
        right := regexreplace(a_loopreadline, r_right, "$1")

        ; Everything before that colon is our sequence, only keep it if it
        ; starts with "<Multi_key>".
        r_left := "^[ \\t]*<Multi_key>([^:]*):.*$"
        if (!regexmatch(a_loopreadline, r_left))
            continue
        left := regexreplace(a_loopreadline, r_left, "$1")
        left := regexreplace(left, "[ \\t]*", "")

        ; Now replace all special key names to build our sequence
        valid := true
        seq =
        loop, parse, left, "<>"
        {
            decoder := { "space":        " " ; 0x20
                       , "exclam":       "!" ; 0x21
                       , "quotedbl":    """" ; 0x22
                       , "numbersign":   "#" ; 0x23
                       , "dollar":       "$" ; 0x24
                       , "percent":      "%" ; 0x25
                       , "ampersand":    "&" ; 0x26 XXX: Is this the right name?
                       , "apostrophe":   "'" ; 0x27
                       , "parenleft":    "(" ; 0x28
                       , "parenright":   ")" ; 0x29
                       , "asterisk":     "*" ; 0x2a
                       , "plus":         "+" ; 0x2b
                       , "comma":        "," ; 0x2c
                       , "minus":        "-" ; 0x2d
                       , "period":       "." ; 0x2e
                       , "slash":        "/" ; 0x2f
                       , "colon":        ":" ; 0x3a
                       , "semicolon":    ";" ; 0x3b
                       , "less":         "<" ; 0x3c
                       , "equal":        "=" ; 0x3d
                       , "greater":      ">" ; 0x3e
                       , "question":     "?" ; 0x3f
                       , "bracketleft":  "[" ; 0x5b
                       , "backslash":   "\\" ; 0x5c
                       , "bracketright": "]" ; 0x5d
                       , "asciicircum":  "^" ; 0x5e
                       , "underscore":   "_" ; 0x5f
                       , "grave":        "`" ; 0x60
                       , "braceleft":    "{" ; 0x7b
                       , "bar":          "|" ; 0x7c
                       , "braceright":   "}" ; 0x7d
                       , "asciitilde":   "~" } ; 0x7e
            if (strlen(a_loopfield) <= 1)
                seq .= a_loopfield
            else if (decoder.haskey(a_loopfield))
                seq .= decoder[a_loopfield]
            else
                valid := false
        }

        ; If still not valid, drop it
        if (!valid)
            continue

        add_sequence(seq, right)
        count += 1
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

