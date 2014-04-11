
;
; Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.

#singleinstance force
#escapechar \
#persistent
#noenv

#include utils.ahk
#include constants.ahk
#include ui.ahk

; The name and version of this script
global app := "WinCompose"
global version := "0.6.0"
global website := "https://github.com/samhocevar/wincompose"

; Configuration directory and file
global config_dir := a_appdata . "\\" . app
global config_file := config_dir . "\\settings.ini"

; Activate debug messages?
;global have_debug := true

; Global runtime variables
global S := { typing: false          ; Is the user typing something?
            , disabled: false        ; Is everything disabled?
            , compose_down: false    ; Is the compose key down?
            , special_down: false    ; Is a special key down?
            , selected_char: ""      ; The character currently selected in GUI
            , selected_seq: "" }     ; The sequence currently selected

; Runtime configuration, imported from the config files
global R := { sequences:    {}
            , seq_count:    0
            , prefixes:     {}
            , descriptions: {}
            , keynames:     {}
            , compose_key:  C.keys.default
            , reset_delay:  C.delays.valid }

main()
return

;
; Main entry point
;

main()
{
    ; Don't crash if the icons cannot be found
    menu tray, useerrorlevel

    ; Early icon initialisation to prevent flashing
    tmp := C.files.resources
    menu tray, icon, %tmp%, 1

    load_settings()
    load_sequences()

    create_gui()

    set_ascii_hotkeys(true)
    set_compose_hotkeys(true)
}

;
; Handle Settings
;

load_settings()
{
    ; Read the compose key value and sanitise it if necessary
    iniread tmp, %config_file%, Global, % R.compose_key, ""
    R.compose_key := C.keys.valid.haskey(tmp) ? tmp : C.keys.default

    ; Read the reset delay value and sanitise it if necessary
    iniread tmp, %config_file%, Global, % R.reset_delay, ""
    R.reset_delay := C.delays.valid.haskey(tmp) ? tmp : C.delays.valid

    save_settings()
}

save_settings()
{
    filecreatedir %config_dir%
    iniwrite % R.compose_key, %config_file%, Global, compose_key
    iniwrite % R.reset_delay, %config_file%, Global, reset_delay
}

;
; Keystroke logic
;

send_keystroke(keystroke)
{
    static sequence := ""
    settimer, reset_callback, off

    if (S.disabled)
    {
        ; This should not happen, because the DISABLED state should
        ; completely disable the compose key and the other callbacks
        ; are automatically disabled in suspend mode, but I guess it
        ; doesn't hurt to have some fallback solution.
        if (keystroke == "compose")
            send % R.compose_key
        else
            send_raw(char)
        sequence := ""
    }
    else if (!S.typing)
    {
        ; Enter typing state if compose was pressed; otherwise we
        ; should not be here but send the character anyway.
        if (keystroke == "compose")
        {
            check_keyboard_layout()
            S.typing := true
            if (R.reset_delay > 0)
                settimer, reset_callback, % R.reset_delay
        }
        else
            send_raw(char)
        sequence := ""
    }
    else ; if (S.typing)
    {
        ; If the compose key is an actual character, don't cancel the compose
        ; sequence since the character could be used in the sequence itself.
        if (keystroke == "compose" && strlen(R.compose_key) == 1)
            keystroke := R.compose_key

        if (keystroke == "compose")
        {
            settimer, reset_callback, off
            sequence := ""
            S.typing := false
        }
        else
        {
            ; If this is a numpad key, replace it with its ASCII value
            if (C.keys.numpad.haskey(regexreplace(keystroke, "[$]", "")))
                keystroke := "$" C.keys.numpad[keystroke]

            ; The actual character is the last char of the keystroke
            char := substr(keystroke, strlen(keystroke))

            ; If holding shift, switch letters to uppercase
            if (asc(char) >= asc("a") && asc(char) <= asc("z"))
                if (getkeystate("Capslock", "T") != getkeystate("Shift"))
                    char := chr(asc(char) - asc("a") + asc("A"))

            sequence .= char

            info := "Sequence: [ " sequence " ]"

            if (has_sequence(sequence))
            {
                info .= " -> [ " get_sequence(sequence) " ]"
                send_unicode(get_sequence(sequence))
                S.typing := false
                sequence := ""
            }
            else if (!has_prefix(sequence))
            {
                info .= " ABORTED"
                send_raw(sequence)
                S.typing := false
                sequence := ""
            }
            else
            {
                if (R.reset_delay > 0)
                    settimer, reset_callback, % R.reset_delay
            }

            debug(info)
        }
    }

    refresh_systray()
    return

reset_callback:
    settimer, reset_callback, off
    sequence := ""
    S.typing := false
    refresh_systray()
    return

toggle_callback:
    if (S.disabled)
    {
        S.disabled := false
        S.typing := false
        set_compose_hotkeys(true)
    }
    else
    {
        set_compose_hotkeys(false)
        S.disabled := true
    }
    refresh_systray()
    return
}

send_unicode(char)
{
    ; HACK: GTK+ applications behave differently with Unicode, and some applications
    ; such as XChat for Windows rename their own top-level window
    for ignored, class in C.hacks.gdk_classes
    {
        if (winactive("ahk class " class))
        {
            sendinput % "{ctrl down}{shift down}u" num_to_hex(asc(char), 4) "{space}{shift up}{ctrl up}"
            return
        }
    }

    ; HACK: if the character is pure ASCII, we need raw send otherwise AHK
    ; may think it's a control character of some sort.
    if (asc(char) < 0x7f)
    {
        send {raw}%char%
        return
    }

    send %char%
}

send_raw(string)
{
    loop, parse, string
    {
        if (a_loopfield == " ")
            send {space}
        else
            sendinput {raw}%a_loopfield%
    }
}

check_keyboard_layout()
{
    critical on ; don't self-interrupt
    detecthiddenwindows on

    winget client_hwnd, ID, A
    client_thread := dllcall("user32\\GetWindowThreadProcessId", "uint", client_hwnd, "uint", 0, "uint")
    client_layout := dllcall("user32\\GetKeyboardLayout", "uint", client_thread, "uint")

    script_hwnd := dllcall("imm32\\ImmGetDefaultIMEWnd", "uint", a_scripthwnd, "uint")
    script_thread := dllcall("user32\\GetWindowThreadProcessId", "uint", script_hwnd, "uint", 0, "uint")
    script_layout := dllcall("user32\\GetKeyboardLayout", "uint", script_thread, "uint")

    if (client_layout == script_layout)
        return

    set_ascii_hotkeys(false)

    WM_INPUTLANGCHANGEREQUEST := 0x50
    postmessage %WM_INPUTLANGCHANGEREQUEST%, 0, %client_layout%, , ahk_id %script_hwnd%

    loop % 10
    {
        script_layout := dllcall("user32\\GetKeyboardLayout", "uint", script_thread, "uint")
        if (client_layout == script_layout)
            break
        sleep, 50
    }

    ;if (client_layout != script_layout)
    ;    msgbox, Something went wrong!

    set_ascii_hotkeys(true)
}

set_ascii_hotkeys(must_enable)
{
    ; Hotkeys for all shifted letters
    c1 := "abcdefghijklmnopqrstuvwxyz"

    ; Hotkeys for all other ASCII characters, including non-shifted letters
    c2 := c1 . "\ !""#$%&'()*+,-./0123456789:;<=>?@[\\]^_`{|}~"

    flag := must_enable ? "on" : "off"
    loop, parse, c1
        hotkey $+%a_loopfield%, key_callback, %flag%, useerrorlevel
    loop, parse, c2
        hotkey $%a_loopfield%, key_callback, %flag%, useerrorlevel
    for key, val in C.keys.numpad
        hotkey $%key%, key_callback, %flag%, useerrorlevel
}

;
; Handle special keys that may be used with the compose key in other situations,
; for instance Alt-Tab when Alt is the compose key, or Windows-Left/Right, etc.
;
set_special_hotkeys(must_enable)
{
    flag := must_enable ? "on" : "off"

    for ignored, key in C.keys.special
    {
        hotkey $%key%, special_callback, %flag%, useerrorlevel
        hotkey $%key% up, special_callback, %flag%, useerrorlevel
    }

    return

special_callback:
    ; This hotkey must always be active
    suspend permit
    key := regexreplace(a_thishotkey, "[^a-z0-9]*([a-z0-9]*).*", "$1", ret)
    if (instr(a_thishotkey, " up"))
    {
        S.special_down := false
        sendinput {%key% up}
    }
    else
    {
        ; Cancel any sequence in progress
        if (S.typing)
            send_keystroke("compose")

        if (!S.special_down)
            sendinput % "{" R.compose_key " down}"

        sendinput {%key% down}
        S.special_down := true
    }
    return
}

;
; Handle the key currently acting as the compose key
;
set_compose_hotkeys(must_enable)
{
    ; HACK: The ^ + ! variants are here just in case; for instance, Outlook 2010
    ; seems to automatically remap "Right Alt" to "Left Control + Right Alt", so
    ; obviously in this case we need to add hooks for LControl + RAlt.
    compose_prefixes := [ "$", "$^", "$+", "$!" ]

    if (must_enable)
    {
        ; Make sure that 1-character hotkeys are activated; these may have
        ; been deactivated by set_compose_hotkeys(false).
        for key, val in C.keys.valid
            if (strlen(key) == 1)
                hotkey $%key%, key_callback, on, useerrorlevel

        ; Activate the compose key for real
        for ignored, prefix in compose_prefixes
        {
            hotkey % prefix R.compose_key, compose_callback, on, useerrorlevel
            hotkey % prefix R.compose_key " up", compose_callback, on, useerrorlevel
        }
    }
    else
    {
        ; Disable any existing hotkeys
        for ignored, prefix in compose_prefixes
        {
            for key, val in C.keys.valid
            {
                hotkey % prefix key, off, useerrorlevel
                hotkey % prefix key " up", off, useerrorlevel
            }
        }
    }

    return

compose_callback:
    ; This hotkey must always be active
    suspend permit
    if (instr(a_thishotkey, " up"))
    {
        ; Compose was released
        S.compose_down := false
        ; Tell the system it was released, just in case a special
        ; hotkey was triggered.
        sendinput % "{" R.compose_key " up}"
        set_special_hotkeys(false)
    }
    else if (!S.compose_down)
    {
        ; Compose was pressed down -- protect against autorepeat
        S.compose_down := true
        send_keystroke("compose")
        set_special_hotkeys(true)
    }
    return
}

load_sequences()
{
    ; Read the default key file
    for ignored, file in C.files.keys
        read_key_file(file)

    ; Read the default sequence files
    for ignored, file in C.files.sequences
        read_sequence_file(file)

    ; Read a user-provided sequence file, if available
    envget userprofile, userprofile
    read_sequence_file(userprofile "\\.XCompose")
    read_sequence_file(userprofile "\\.XCompose.txt")

    info(_("Loaded @1@ Sequences", R.seq_count) "\n" _("Compose Key is @1@", C.keys.valid[R.compose_key]) "\n")
}

read_key_file(file)
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

    loop read, %file%
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

        R.keynames[regexreplace(left, "[<>]*", "")] := right
    }
}

read_sequence_file(file)
{
    FileEncoding UTF-8

    ; See read_key_file() for more explanations
    r_right := "^[^"":#]*:[^""#]*""(\\\\(.)|([^\\""]))"".*$"
    r_left := "^[ \\t]*(([ \\t]*<[^>]*>)*)([^:]*):.*$"

    loop read, %file%
    {
        ; Retrieve destination character(s) -- could be an UTF-16 sequence
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
            else if (R.keynames.haskey(a_loopfield))
                seq .= R.keynames[a_loopfield]
            else
                valid := false
        }

        ; If valid, add it to our list
        if (valid)
            add_sequence(seq, right, comment)
    }
}

; Register a compose sequence, and add all substring prefixes to our list
; of valid prefixes so that we can cancel invalid sequences early on.
add_sequence(seq, char, desc)
{
    ; Only increment sequence count if we're not replacing one
    if (!has_sequence(seq))
        R.seq_count += 1

    ; Insert into our lookup table
    R.sequences.insert(string_to_hex(seq), [seq, char])

    ; Insert into the prefix lookup table
    loop % strlen(seq) - 1
        R.prefixes.insert(string_to_hex(substr(seq, 1, a_index)), true)

    ; Insert into Unicode description list
    R.descriptions.insert(string_to_hex(seq), desc)
}

; Fill the default list view widget with all the compose rules that
; contain the string "filter", either in the compose sequence or in
; the description of the Unicode character.
fill_sequences(filter)
{
    stringlower filter_low, filter
    for k, v in R.sequences
    {
        seq := v[1]
        char := v[2]
        desc := R.descriptions[k]

        ; Filter out if necessary
        if (filter != char && !instr(seq, filter) && !instr(desc, filter_low))
            continue

        ; Insert into the GUI if applicable
        sequence := regexreplace(seq, "(.)", " $1")
        sequence := regexreplace(sequence, "  ", " space")
        sequence := regexreplace(sequence, "^ ", "")
        uni := "U+"

        if (strlen(char) == 1)
        {
            code := asc(char)
            digits := 4
        }
        else if (strlen(char) == 2)
        {
            code := (asc(substr(char, 1, 1)) - 0xd800) << 10
            code += asc(substr(char, 2, 1)) + 0x10000 - 0xdc00
            digits := 6
            ; HACK: prepend a non-printable character to fix sorting
            uni := chr(0x2063) . uni
        }

        uni .= num_to_hex(code, digits)

        lv_add("", sequence, char, uni)
    }
}

has_sequence(seq)
{
    return R.sequences.haskey(string_to_hex(seq))
}

get_sequence(seq)
{
    return R.sequences[string_to_hex(seq)][2]
}

get_description(seq)
{
    return R.descriptions[string_to_hex(seq)]
}

has_prefix(seq)
{
    return R.prefixes.haskey(string_to_hex(seq))
}

