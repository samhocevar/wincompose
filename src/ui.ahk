
;
; Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.


; UI-related constants
global UI := { _:_
  , listview : { width: 260 } }

; Global GUI variables
global ui_listbox, ui_edit_filter, ui_button
global ui_text_filter, ui_text_filterw, ui_text_bigchar, ui_text_desc
global ui_keycap_0
global ui_keycap_1, ui_keycap_2, ui_keycap_3, ui_keycap_4, ui_keycap_5, ui_keycap_6, ui_keycap_7, ui_keycap_8, ui_keycap_9
global ui_keytext_1, ui_keytext_2, ui_keytext_3, ui_keytext_4, ui_keytext_5, ui_keytext_6, ui_keytext_7, ui_keytext_8, ui_keytext_9


create_gui()
{
    onexit exit_callback

    ; The hotkey selection menu
    for key, val in C.keys.valid
        menu, hotkeymenu, add, %val%, hotkeymenu_callback

    ; The delay selection menu
    for key, val in C.delays.valid
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
    gui add, listview, % "vui_listbox glistview_callback w" UI.listview.width " r5 altsubmit -multi", % _("seq_win.columns")

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
    for key, val in C.keys.valid
        if (val == a_thismenuitem)
            R.compose_key := key
    refresh_systray()
    set_compose_hotkeys(true)
    return

delaymenu_callback:
    for key, val in C.delays.valid
        if (val == a_thismenuitem)
            R.reset_delay := key
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
            lv_gettext(char, a_eventinfo, 2)
            if (char != state.selected_char)
            {
                lv_gettext(sequence, a_eventinfo, 1)
                sequence := regexreplace(sequence, " ", "")
                sequence := regexreplace(sequence, "space", " ")
                lv_gettext(unicode, a_eventinfo, 3)

                state.selected_char := char
                state.selected_seq := sequence

                guicontrol text, ui_text_bigchar, %char%
                ; HACK: remove the non-printable character we added for sorting purposes
                desc := " " regexreplace(unicode, ".*U+", "U+") " " char "\n"
                desc .= substr("————————————————————", 1, strlen(unicode) + 5) "\n"
                desc .= get_description(sequence)
                guicontrol text, ui_text_desc, %desc%
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

refresh_gui()
{
    w := state.gui_width
    h := state.gui_height
    lb_w := UI.listview.width
    lb_h := h - 45
    bigchar_w := 180
    bigchar_h := 180

    guicontrol move, ui_listbox, % "w" lb_w " h" lb_h

    guicontrol move, ui_text_desc, % "x" lb_w + 32 " y" 16 " w" w - lb_w - 40 " h" 120

    guicontrol move, ui_text_bigchar, % "x" lb_w + (w - lb_w - bigchar_w) / 2 " y" 64 + (h - bigchar_h) / 2 " w" bigchar_w " h" bigchar_h

    guicontrol move, ui_text_filter, % "x" 8 " y" (h - 26)
    guicontrol move, ui_edit_filter, % "x" (ui_text_filterw + 15) " w" (w - 140 - ui_text_filterw) " y" (h - 30)
    guicontrol move, ui_button, % "x" (w - 87) " y" (h - 30) " w80"

    guicontrol move, ui_keycap_0, % "x" (lb_w + 20) " y" 100

    loop % 9
    {
        if (a_index > strlen(state.selected_seq))
        {
            guicontrol hide, ui_keycap_%a_index%
            guicontrol hide, ui_keytext_%a_index%
        }
        else
        {
            char := substr(state.selected_seq, a_index, 1)

            guicontrol show, ui_keycap_%a_index%
            guicontrol move, ui_keycap_%a_index%, % "x" (lb_w + 20 + a_index * 52) " y" 100

            guicontrol text, ui_keytext_%a_index%, % char == "&" ? "&&" : char
            guicontrol show, ui_keytext_%a_index%
            guicontrol move, ui_keytext_%a_index%, % "x" (lb_w + 20 + a_index * 52) " y" (100 + 6)
        }
    }
}

refresh_systray()
{
    if (state.disabled)
    {
        suspend on
        menu tray, check, % _("menu.disable")
        tmp := C.files.resources
        menu tray, icon, %tmp%, 3, 1
        menu tray, tip, % _("tray_tip.disabled")
    }
    else if (!state.typing)
    {
        ; Disable hotkeys; we only want them on during a compose sequence
        suspend on
        menu tray, uncheck, % _("menu.disable")
        tmp := C.files.resources
        menu tray, icon, %tmp%, 1, 1
        menu tray, tip, % _("tray_tip.active")
    }
    else ; if (state.typing)
    {
        suspend off
        menu tray, uncheck, % _("menu.disable")
        tmp := C.files.resources
        menu tray, icon, %tmp%, 2
        menu tray, tip, % _("tray_tip.typing")
    }

    for key, val in C.keys.valid
        menu, hotkeymenu, % (key == R.compose_key) ? "check" : "uncheck", %val%

    for key, val in C.delays.valid
        menu, delaymenu, % (key == R.reset_delay) ? "check" : "uncheck", %val%
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

