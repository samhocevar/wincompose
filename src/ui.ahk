
;
; Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.


; UI-related constants
global UI := { _:_
    ; Sequence window
  , app_win : { _:_
      , width: 0
      , height: 0
      , margin: 8
        ; The listview
      , listview : { _:_
          , width: 260 }
        ; The big char
      , bigchar : { _:_
          , width: 180
          , height: 180 } } }

; Global GUI variables
global ui_tab
global ui_listbox, ui_edit_filter, ui_button
global ui_text_filter, ui_text_filterw, ui_text_bigchar, ui_text_desc
global ui_text_composekey, ui_dropdown_composekey
global ui_text_delay, ui_dropdown_delay
global ui_text_language, ui_dropdown_language
global ui_separator1, ui_separator2
global ui_checkbox_case, ui_checkbox_discard, ui_checkbox_beep
global ui_keycap_0
global ui_keycap_1, ui_keycap_2, ui_keycap_3, ui_keycap_4, ui_keycap_5, ui_keycap_6, ui_keycap_7, ui_keycap_8, ui_keycap_9
global ui_keytext_1, ui_keytext_2, ui_keytext_3, ui_keytext_4, ui_keytext_5, ui_keytext_6, ui_keytext_7, ui_keytext_8, ui_keytext_9


create_gui()
{
    onexit on_exit

    create_systray()
    create_app_win()

    refresh_systray()
}

create_systray()
{
    ; Build the systray menu
    menu tray, click, 1
    menu tray, NoStandard
    menu tray, add, % _("Show &Sequences…"), on_show_sequences
    menu tray, add, % _("Disable"), on_disable
    menu tray, add, % _("Restart"), on_restart
    menu tray, add
    if (have_debug)
    {
        menu tray, add, % _("&History"), on_show_history
        menu tray, add, % _("Hotkey &List"), on_show_hotkeys
        menu tray, add
    }
    menu tray, add, % _("&Options…"), on_show_options
    menu tray, add, % _("&About"), on_show_about
    menu tray, add, % _("&Visit Website"), on_show_website
    menu tray, add, % _("E&xit"), on_exit
    menu tray, default, % _("Show &Sequences…")

    return

on_show_sequences:
on_show_options:
    critical on
    gui_title := _("@APP_NAME@")
    if (winexist(gui_title " ahk_class AutoHotkeyGUI"))
        goto on_hide_app_win

    is_sequences := instr(a_thislabel, "sequences", true)
    is_settings := instr(a_thislabel, "options", true)

    if (is_sequences)
        guicontrol choose, ui_tab, 1
    else if (is_settings)
        guicontrol choose, ui_tab, 2

    recompute_gui_filter()
    gui show, , %gui_title%

    if (is_settings)
        guicontrol focus, ui_edit_filter

    return

on_hide_app_win:
    hide_app_win()
    return

on_restart:
    save_all_settings()
    reload
    return

on_disable:
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
        S.typing := false
    }
    refresh_systray()
    return

on_show_history:
    keyhistory
    return

on_show_hotkeys:
    listhotkeys
    return

on_show_about:
    about_text := _("@APP_NAME@ v@APP_VERSION@\n")
    about_text .= _("\n")
    about_text .= _("by Sam Hocevar <sam@hocevar.net>\n")
    about_text .= _("running on AHK v@AHK_VERSION@\n")
    msgbox 64, %app%, %about_text%
    return

on_show_website:
    run %website%
    return

on_exit:
    save_all_settings()
    exitapp
    return
}

create_app_win()
{
    ; Build the main window
    gui +resize +minsize720x450 +labelon_app_win_
    gui margin, 8, 8

    gui add, tab2, vui_tab, % _("Sequences") "|" _("Options")

    ; Build the sequence list pane
    gui tab, 1

    gui font, s11
    gui font, s11, Courier New
    gui font, s11, Lucida Console
    gui font, s11, Consolas
    columns := _("Sequence") "|" _("Char") "|" _("Unicode")
    gui add, listview, % "vui_listbox gon_select_sequence w" UI.app_win.listview.width " r5 altsubmit -multi", % columns

    gui font, s100
    gui add, text, vui_text_bigchar center +E0x200, % ""

    gui font, s11
    gui add, text, vui_text_desc backgroundtrans, % ""

    tmp := C.files.resources
    gui add, picture, w48 h48 vui_keycap_0 icon2, %tmp%

    gui font, s22
    gui font, w700
    loop % 9
    {
        tmp := C.files.resources
        gui add, picture, x0 y0 w48 h48 vui_keycap_%a_index% icon4, %tmp%
        gui add, text, x0 y0 w48 h48 center vui_keytext_%a_index% backgroundtrans, % ""
        guicontrol hide, ui_keycap_%a_index%
        guicontrol hide, ui_keytext_%a_index%
    }

    gui font
    gui add, text, vui_text_filter, % _("Search Filter:")
    guicontrolget ui_text_filter, pos

    gui add, edit, vui_edit_filter gon_set_filter

    ; Build the settings pane
    gui tab, 2

    keylist := ""
    for key, val in C.keys.valid
    {
        keylist .= keylist ? "|" val : val
        if (key == R.compose_key)
            keylist .= "||"
    }
    keylist := regexreplace(keylist, "\\|\\|\\|", "||")
    gui add, text, vui_text_composekey, % _("Compose Key:")
    gui add, dropdownlist, vui_dropdown_composekey gon_set_compose, %keylist%

    delaylist := ""
    for key, val in C.delays.valid
    {
        delaylist .= delaylist ? "|" val : val
        if (key == R.reset_delay)
            delaylist .= "||"
    }
    delaylist := regexreplace(delaylist, "\\|\\|\\|", "||")
    gui add, text, vui_text_delay, % _("Delay:")
    gui add, dropdownlist, vui_dropdown_delay gon_set_delay, %delaylist%

    gui add, text, vui_separator1 0x10

    gui add, checkbox, vui_checkbox_case gon_toggle_case, % _("Fall back to case insensitive matches on invalid sequences")
    guicontrol ,, ui_checkbox_case, % R.opt_case ? 1 : 0

    gui add, checkbox, vui_checkbox_discard gon_toggle_discard, % _("Discard characters from invalid sequences")
    guicontrol ,, ui_checkbox_discard, % R.opt_discard ? 1 : 0

    gui add, checkbox, vui_checkbox_beep gon_toggle_beep, % _("Beep on invalid sequences")
    guicontrol ,, ui_checkbox_beep, % R.opt_beep ? 1 : 0

    gui add, text, vui_separator2 0x10

    langlist := ""
    for key, val in C.languages.valid
    {
        langlist .= langlist ? "|" val : val
        if (key == R.language)
            langlist .= "||"
    }
    langlist := regexreplace(langlist, "\\|\\|\\|", "||")
    gui add, text, vui_text_language, % _("Interface language:")
    gui add, dropdownlist, vui_dropdown_language gon_set_language, %langlist%

    ; Build the rest of the window
    gui tab

    gui add, button, vui_button gon_click_close default, % _("Close")

    ; The copy character menu
    menu contextmenu, add, % _("Copy Character"), on_copy_char

    return

on_app_win_size:
    if (a_eventinfo != 1) ; Ignore minimising
    {
        UI.app_win.width := a_guiwidth
        UI.app_win.height := a_guiheight
        refresh_gui()
    }
    return

on_app_win_contextmenu:
    if (a_guicontrol == "ui_listbox")
    {
        if (a_eventinfo > 0)
        {
            lv_gettext(tmp, a_eventinfo, 1)
            S.selected_seq := dehumanize_sequence(tmp)
        }
    }
    menu contextmenu, show
    return

on_copy_char:
    clipboard := get_sequence(S.selected_seq)
    return

on_set_filter:
    critical on ; don't self-interrupt or we will corrupt the listview
    recompute_gui_filter()
    return

on_set_compose:
    guicontrolget tmp, , ui_dropdown_composekey
    set_compose_hotkeys(false)
    for key, val in C.keys.valid
        if (val == tmp)
            R.compose_key := key
    save_all_settings()
    refresh_systray()
    set_compose_hotkeys(true)
    return

on_set_delay:
    guicontrolget tmp, , ui_dropdown_delay
    for key, val in C.delays.valid
        if (val == tmp)
            R.reset_delay := key
    save_all_settings()
    refresh_systray()
    return

on_toggle_case:
on_toggle_discard:
on_toggle_beep:
    guicontrolget val, , % regexreplace(a_thislabel, "on_toggle", "ui_checkbox")
    R[regexreplace(a_thislabel, "on_toggle_", "opt_")] := val
    save_all_settings()
    return

on_set_language:
    guicontrolget tmp, , ui_dropdown_language
    old_language := R.language
    for key, val in C.languages.valid
        if (val == tmp)
            R.language := key
    save_all_settings()
    if (old_language != R.language)
        reload
    return

on_select_sequence:
    critical on
    if (a_guievent == "I")
    {
        ; If a new line was selected, update all the information
        if (instr(errorlevel, "S", true))
        {
            lv_gettext(sequence, a_eventinfo, 1)
            sequence := dehumanize_sequence(sequence)

            if (sequence != S.selected_seq)
            {
                S.selected_seq := sequence
                str := get_sequence(S.selected_seq)

                lv_gettext(unicode, a_eventinfo, 3)

                guicontrol text, ui_text_bigchar, %str%
                ; HACK: remove the non-printable character we added for sorting purposes
                desc := " " regexreplace(unicode, ".*U", "U") " " str "\n"
                desc .= substr("————————————————————", 1, strlen(desc) + 2) "\n"
                desc .= _("Description:") " " get_description(sequence)
                guicontrol text, ui_text_desc, %desc%
            }
            refresh_gui()
        }
    }
    return

on_click_close:
on_app_win_close:
on_app_win_escape:
    hide_app_win()
    return
}

hide_app_win()
{
    gui hide
}

refresh_gui()
{
    w := UI.app_win.width
    h := UI.app_win.height
    m := UI.app_win.margin

    ; Main window top containers
    t_w := w - 2 * m
    t_h := h - 30 - 2 * m
    guicontrol move, ui_tab, % "w" t_w " h" t_h

    guicontrol move, ui_button, % "x" (w - 80 - m) " y" (h - 30) " w80"

    ; First tab (settings)
    guicontrol move, ui_text_composekey, % "x" 3 * m " y" (40 + m)
    guicontrol move, ui_dropdown_composekey, % "x" (150) " y" (40 - 4 + m)

    guicontrol move, ui_text_delay, % "x" 3 * m " y" (40 + 32 + m)
    guicontrol move, ui_dropdown_delay, % "x" (150) " y" (40 + 32 - 4 + m)

    guicontrol move, ui_separator1, % "x" (3 * m) " y" (40 + 64 + m) " w" (t_w - 4 * m)

    guicontrol move, ui_checkbox_case, % "x" 3 * m " y" 120 + m
    guicontrol move, ui_checkbox_discard, % "x" 3 * m " y" 120 + 25 + m
    guicontrol move, ui_checkbox_beep, % "x" 3 * m " y" 120 + 50 + m

    guicontrol move, ui_separator2, % "x" (3 * m) " y" (200 + m) " w" (t_w - 4 * m)

    guicontrol move, ui_text_language, % "x" 3 * m " y" (200 + 24 + m)
    guicontrol move, ui_dropdown_language, % "x" (150) " y" (200 + 24 - 4 + m) " w" (150)

    ; Second tab (sequences)
    lb_w := UI.app_win.listview.width
    lb_h := t_h - 22 - 40 - m
    bc_w := UI.app_win.bigchar.width
    bc_h := UI.app_win.bigchar.height

    guicontrol move, ui_listbox, % "w" lb_w " h" lb_h

    guicontrol move, ui_text_desc, % "x" (lb_w + 32) " y" (40) " w" (w - lb_w - 3 * m - 32) " h" 120

    guicontrol move, ui_text_bigchar, % "x" lb_w + (w - lb_w - bc_w) / 2 " y" 64 + (h - bc_h) / 2 " w" bc_w " h" bc_h

    guicontrol move, ui_text_filter, % "x" 2 * m " y" (t_h - 10 - m)
    guicontrol move, ui_edit_filter, % "x" (ui_text_filterw + 3 * m) " w" (lb_w - m - ui_text_filterw) " y" (t_h - 10 - 4 - m)

    guicontrol move, ui_keycap_0, % "x" (lb_w + 20 + m) " y" 120

    loop % 9
    {
        if (a_index > strlen(S.selected_seq))
        {
            guicontrol hide, ui_keycap_%a_index%
            guicontrol hide, ui_keytext_%a_index%
        }
        else
        {
            char := substr(S.selected_seq, a_index, 1)

            guicontrol show, ui_keycap_%a_index%
            guicontrol move, ui_keycap_%a_index%, % "x" (lb_w + 20 + m + a_index * 52) " y" 120

            guicontrol text, ui_keytext_%a_index%, % char == "&" ? "&&" : char
            guicontrol show, ui_keytext_%a_index%
            guicontrol move, ui_keytext_%a_index%, % "x" (lb_w + 20 + m + a_index * 52) " y" (120 + 6)
        }
    }
}

refresh_systray()
{
    ; Disable hotkeys except during a compose sequence
    suspend % S.typing ? "off" : "on"

    if (S.disabled)
    {
        icon := 3
        tip := _("@APP_NAME@ (disabled)") "\n"
    }
    else if (!S.typing)
    {
        icon := 1
        tip := _("@APP_NAME@ (active)") "\n"
    }
    else ; if (S.typing)
    {
        icon := 2
        tip := _("@APP_NAME@ (typing)") "\n"
    }

    tip .= _("Loaded @1@ Sequences", R.seq_count) "\n"
    tip .= _("Compose Key is @1@", C.keys.valid[R.compose_key]) "\n"

    menu tray, % S.disabled ? "check" : "uncheck", % _("Disable")
    tmp := C.files.resources
    menu tray, icon
    menu tray, icon, %tmp%, %icon%, 1

    menu tray, tip, %tip%
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

info(string)
{
    traytip, %app%, %string%, 10, 1
}

debug(string)
{
    if (have_debug)
        traytip, %app%, %string%, 10, 1
}

