
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
global version := "0.4.6"

; Configuration directory and file
global config_dir := a_appdata . "\\" . app
global config_file := config_dir . "\\settings.ini"

; Activate debug messages?
global have_debug := false

; Global runtime variables
global state := { mode: "WAITING"        ; Global state, one of WAITING, TYPING, or DISABLED
                , compose_down: false    ; Is the compose key down?
                , special_down: false }  ; Is a special key down?

; Runtime configuration, taken from the config files
global config := { sequences:   {}
                 , prefixes:    {}
                 , compose_key: default_key
                 , reset_delay: default_delay }

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
    menu tray, icon, %resource_file%, 1

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
    iniread, tmp, %config_file%, Global, % config.compose_key, ""
    config.compose_key := valid_keys.haskey(tmp) ? tmp : default_key

    ; Read the reset delay value and sanitise it if necessary
    iniread, tmp, %config_file%, Global, % config.reset_delay, ""
    config.reset_delay := valid_delays.haskey(tmp) ? tmp : default_delay

    save_settings()
}

save_settings()
{
    filecreatedir, %config_dir%
    iniwrite, % config.compose_key, %config_file%, Global, compose_key
    iniwrite, % config.reset_delay, %config_file%, Global, reset_delay
}

;
; Handle i18n
;

_(str, args*)
{
    static t

    if (!t)
    {
        t := {}

        regread, locale, HKEY_CURRENT_USER, Control Panel\\International, localename
        files := [ "default", substr(locale, 1, 2), regexreplace(locale, "-", "_") ]

        FileEncoding UTF-8

        for ignored, file in files
        {
            section := ""
            loop read, % "locale/" file ".ini"
            {
                regex := "^[ \\t]*\\[([^ \\t\\]]*)[^ \\t\\]]*\\].*"
                newsection := regexreplace(a_loopreadline, regex, "$1", ret)
                if (ret == 1)
                {
                    section := newsection
                    continue
                }

                regex := "^[ \\t]*([^ \\t=]*)[ \\t]*=[ \\t]*(""(.*)""|(.*[^ ]))[ \\t]*$"
                key := regexreplace(a_loopreadline, regex, "$1", ret)
                val := regexreplace(a_loopreadline, regex, "$3$4", ret2)
                if (ret == 1 && ret2 == 1)
                {
                    t.insert(section "." key, val)
                    continue
                }
            }
        }
    }

    ret := t.haskey(str) ? t[str] : "[" str "]"

    ret := regexreplace(ret, "@APP_NAME@", app)
    ret := regexreplace(ret, "@APP_VERSION@", version)
    ret := regexreplace(ret, "@AHK_VERSION@", a_ahkversion)

    for index, arg in args
        ret := regexreplace(ret, "@" index "@", arg)

    return ret
}

;
; Utility functions
;

send_keystroke(keystroke)
{
    static sequence := ""
    settimer, reset_callback, off

    if (state.mode == "DISABLED")
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
    else if (state.mode == "WAITING")
    {
        ; Enter typing state if compose was pressed; otherwise we
        ; should not be here but send the character anyway.
        if (keystroke == "compose")
        {
            check_keyboard_layout()
            state.mode := "TYPING"
            if (config.reset_delay > 0)
                settimer, reset_callback, % config.reset_delay
        }
        else
            send_raw(char)
        sequence := ""
    }
    else if (state.mode == "TYPING")
    {
        ; If the compose key is an actual character, don't cancel the compose
        ; sequence since the character could be used in the sequence itself.
        if (keystroke == "compose" && strlen(config.compose_key) == 1)
            keystroke := config.compose_key

        if (keystroke == "compose")
        {
            settimer, reset_callback, off
            sequence := ""
            state.mode := "WAITING"
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
                state.mode := "WAITING"
                sequence := ""
            }
            else if (!has_prefix(sequence))
            {
                info .= " ABORTED"
                send_raw(sequence)
                state.mode := "WAITING"
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
    if (state.mode == "TYPING")
        state.mode := "WAITING"
    refresh_systray()
    return

toggle_callback:
    if (state.mode == "DISABLED")
    {
        state.mode := "WAITING"
        set_compose_hotkeys(true)
    }
    else
    {
        set_compose_hotkeys(false)
        state.mode := "DISABLED"
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
    menu tray, add, % _("menu.exit"), exit_callback
    menu tray, default, % _("menu.sequences")

    ; Build the sequence list window
    global ui_listbox, ui_text, ui_textw, ui_textedit, ui_button
    gui +resize +minsize300x115
    gui margin, 8, 8
    gui font, s11
    gui font, s11, Courier New
    gui font, s11, Lucida Console
    gui font, s11, Consolas
    gui add, listview, vui_listbox w700 r18, % _("seq_win.columns")
    gui font
    gui add, text, vui_text, % _("seq_win.filter")
    guicontrolget ui_text, pos
    gui add, edit, vui_textedit gedit_callback
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

exit_callback:
    save_settings()
    exitapp
    return

guisize:
    if (a_eventinfo != 1) ; Ignore minimising
    {
        w := a_guiwidth
        h := a_guiheight
        global ui_listbox, ui_text, ui_textedit, ui_textw, ui_button
        guicontrol move, ui_listbox, % "w" (w - 16) " h" (h - 45)
        guicontrol move, ui_text, % "y" (h - 26)
        guicontrol move, ui_textedit, % "x" (ui_textw + 15) " w" (w - 140 - ui_textw) " y" (h - 30)
        guicontrol move, ui_button, % "x" (w - 87) " y" (h - 30) " w80"
    }
    return

guicontextmenu:
    if (a_guicontrol == "ui_listbox")
    {
        if (a_eventinfo > 0)
        {
            global ui_selected_char
            lv_gettext(ui_selected_char, a_eventinfo, 2)
            menu, contextmenu, show
        }
    }
    return

copychar_callback:
    global ui_selected_char
    clipboard := ui_selected_char
    return

edit_callback:
    critical on ; don't self-interrupt or we will corrupt the listview
    refresh_gui()
    return

showgui_callback:
    gui_title := _("seq_win.title")
    if (winexist(gui_title))
        goto hidegui_callback
    refresh_gui()
    gui show, , %gui_title%
    global ui_textedit
    guicontrol focus, ui_textedit
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
    if (state.mode == "WAITING")
    {
        ; Disable hotkeys; we only want them on during a compose sequence
        suspend on
        menu tray, uncheck, % _("menu.disable")
        menu tray, icon, %resource_file%, 1, 1
        menu tray, tip, % _("tray_tip.active")
    }
    else if (state.mode == "TYPING")
    {
        suspend off
        menu tray, uncheck, % _("menu.disable")
        menu tray, icon, %resource_file%, 2
        menu tray, tip, % _("tray_tip.typing")
    }
    else if (state.mode == "DISABLED")
    {
        suspend on
        menu tray, check, % _("menu.disable")
        menu tray, icon, %resource_file%, 3, 1
        menu tray, tip, % _("tray_tip.disabled")
    }

    for key, val in valid_keys
        menu, hotkeymenu, % (key == config.compose_key) ? "check" : "uncheck", %val%

    for key, val in valid_delays
        menu, delaymenu, % (key == config.reset_delay) ? "check" : "uncheck", %val%
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
    special_key := regexreplace(a_thishotkey, "[^a-z0-9]*([a-z0-9]*).*", "$1", ret)
    if (instr(a_thishotkey, " up"))
    {
        state.special_down := false
        sendinput {%special_key% up}
    }
    else
    {
        ; Cancel any sequence in progress
        if (state.mode == "TYPING")
            send_keystroke("compose")
        
        if (!state.special_down)
            sendinput % "{" config.compose_key " down}"

        sendinput {%special_key% down}
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

refresh_gui()
{
    lv_delete()
    global ui_textedit
    guicontrolget ui_textedit
    fill_sequences(ui_textedit)
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

    info(_("tray_notify.loaded", count) "\n" _("tray_notify.keyinfo", valid_keys[config.compose_key]) "\n")
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
    ; Insert into our lookup table
    stringlower desc, desc
    config.sequences.insert(string_to_hex(key), [key, val, desc])

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
        sequence := "♦" . regexreplace(key, "(.)", " $1")
        sequence := regexreplace(sequence, "  ", " {spc}")
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

