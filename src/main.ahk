
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
global version := "0.6.4"
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
global R := { sequences:     {}      ; List of valid sequences
            , sequences_alt: {}      ; Alt table for case insensitive
            , seq_count:     0
            , prefixes:      {}      ; List of valid prefixes
            , prefixes_alt:  {}      ; Alt table for case insensitive
            , descriptions:  {}
            , keynames:      {}
            , compose_key:   C.keys.default
            , reset_delay:   C.delays.valid
            , opt_case:      false
            , opt_discard:   false
            , opt_beep:      false }

main()
return

;
; Main entry point
;

main()
{
    ; We know what we are doing, so run at full speed
    setbatchlines -1
    listlines off

    ; Don't crash if the icons cannot be found
    menu tray, useerrorlevel

    ; Early icon initialisation to prevent flashing
    tmp := C.files.resources
    menu tray, icon, %tmp%, 1

    load_settings()
    load_sequences()

    create_gui()

    set_printable_hotkeys(true)
    set_compose_hotkeys(true)
}

;
; Handle Settings
;

load_settings()
{
    ; Read the compose key value and sanitise it if necessary
    iniread tmp, %config_file%, Global, % "compose_key", % ""
    R.compose_key := C.keys.valid.haskey(tmp) ? tmp : C.keys.default

    ; Read the reset delay value and sanitise it if necessary
    iniread tmp, %config_file%, Global, % "reset_delay", % ""
    R.reset_delay := C.delays.valid.haskey(tmp) ? tmp : C.delays.default

    iniread tmp, %config_file%, Global, % "case_insensitive", false
    R.opt_case := tmp == "true"
    iniread tmp, %config_file%, Global, % "discard_on_invalid", false
    R.opt_discard := tmp == "true"
    iniread tmp, %config_file%, Global, % "beep_on_invalid", false
    R.opt_beep := tmp == "true"

    save_settings()
}

save_settings()
{
    filecreatedir %config_dir%
    iniwrite % R.compose_key, %config_file%, Global, % "compose_key"
    iniwrite % R.reset_delay, %config_file%, Global, % "reset_delay"
    iniwrite % R.opt_case ? "true" : "false", %config_file%, Global, % "case_insensitive"
    iniwrite % R.opt_discard ? "true" : "false", %config_file%, Global, % "discard_on_invalid"
    iniwrite % R.opt_beep ? "true" : "false", %config_file%, Global, % "beep_on_invalid"
}

;
; Keystroke logic
;

send_keystroke(keystroke)
{
    static sequence := ""
    settimer on_delay_expired, off

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
                settimer on_delay_expired, % R.reset_delay
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
            settimer on_delay_expired, off
            sequence := ""
            S.typing := false
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

            info := "Sequence: [ " sequence " ]"

            if (has_sequence(sequence))
            {
                info .= " -> [ " get_sequence(sequence) " ]"
                send_unicode(get_sequence(sequence))
                S.typing := false
                sequence := ""
            }
            else if (has_prefix(sequence))
            {
                if (R.reset_delay > 0)
                    settimer on_delay_expired, % R.reset_delay
            }
            else
            {
                info .= " ABORTED"
                if (!R.opt_discard)
                    send_raw(sequence)
                S.typing := false
                sequence := ""
                if (R.opt_beep)
                    soundplay *-1
            }

            debug(info)
        }
    }

    refresh_systray()
    return

on_delay_expired:
    settimer on_delay_expired, off
    sequence := ""
    S.typing := false
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

    set_printable_hotkeys(false)

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

    set_printable_hotkeys(true)
}

set_printable_hotkeys(must_enable)
{
    flag := must_enable ? "on" : "off"

    ; Register hotkeys for all keys that potentially display something, including
    ; combinations with Shift or Ctrl+Alt (aka AltGr). We must use virtual keys
    ; instead of characters because we have no way to know what keys are available
    ; on the currently active keyboard layout. The list of virtual keys has been
    ; manually compiled into C.keys.printable.
    manifesthooks off
    for ignored, range in C.keys.printable
    {
        loop % (range[2] - range[1])
        {
            i := range[1] + a_index - 1
            hotkey % "$vk" num_to_hex(i, 2), on_printable_key, %flag%, useerrorlevel
            hotkey % "$+vk" num_to_hex(i, 2), on_printable_key, %flag%, useerrorlevel
            hotkey % "$<^>!vk" num_to_hex(i, 2), on_printable_key, %flag%, useerrorlevel
        }
    }
    manifesthooks on

    return

on_printable_key:
    ; This hotkey must always be high priority
    critical on
    vk := regexreplace(a_thishotkey, ".*vk", "vk")
    has_shift := instr(a_thishotkey, "$+")
    has_altgr := instr(a_thishotkey, "$<^>!")
    varsetcapacity(mods, 256, 0)
    numput(has_shift ? 0x80 : 0x00, mods, 0x10, "uchar")
    numput(has_altgr ? 0x80 : 0x00, mods, 0x11, "uchar")
    numput(has_altgr ? 0x80 : 0x00, mods, 0x12, "uchar")
    err := dllcall("ToAscii", "uint", getkeyvk(vk), "uint", getkeysc(vk), "ptr", &mods, "uintp", ascii, "uint", 0, "uint")
    if (err > 0 && ascii > 0)
    {
        ; If the system was able to translate the key, pass it to the
        ; composition handler.
        send_keystroke(chr(ascii))
    }
    else
    {
        ; If the system doesn't know the key, make an honest attempt at sending
        ; it anyways. Note that I have never seen this happen yet.
        tosend := "{" vk "}"
        if (has_shift)
            tosend := "{shift down}" tosend "{shift up}"
        if (has_altgr)
            tosend := "{sc138 down}" tosend "{sc138 up}"
        send %tosend%
    }
    return
}

;
; Handle special keys that may be used with the compose key in other situations,
; for instance Alt-Tab when Alt is the compose key, or Windows-Left/Right, etc.
;
set_special_hotkeys(must_enable)
{
    flag := must_enable ? "on" : "off"

    manifesthooks off
    for ignored, key in C.keys.special
    {
        hotkey $%key%, on_special_hotkey, %flag%, useerrorlevel
        hotkey $%key% up, on_special_hotkey, %flag%, useerrorlevel
    }
    manifesthooks on

    return

on_special_hotkey:
    ; This hotkey must always be active and high priority
    suspend permit
    critical on
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

    manifesthooks off
    if (must_enable)
    {
        ; Activate the compose key for real
        for ignored, prefix in compose_prefixes
        {
            hotkey % prefix R.compose_key, on_compose_key, on, useerrorlevel
            hotkey % prefix R.compose_key " up", on_compose_key, on, useerrorlevel
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
    manifesthooks on

    return

on_compose_key:
    ; This hotkey must always be active and high priority
    suspend permit
    critical on
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

    ; Insert into our [sequence → character] lookup table
    R.sequences.insert(string_to_hex(seq), [seq, char])
    R.sequences_alt.insert(seq, seq)

    ; Insert into the prefix lookup table
    loop % strlen(seq) - 1
    {
        prefix := substr(seq, 1, a_index)
        R.prefixes.insert(string_to_hex(prefix), true)
        R.prefixes_alt.insert(prefix, prefix)
    }

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
    ret := R.sequences.haskey(string_to_hex(seq))
    ; Try to match case-insensitive, but only if it is not a valid prefix
    if (!ret && R.opt_case && !R.prefixes_alt.haskey(seq))
        ret := R.sequences_alt.haskey(seq)
    return ret
}

get_sequence(seq)
{
    ret := R.sequences[string_to_hex(seq)][2]
    ; Try to match case-insensitive
    if (!ret && R.opt_case)
        ret := R.sequences[string_to_hex(R.sequences_alt[seq])][2]
    return ret
}

get_description(seq)
{
    return R.descriptions[string_to_hex(seq)]
}

has_prefix(seq)
{
    ret := R.prefixes.haskey(string_to_hex(seq))
    ; Try to match case-insensitive
    if (!ret && R.opt_case)
        ret := R.prefixes_alt.haskey(seq)
    return ret
}

