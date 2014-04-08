
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

#include constants.ahk
#include utils.ahk

; The name and version of this script
global app := "WinCompose"
global version := "0.5.0"
global website := "https://github.com/SamHocevar/wincompose"

; Configuration directory and file
global config_dir := a_appdata . "\\" . app
global config_file := config_dir . "\\settings.ini"

; Activate debug messages?
global have_debug := false

; Global runtime variables
global state := { typing: false          ; Is the user typing something?
                , disabled: false        ; Is everything disabled?
                , compose_down: false    ; Is the compose key down?
                , special_down: false    ; Is a special key down?
                , selected_char: ""      ; The character currently selected in GUI
                , selected_seq: ""       ; The sequence currently selected
                , gui_width: 0
                , gui_height: 0 }

; Runtime configuration, taken from the config files
global config := { sequences:   {}
                 , seq_count:   0
                 , prefixes:    {}
                 , keynames:    {}
                 , compose_key: default_key
                 , reset_delay: default_delay }

; GUI variables
global ui_listbox, ui_edit_filter, ui_button
global ui_text_filter, ui_text_filterw, ui_text_bigchar, ui_text_desc
global ui_keycap_0
global ui_keycap_1, ui_keycap_2, ui_keycap_3, ui_keycap_4, ui_keycap_5, ui_keycap_6, ui_keycap_7, ui_keycap_8, ui_keycap_9
global ui_keytext_1, ui_keytext_2, ui_keytext_3, ui_keytext_4, ui_keytext_5, ui_keytext_6, ui_keytext_7, ui_keytext_8, ui_keytext_9

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
    menu tray, icon, %global_resource_file%, 1

    load_settings()
    load_sequences()
    setup_ui()
}

;
; Handle Settings
;

load_settings()
{
    ; Read the compose key value and sanitise it if necessary
    iniread tmp, %config_file%, Global, % config.compose_key, ""
    config.compose_key := valid_keys.haskey(tmp) ? tmp : default_key

    ; Read the reset delay value and sanitise it if necessary
    iniread tmp, %config_file%, Global, % config.reset_delay, ""
    config.reset_delay := valid_delays.haskey(tmp) ? tmp : default_delay

    save_settings()
}

save_settings()
{
    filecreatedir %config_dir%
    iniwrite % config.compose_key, %config_file%, Global, compose_key
    iniwrite % config.reset_delay, %config_file%, Global, reset_delay
}

;
; Keystroke logic
;

send_keystroke(keystroke)
{
    static sequence := ""
    settimer, reset_callback, off

    if (state.disabled)
    {
        ; This should not happen, because the DISABLED state should
        ; completely disable the compose key and the other callbacks
        ; are automatically disabled in suspend mode, but I guess it
        ; doesn't hurt to have some fallback solution.
        if (keystroke == "compose")
            send % config.compose_key
        else
            send_raw(char)
        sequence := ""
    }
    else if (!state.typing)
    {
        ; Enter typing state if compose was pressed; otherwise we
        ; should not be here but send the character anyway.
        if (keystroke == "compose")
        {
            check_keyboard_layout()
            state.typing := true
            if (config.reset_delay > 0)
                settimer, reset_callback, % config.reset_delay
        }
        else
            send_raw(char)
        sequence := ""
    }
    else ; if (state.typing)
    {
        ; If the compose key is an actual character, don't cancel the compose
        ; sequence since the character could be used in the sequence itself.
        if (keystroke == "compose" && strlen(config.compose_key) == 1)
            keystroke := config.compose_key

        if (keystroke == "compose")
        {
            settimer, reset_callback, off
            sequence := ""
            state.typing := false
        }
        else
        {
            ; If this is a numpad key, replace it with its ASCII value
            if (num_keys.haskey(keystroke))
                keystroke := "$" num_keys[keystroke]

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
                state.typing := false
                sequence := ""
            }
            else if (!has_prefix(sequence))
            {
                info .= " ABORTED"
                send_raw(sequence)
                state.typing := false
                sequence := ""
            }
            else
            {
                if (config.reset_delay > 0)
                    settimer, reset_callback, % config.reset_delay
            }

            debug(info)
        }
    }

    refresh_systray()
    return

reset_callback:
    settimer, reset_callback, off
    sequence := ""
    state.typing := false
    refresh_systray()
    return

toggle_callback:
    if (state.disabled)
    {
        state.disabled := false
        state.typing := false
        set_compose_hotkeys(true)
    }
    else
    {
        set_compose_hotkeys(false)
        state.disabled := true
    }
    refresh_systray()
    return
}

send_unicode(char)
{
    ; HACK: GTK+ applications behave differently with Unicode, and some applications
    ; such as XChat for Windows rename their own top-level window
    for ignored, class in gdk_classes
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

info(string)
{
    traytip, %app%, %string%, 10, 1
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
        menu, hotkeymenu, add, %val%, hotkeymenu_callback

    ; The delay selection menu
    for key, val in valid_delays
        menu, delaymenu, add, %val%, delaymenu_callback

    ; Build the systray menu
    menu tray, click, 1
    menu tray, NoStandard
    menu tray, add, % _("menu.sequences"), showgui_callback
    menu tray, add, % _("menu.composekey"), :hotkeymenu
    menu tray, add, % _("menu.timeout"), :delaymenu
    menu tray, add, % _("menu.disable"), toggle_callback
    menu tray, add, % _("menu.restart"), restart_callback
    menu tray, add
    if (have_debug)
    {
        menu tray, add, % _("menu.history"), history_callback
        menu tray, add, % _("menu.hotkeylist"), hotkeylist_callback
    }
    menu tray, add, % _("menu.about"), about_callback
    menu tray, add, % _("menu.website"), website_callback
    menu tray, add, % _("menu.exit"), exit_callback
    menu tray, default, % _("menu.sequences")

    ; Build the sequence list window
    gui +resize +minsize720x400
    gui margin, 8, 8

    gui font, s11
    gui font, s11, Courier New
    gui font, s11, Lucida Console
    gui font, s11, Consolas
    gui add, listview, vui_listbox glistview_callback w300 r5 altsubmit -multi, % _("seq_win.columns")

    gui font, s100
    gui add, text, vui_text_bigchar center +E0x200, % ""

    gui font, s11
    gui add, text, vui_text_desc backgroundtrans, % ""

    gui add, picture, w48 h48 vui_keycap_0 icon2, %global_resource_file%

    gui font, s22
    gui font, w700
    loop % 9
    {
        gui add, picture, x0 y0 w48 h48 vui_keycap_%a_index% icon4, %global_resource_file%
        gui add, text, x0 y0 w48 h48 center vui_keytext_%a_index% backgroundtrans, % ""
        guicontrol hide, ui_keycap_%a_index%
        guicontrol hide, ui_keytext_%a_index%
    }

    gui font
    gui add, text, vui_text_filter, % _("seq_win.filter")
    guicontrolget ui_text_filter, pos

    gui add, edit, vui_edit_filter gedit_callback

    gui add, button, vui_button default, % _("seq_win.close")

    ; The copy character menu
    menu, contextmenu, add, % _("contextmenu.copy"), copychar_callback

    set_ascii_hotkeys(true)
    set_compose_hotkeys(true)

    refresh_systray()

    return

key_callback:
    send_keystroke(a_thishotkey)
    return

hotkeymenu_callback:
    set_compose_hotkeys(false)
    for key, val in valid_keys
        if (val == a_thismenuitem)
            config.compose_key := key
    refresh_systray()
    set_compose_hotkeys(true)
    return

delaymenu_callback:
    for key, val in valid_delays
        if (val == a_thismenuitem)
            config.reset_delay := key
    refresh_systray()
    return

restart_callback:
    save_settings()
    reload
    return

history_callback:
    keyhistory
    return

hotkeylist_callback:
    listhotkeys
    return

about_callback:
    about_text := _("about_win.line1") . "\n"
    about_text .= _("about_win.line2") . "\n"
    about_text .= _("about_win.line3") . "\n"
    about_text .= _("about_win.line4") . "\n"
    msgbox 64, %app%, %about_text%
    return

website_callback:
    run %website%
    return

exit_callback:
    save_settings()
    exitapp
    return

guisize:
    if (a_eventinfo != 1) ; Ignore minimising
    {
        state.gui_width := a_guiwidth
        state.gui_height := a_guiheight
        refresh_gui()
    }
    return

guicontextmenu:
    if (a_guicontrol == "ui_listbox")
    {
        if (a_eventinfo > 0)
        {
            lv_gettext(tmp, a_eventinfo, 2)
            state.selected_char := tmp
            menu, contextmenu, show
        }
    }
    return

copychar_callback:
    clipboard := state.selected_char
    return

edit_callback:
    critical on ; don't self-interrupt or we will corrupt the listview
    recompute_gui_filter()
    return

listview_callback:
    critical on
    if (a_guievent == "I")
    {
        ; If a new line was selected, update all the information
        if (instr(errorlevel, "S", true))
        {
            lv_gettext(tmp, a_eventinfo, 2)
            if (tmp != state.selected_char)
            {
                state.selected_char := tmp
                guicontrol text, ui_text_bigchar, %tmp%

                lv_gettext(tmp2, a_eventinfo, 3)
                lv_gettext(tmp3, a_eventinfo, 4)
                guicontrol text, ui_text_desc, % tmp2 " " tmp "\n\n" tmp3

                lv_gettext(tmp4, a_eventinfo, 1)
                state.selected_seq := tmp4
            }
            refresh_gui()
        }
    }
    return

showgui_callback:
    critical on
    gui_title := _("seq_win.title")
    if (winexist(gui_title))
        goto hidegui_callback
    recompute_gui_filter()
    gui show, , %gui_title%
    guicontrol focus, ui_edit_filter
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
    if (state.disabled)
    {
        suspend on
        menu tray, check, % _("menu.disable")
        menu tray, icon, %global_resource_file%, 3, 1
        menu tray, tip, % _("tray_tip.disabled")
    }
    else if (!state.typing)
    {
        ; Disable hotkeys; we only want them on during a compose sequence
        suspend on
        menu tray, uncheck, % _("menu.disable")
        menu tray, icon, %global_resource_file%, 1, 1
        menu tray, tip, % _("tray_tip.active")
    }
    else ; if (state.typing)
    {
        suspend off
        menu tray, uncheck, % _("menu.disable")
        menu tray, icon, %global_resource_file%, 2
        menu tray, tip, % _("tray_tip.typing")
    }

    for key, val in valid_keys
        menu, hotkeymenu, % (key == config.compose_key) ? "check" : "uncheck", %val%

    for key, val in valid_delays
        menu, delaymenu, % (key == config.reset_delay) ? "check" : "uncheck", %val%
}

refresh_gui()
{
    w := state.gui_width
    h := state.gui_height
    listbox_w := 260
    listbox_h := h - 45
    bigchar_w := 180
    bigchar_h := 180
    guicontrol move, ui_listbox, % "w" listbox_w " h" listbox_h
    guicontrol move, ui_text_desc, % "x" listbox_w + 32 " y" 16 " w" w - listbox_w - 40 " h" 120
    guicontrol move, ui_text_bigchar, % "x" listbox_w + (w - listbox_w - bigchar_w) / 2 " y" (h - bigchar_h - 45) " w" bigchar_w " h" bigchar_h
    guicontrol move, ui_text_filter, % "y" (h - 26)
    guicontrol move, ui_edit_filter, % "x" (ui_text_filterw + 15) " w" (w - 140 - ui_text_filterw) " y" (h - 30)
    guicontrol move, ui_button, % "x" (w - 87) " y" (h - 30) " w80"

    loop % 9
    {
        guicontrol hide, ui_keycap_%a_index%
        guicontrol hide, ui_keytext_%a_index%
    }

    guicontrol move, ui_keycap_0, % "x280 y100"

    tmp := state.selected_seq
    loop parse, tmp, % " "
    {
        guicontrol show, ui_keycap_%a_index%
        guicontrol move, ui_keycap_%a_index%, % "x" 280 + a_index * 52 " y" 100

        guicontrol text, ui_keytext_%a_index%, % a_loopfield == "space" ? ""
                                          : a_loopfield == "&" ? "&&"
                                          : a_loopfield
        guicontrol show, ui_keytext_%a_index%
        guicontrol move, ui_keytext_%a_index%, % "x" 280 + a_index * 52 " y" 106
    }
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
    for key, val in num_keys
        hotkey %key%, key_callback, %flag%, useerrorlevel
}

;
; Handle special keys that may be used with the compose key in other situations,
; for instance Alt-Tab when Alt is the compose key, or Windows-Left/Right, etc.
;
set_special_hotkeys(must_enable)
{
    flag := must_enable ? "on" : "off"

    for ignored, key in special_keys
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
        state.special_down := false
        sendinput {%key% up}
    }
    else
    {
        ; Cancel any sequence in progress
        if (state.typing)
            send_keystroke("compose")

        if (!state.special_down)
            sendinput % "{" config.compose_key " down}"

        sendinput {%key% down}
        state.special_down := true
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
        for key, val in valid_keys
            if (strlen(key) == 1)
                hotkey $%key%, key_callback, on, useerrorlevel

        ; Activate the compose key for real
        for ignored, prefix in compose_prefixes
        {
            hotkey % prefix config.compose_key, compose_callback, on, useerrorlevel
            hotkey % prefix config.compose_key " up", compose_callback, on, useerrorlevel
        }
    }
    else
    {
        ; Disable any existing hotkeys
        for ignored, prefix in compose_prefixes
        {
            for key, val in valid_keys
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
        state.compose_down := false
        ; Tell the system it was released, just in case a special
        ; hotkey was triggered.
        sendinput % "{" config.compose_key " up}"
        set_special_hotkeys(false)
    }
    else if (!state.compose_down)
    {
        ; Compose was pressed down -- protect against autorepeat
        state.compose_down := true
        send_keystroke("compose")
        set_special_hotkeys(true)
    }
    return
}

recompute_gui_filter()
{
    lv_delete()
    guicontrolget ui_edit_filter
    fill_sequences(ui_edit_filter)

    loop % 3
        lv_modifycol(a_index, "autohdr")
    lv_modifycol(1, "center") ; center the sequences column
    lv_modifycol(1, "sort")   ; sort the sequences column
    lv_modifycol(2, "center") ; center the character column
    lv_modifycol(4, "0")      ; hide the description column

    lv_modify(1, "select")
}

load_sequences()
{
    ; Read the default key file
    read_key_file(global_key_file)

    ; Read the default sequence file
    read_sequence_file(global_sequence_file)

    ; Read a user-provided sequence file, if available
    envget userprofile, userprofile
    read_sequence_file(userprofile "\\.XCompose")
    read_sequence_file(userprofile "\\.XCompose.txt")

    info(_("tray_notify.loaded", config.seq_count) "\n" _("tray_notify.keyinfo", valid_keys[config.compose_key]) "\n")
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

        config.keynames[regexreplace(left, "[<>]*", "")] := right
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
            else if (config.keynames.haskey(a_loopfield))
                seq .= config.keynames[a_loopfield]
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
add_sequence(key, val, desc)
{
    ; Insert into our lookup table
    stringlower desc, desc
    config.sequences.insert(string_to_hex(key), [key, val, desc])
    config.seq_count += 1

    ; Insert into the prefix lookup table
    loop % strlen(key) - 1
        config.prefixes.insert(string_to_hex(substr(key, 1, a_index)), true)
}

; Fill the default list view widget with all the compose rules that
; contain the string "filter", either in the compose sequence or in
; the description of the Unicode character.
fill_sequences(filter)
{
    stringlower filter_low, filter
    for k, v in config.sequences
    {
        key := v[1]
        val := v[2]
        desc := v[3]

        ; Filter out if necessary
        if (filter != val && !instr(key, filter) && !instr(desc, filter_low))
            continue

        ; Insert into the GUI if applicable
        sequence := regexreplace(key, "(.)", " $1")
        sequence := regexreplace(sequence, "  ", " space")
        sequence := regexreplace(sequence, "^ ", "")
        result := val
        uni := "U+"

        if (strlen(val) == 1)
        {
            code := asc(val)
            digits := 4
        }
        else if (strlen(val) == 2)
        {
            code := (asc(substr(val, 1, 1)) - 0xd800) << 10
            code += asc(substr(val, 2, 1)) + 0x10000 - 0xdc00
            digits := 6
            ; HACK: prepend a non-printable character to fix sorting
            uni := chr(0x2063) . uni
        }

        uni .= num_to_hex(code, digits)

        lv_add("", sequence, val, uni, desc)
    }
}

has_sequence(key)
{
    return config.sequences.haskey(string_to_hex(key))
}

get_sequence(key)
{
    return config.sequences[string_to_hex(key)][2]
}

has_prefix(key)
{
    return config.prefixes.haskey(string_to_hex(key))
}

