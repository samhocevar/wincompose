
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

; The name and version of this script
global app := "WinCompose"
global version := "0.6.8"
global website := "https://github.com/samhocevar/wincompose"

; Configuration file location -- this needs to exist early, before
; any calls to _() are made. It will be replaced with the proper
; version later.
global R := { config_file: "" }

#include utils.ahk
#include constants.ahk
#include ui.ahk

; Activate debug messages?
;global have_debug := true

; Global runtime variables
global S := { sequence: ""           ; The sequence being typed
            , typing: false          ; Is the user typing something?
            , disabled: false        ; Is everything disabled?
            , compose_down: false    ; Is the compose key down?
            , special_down: false    ; Was at least one special key down?
            , selected_seq: "" }     ; The sequence currently selected

; Runtime configuration, imported from the config files
global R := { sequences:     {}      ; List of valid sequences
            , sequences_alt: {}      ; Alt table for case insensitive
            , seq_count:     0
            , prefixes:      {}      ; List of valid prefixes
            , prefixes_alt:  {}      ; Alt table for case insensitive
            , descriptions:  {}
            , keynames:      {}
            , config_file:   ""      ; Configuration file location
            , compose_key:   C.keys.default
            , reset_delay:   C.delays.default
            , language:      C.languages.default
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

    load_all_settings()
    load_sequences()

    create_gui()

    set_printable_hotkeys(true)
    set_compose_hotkeys(true)
}

;
; Keystroke logic
;

send_keystroke(char)
{
    settimer on_delay_expired, off

    if (S.disabled)
    {
        ; This should not happen, because the DISABLED state should
        ; completely disable the compose key and the other callbacks
        ; are automatically disabled in suspend mode, but I guess it
        ; doesn't hurt to have some fallback solution.
        if (char == "compose")
        {
            set_compose_hotkeys(false) ; Try again to disable these
            sendinput % "{" R.compose_key "}"
        }
        else
            send_raw(char)
        S.sequence := ""
    }
    else if (!S.typing)
    {
        ; Enter typing state if compose was pressed. Otherwise, we
        ; should not be here, but send the character anyway.
        if (char == "compose")
        {
            check_keyboard_layout()
            S.typing := true
            if (R.reset_delay > 0)
                settimer on_delay_expired, % R.reset_delay
        }
        else
            send_raw(char)
        S.sequence := ""
    }
    else ; if (S.typing)
    {
        ; If the compose key is an actual character, don't cancel the compose
        ; sequence since the character could be used in the sequence itself.
        if (char == "compose" && strlen(R.compose_key) == 1)
            char := R.compose_key

        if (char == "compose")
        {
            settimer on_delay_expired, off
            ; Maybe we actually typed a valid sequence; output it
            if (has_sequence(S.sequence))
                send_unicode(get_sequence(S.sequence))
            else if (!R.opt_discard)
                send_raw(S.sequence)
            S.typing := false
            S.sequence := ""
        }
        else
        {
            ; Decide whether we need to (1) keep on building the sequence, or
            ; (2) build it and print it, or (3) print it and output the remaining
            ; character. (0) means invalid sequence.
            if (has_exact_prefix(S.sequence char))
                action := 1
            else if (has_exact_sequence(S.sequence char))
                action := 2
            else if (has_exact_sequence(S.sequence))
                action := 3
            ; ... optionally, retry with a case insensitive match
            else if (R.opt_case && has_prefix(S.sequence char))
                action := 1
            else if (R.opt_case && has_sequence(S.sequence char))
                action := 2
            else if (R.opt_case && has_sequence(S.sequence))
                action := 3
            ; Every attempt failed, abort sequence
            else
                action := 0

            ; Now apply the decision.
            if (action == 0)
            {
                S.sequence .= char
                info := "Sequence: [ " S.sequence " (" string_to_hex(S.sequence) ") ] ABORTED"
                if (!R.opt_discard)
                    send_raw(S.sequence)
                S.typing := false
                S.sequence := ""
                if (R.opt_beep)
                    soundplay *-1
            }
            else if (action == 1)
            {
                S.sequence .= char
                info := "Sequence: [ " S.sequence " ]"
                if (R.reset_delay > 0)
                    settimer on_delay_expired, % R.reset_delay
            }
            else ; action == 2 or action == 3
            {
                if (action == 2)
                    S.sequence .= char

                info := "Sequence: [ " S.sequence " ] -> [ " get_sequence(S.sequence) " ]"
                send_unicode(get_sequence(S.sequence))
                if (action == 3)
                    send_raw(char)
                S.typing := false
                S.sequence := ""
            }

            debug(info)
        }
    }

    refresh_systray()
    return

on_delay_expired:
    settimer on_delay_expired, off
    if (S.typing)
        send_keystroke("compose")
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
    has_capslock := getkeystate("capslock", "T")
    varsetcapacity(mods, 256, 0)
    numput(has_shift ? 0x80 : 0x00, mods, 0x10, "uchar")
    numput(has_altgr ? 0x80 : 0x00, mods, 0x11, "uchar")
    numput(has_altgr ? 0x80 : 0x00, mods, 0x12, "uchar")
    numput(has_capslock ? 0x01 : 0x00, mods, 0x14, "uchar")
    ; Use ToUnicode instead of ToAscii because a lot of languages have non-ASCII chars
    ; available on their keyboards.
    ret := dllcall("ToUnicode", "uint", getkeyvk(vk), "uint", getkeysc(vk)
                 , "ptr", &mods, "uintp", unicode_char, "int", 2, "uint", 0, "uint")
    if (ret > 0 && unicode_char > 0)
    {
        ; If the system was able to translate the key, pass it to the
        ; composition handler. There might be two keys (in the case of
        ; dead keys).
        if (ret >= 2)
            send_keystroke(chr((unicode_char >> 16) & 0xffff))
        send_keystroke(chr(unicode_char & 0xffff))
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
        if (%key% != R.compose_key)
        {
            hotkey $%key%, on_special_hotkey, %flag%, useerrorlevel
            hotkey $%key% up, on_special_hotkey, %flag%, useerrorlevel
        }
    }
    manifesthooks on

    if (!must_enable && S.special_down)
        sendinput % "{" R.compose_key " up}"
    S.special_down := false

    return

on_special_hotkey:
    ; This hotkey must always be active and high priority
    suspend permit
    critical on
    key := regexreplace(a_thishotkey, "[^a-z0-9]*([a-z0-9]*).*", "$1", ret)
    if (instr(a_thishotkey, " up"))
    {
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
            for key, ignored in C.keys.valid
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
        ; In case the compose key is a modifier (see below), tell the system it was
        ; released, just in case a special hotkey was triggered.
        set_special_hotkeys(false)
    }
    else if (!S.compose_down)
    {
        ; Compose was pressed down -- protect against autorepeat
        S.compose_down := true
        send_keystroke("compose")
        ; In case the compose key is a modifier (e.g. Alt or Ctrl) we bind some
        ; additional special hotkeys to allow shortcuts such as Alt-F4.
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
    r_right := "^[^"":#]*:[^""#]*""((\\\\.|[^\\""])*)"".*$"

    ; Regex to match a key sequence between brackets and before a colon,
    ; such as:
    ;  <key> <other_key><j> <more_keys>  : ... any stuff ...
    r_left := "^[ \\t]*(([ \\t]*<[^>]*>)*)([^:]*):.*$"

    loop read, %file%
    {
        ; Retrieve destination character
        right := regexreplace(a_loopreadline, r_right, "$1", ret)
        if (ret != 1)
            continue
        right := regexreplace(right, "\\\\(.)", "$1")

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
    r_right := "^[^"":#]*:[^""#]*""((\\\\.|[^\\""])*)"".*$"
    r_left := "^[ \\t]*(([ \\t]*<[^>]*>)*)([^:]*):.*$"

    loop read, %file%
    {
        ; Retrieve destination character(s) -- could be an UTF-16 sequence
        right := regexreplace(a_loopreadline, r_right, "$1", ret)
        if (ret < 1)
            continue
        right := regexreplace(right, "\\\\(.)", "$1")

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
    if (!has_exact_sequence(seq))
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
        str := v[2]
        desc := R.descriptions[k]

        ; Filter out if necessary
        if (filter != str && !instr(seq, filter) && !instr(desc, filter_low))
            continue

        ; Now insert into the GUI
        if (strlen(str) == 1)
        {
            digits := 4
            uni := "U+" num_to_hex(asc(str), digits)
        }
        else if ((strlen(str) == 2) && ((asc(substr(str, 1, 1)) & 0xf800) == 0xd800))
        {
            code := (asc(substr(str, 1, 1)) - 0xd800) << 10
            code += asc(substr(str, 2, 1)) + 0x10000 - 0xdc00
            digits := 6
            ; HACK: prepend a non-printable character to fix sorting
            uni := chr(0x2063) "U+" num_to_hex(code, digits)
        }
        else
        {
            if (strlen(str) > 3)
                str := substr(str, 1, 2) "…"
            uni := chr(0x2063) chr(0x2063)
        }

        lv_add("", humanize_sequence(seq), str, uni)
    }
}

has_exact_prefix(seq)
{
    return R.prefixes.haskey(string_to_hex(seq))
}

has_prefix(seq)
{
    return R.prefixes_alt.haskey(seq)
}

has_exact_sequence(seq)
{
    return R.sequences.haskey(string_to_hex(seq))
}

has_sequence(seq)
{
    return R.sequences_alt.haskey(seq)
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

