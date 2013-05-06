
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

; The name and version of this script
global app := "WinCompose"
global version := "0.4.0"

; Configuration directory and file
global config_dir := a_appdata . "\\" . app
global config_file := config_dir . "\\settings.ini"

; GUI window title
global gui_title := app . " - List of Sequences"

; About box text
global about_text := app . " v" . version . "\n"
       about_text .= "\n"
       about_text .= "by Sam Hocevar <sam@hocevar.net>\n"
       about_text .= "running on AHK v" . a_ahkversion . "\n"

; List of keys that can be used for Compose
global valid_keys := { "Left Alt"      : "LAlt"
                     , "Right Alt"     : "RAlt"
                     , "Left Control"  : "LControl"
                     , "Right Control" : "RControl"
                     , "Left Windows"  : "LWin"
                     , "Right Windows" : "RWin"
                     , "Caps Lock"     : "CapsLock"
                     , "Num Lock"      : "NumLock"
                     , "Pause"         : "Pause"
                     , "Scroll Lock"   : "ScrollLock" }

; Resource files
global compose_file := "res/Compose.txt"
global keys_file := "res/Keys.txt"
global standard_icon := "res/wc.ico"
global active_icon := "res/wca.ico"
global disabled_icon := "res/wcd.ico"

; Activate debug messages?
global have_debug := false

; Global runtime variables
global state, compose_key, reset_delay

main()
return

;
; Main entry point
;

main()
{
    ; Early icon initialisation to prevent flashing
    menu tray, icon, %standard_icon%

    ; Global state, one of WAITING, TYPING, or DISABLED
    state := "WAITING"

    load_settings()
    load_sequences()
    setup_ui()
}

;
; Handle Settings
;

load_settings()
{
    iniread, compose_key, %config_file%, Global, compose_key, ""
    iniread, reset_delay, %config_file%, Global, reset_delay, 5000

    ; Sanitize configuration just in case
    if (!valid_keys.haskey(compose_key))
        compose_key := "Right Alt"

    save_settings()
}

save_settings()
{
    filecreatedir, %config_dir%
    iniwrite, %compose_key%, %config_file%, Global, compose_key
    iniwrite, %reset_delay%, %config_file%, Global, reset_delay
}

;
; Utility functions
;

send_keystroke(keystroke)
{
    static sequence := ""

    if (state == "DISABLED")
    {
        ; This should not happen; do nothing
    }
    else if (state == "WAITING")
    {
        ; Enter typing state if compose was pressed; otherwise we
        ; should not be here but send the character anyway.
        if (keystroke == "compose")
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
        if (keystroke == "compose")
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
        if (a_loopfield == " ")
            send {Space}
        else
            sendinput {raw}%a_loopfield%
    }
}

info(string)
{
    traytip, %app%, %string%, 10, 1
}

debug(string)
{
    if (have_debug)
        traytip, %app%, %string%, 10, 1
}

setup_ui()
{
    onexit exit_callback

    ; The hotkey selection menu
    for key, val in valid_keys
        menu, hotkeymenu, add, %key%, hotkeymenu_callback

    ; Build the menu
    menu tray, click, 1
    menu tray, NoStandard
    menu tray, add, &Sequences…, showgui_callback
    menu tray, add, Compose Key, :hotkeymenu
    menu tray, add, &Disable, toggle_callback
    menu tray, add, &Restart, restart_callback
    menu tray, add
    if (have_debug)
        menu tray, add, Key &History, history_callback
    menu tray, add, &About, about_callback
    menu tray, add, E&xit, exit_callback
    menu tray, default, &Sequences…

    ; Build the sequence list window
    global my_listbox, my_text, my_edit, my_button
    gui +resize +minsize300x115
    gui font, s11
    gui font, s11, Courier New
    gui font, s11, Lucida Console
    gui font, s11, Consolas
    gui add, listview, vmy_listbox w700 r18, Sequence|Char|Unicode|Description
    gui font
    gui add, text, vmy_text, Search Filter:
    gui add, edit, vmy_edit gedit_callback
    gui add, button, vmy_button default, Close

    ; Hotkeys for all shifted letters
    chars := "abcdefghijklmnopqrstuvwxyz"
    loop, parse, chars
        hotkey $+%a_loopfield%, key_callback

    ; Hotkeys for all other ASCII characters, including non-shifted letters
    chars .= "\ !""#$%&'()*+,-./0123456789:;<=>?@[\\]^_`{|}~"
    loop, parse, chars
        hotkey $%a_loopfield%, key_callback

    refresh_systray()
    refresh_bindings()

    return

key_callback:
    send_keystroke(a_thishotkey)
    return

hotkeymenu_callback:
    compose_key := a_thismenuitem
    refresh_systray()
    refresh_bindings()
    return

restart_callback:
    save_settings()
    reload
    return

history_callback:
    keyhistory
    return

about_callback:
    msgbox 64, %app%, %about_text%
    return

exit_callback:
    save_settings()
    exitapp
    return

guisize:
    if (a_eventinfo != 1) ; Ignore minimising
    {
        w := a_guiwidth
        h := a_guiheight
        guicontrol move, my_listbox, % "w" (w - 16) " h" (h - 45)
        guicontrol move, my_text, % "y" (h - 26)
        guicontrol move, my_edit, % "x80 w" (w - 220) " y" (h - 30)
        guicontrol move, my_button, % "x" (w - 87) " y" (h - 30) " w80"
    }
    return

edit_callback:
    critical on ; don't self-interrupt or we will corrupt the listview
    refresh_gui()
    return

showgui_callback:
    if (winexist(gui_title))
        goto hidegui_callback
    refresh_gui()
    gui show, , %gui_title%
    guicontrol focus, my_edit
    return

hidegui_callback:
buttonclose:
guiclose:
guiescape:
    gui hide
    return
}

refresh_systray()
{
    if (state == "WAITING")
    {
        ; Disable hotkeys; we only want them on during a compose sequence
        suspend on
        menu tray, uncheck, &Disable
        menu tray, icon, %standard_icon%, , 1
        menu tray, tip, %app% (active)
    }
    else if (state == "TYPING")
    {
        suspend off
        menu tray, uncheck, &Disable
        menu tray, icon, %active_icon%
        menu tray, tip, %app% (typing)
    }
    else if (state == "DISABLED")
    {
        suspend on
        menu tray, check, &Disable
        ; TODO: use icon groups here
        menu tray, icon, %disabled_icon%, , 1
        menu tray, tip, %app% (disabled)
    }

    for key, val in valid_keys
    {
        if (key == compose_key)
            menu, hotkeymenu, check, %key%
        else
            menu, hotkeymenu, uncheck, %key%
    }
}

refresh_bindings()
{
    ; Disable any existing hotkeys
    for key, val in valid_keys
    {
        hotkey %val%, off, useerrorlevel
        hotkey ^%val%, off, useerrorlevel
        hotkey +%val%, off, useerrorlevel
        hotkey !%val%, off, useerrorlevel
    }

    keysym := valid_keys[compose_key]

    ; Activate the compose key for real
    hotkey %keysym%, compose_callback

    ; Activate these variants just in case; for instance, Outlook 2010 seems
    ; to automatically remap "Right Alt" to "Left Control + Right Alt".
    hotkey ^%keysym%, compose_callback
    hotkey +%keysym%, compose_callback
    hotkey !%keysym%, compose_callback

    return

compose_callback:
    suspend ; We're not affected by suspend
    if (state != "DISABLED")
        send_keystroke("compose")
    return
}

refresh_gui()
{
    lv_delete()
    guicontrolget my_edit
    fill_sequences(my_edit)
    loop % 4
        lv_modifycol(a_index, "autohdr")
    lv_modifycol(2, "center") ; center the character column
    lv_modifycol(3, "sort")   ; sort the Unicode column
}

; Read key symbols from a key file, then read compose sequences
; from an X11 compose file
load_sequences()
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

    info("Loaded " count " Sequences\nCompose Key: " compose_key)
}

; We need to encode our strings somehow because AutoHotKey objects have
; case-insensitive hash tables. How retarded is that? Also, make sure the
; first character is special
string_to_hex(str)
{
    hex := "*"
    loop, parse, str
        hex .= num_to_hex(asc(a_loopfield), 2)
    return hex
}

; Convert a number to a hexadecimal string with a minimum number of digits
num_to_hex(x, mindigits)
{
    chars := "0123456789ABCDEF"
    ret := ""
    while (x > 0)
    {
        ret := substr(chars, 1 + mod(x, 16), 1) . ret
        x /= 16
    }
    while (strlen(ret) < mindigits || strlen(ret) < 1)
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
    stringlower desc, desc
    s.insert(string_to_hex(key), [key, val, desc])

    ; Insert into the prefix lookup table
    loop % strlen(key) - 1
        p.insert(string_to_hex(substr(key, 1, a_index)), true)
}

fill_sequences(filter)
{
    global s
    stringlower filter_low, filter
    for k, v in s
    {
        key := v[1]
        val := v[2]
        desc := v[3]

        ; Insert into the GUI if applicable
        if (filter == val || instr(key, filter) || instr(desc, filter_low))
        {
            sequence := "♦" . regexreplace(key, "(.)", " $1")
            sequence := regexreplace(sequence, "  ", " {spc}")
            result := val
            uni := "U+" . num_to_hex(asc(val), 4)
            lv_add("", sequence, val, uni, desc)
        }
    }
}

has_sequence(key)
{
    global s
    return s.haskey(string_to_hex(key))
}

get_sequence(key)
{
    global s
    return s[string_to_hex(key)][2]
}

has_prefix(key)
{
    global p
    return p.haskey(string_to_hex(key))
}

