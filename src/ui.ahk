
;
; Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.


; UI-related constants
global UI := { _:_
    ; Sequence window
  , seq_win : { _:_
      , width: 0
      , height: 0
        ; The listview
      , listview : { _:_
          , width: 260 } } }

; Global GUI variables
global ui_listbox, ui_edit_filter, ui_button
global ui_text_filter, ui_text_filterw, ui_text_bigchar, ui_text_desc
global ui_keycap_0
global ui_keycap_1, ui_keycap_2, ui_keycap_3, ui_keycap_4, ui_keycap_5, ui_keycap_6, ui_keycap_7, ui_keycap_8, ui_keycap_9
global ui_keytext_1, ui_keytext_2, ui_keytext_3, ui_keytext_4, ui_keytext_5, ui_keytext_6, ui_keytext_7, ui_keytext_8, ui_keytext_9


create_gui()
{
    onexit exit_callback

    create_systray()
    create_seq_win()

    refresh_systray()
}

create_systray()
{
    ; The hotkey selection menu
    for key, val in C.keys.valid
        menu, hotkeymenu, add, %val%, hotkeymenu_callback

    ; The delay selection menu
    for key, val in C.delays.valid
        menu, delaymenu, add, %val%, delaymenu_callback

    ; Build the systray menu
    menu tray, click, 1
    menu tray, NoStandard
    menu tray, add, % _("Sequences…"), showgui_callback
    menu tray, add, % _("Compose Key"), :hotkeymenu
    menu tray, add, % _("Timeout"), :delaymenu
    menu tray, add, % _("Disable"), toggle_callback
    menu tray, add, % _("Restart"), restart_callback
    menu tray, add
    if (have_debug)
    {
        menu tray, add, % _("&History"), history_callback
        menu tray, add, % _("Hotkey &List"), hotkeylist_callback
    }
    menu tray, add, % _("&About"), about_callback
    menu tray, add, % _("&Visit Website"), website_callback
    menu tray, add, % _("E&xit"), exit_callback
    menu tray, default, % _("Sequences…")

    return

showgui_callback:
    critical on
    gui_title := _("@APP_NAME@ - List of sequences")
    if (winexist(gui_title))
        goto hidegui_callback
    recompute_gui_filter()
    gui show, , %gui_title%
    guicontrol focus, ui_edit_filter
    return

hidegui_callback:
    hide_seq_win()
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
    about_text := _("@APP_NAME@ v@APP_VERSION@") . "\n"
    about_text .= _("") . "\n"
    about_text .= _("by Sam Hocevar <sam@hocevar.net>") . "\n"
    about_text .= _("running on AHK v@AHK_VERSION@") . "\n"
    msgbox 64, %app%, %about_text%
    return

website_callback:
    run %website%
    return

exit_callback:
    save_settings()
    exitapp
    return
}

create_seq_win()
{
    ; Build the sequence list window
    gui +resize +minsize720x400
    gui margin, 8, 8

    gui font, s11
    gui font, s11, Courier New
    gui font, s11, Lucida Console
    gui font, s11, Consolas
    columns := _("Sequence") "|" _("Char") "|" _("Unicode")
    gui add, listview, % "vui_listbox glistview_callback w" UI.seq_win.listview.width " r5 altsubmit -multi", % columns

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

    gui add, edit, vui_edit_filter gedit_callback

    gui add, button, vui_button default, % _("Close")

    ; The copy character menu
    menu, contextmenu, add, % _("Copy Character"), copychar_callback

    return

guisize:
    if (a_eventinfo != 1) ; Ignore minimising
    {
        UI.seq_win.width := a_guiwidth
        UI.seq_win.height := a_guiheight
        refresh_gui()
    }
    return

guicontextmenu:
    if (a_guicontrol == "ui_listbox")
    {
        if (a_eventinfo > 0)
        {
            lv_gettext(tmp, a_eventinfo, 2)
            S.selected_char := tmp
        }
    }
    menu, contextmenu, show
    return

copychar_callback:
    clipboard := S.selected_char
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
            if (char != S.selected_char)
            {
                lv_gettext(sequence, a_eventinfo, 1)
                sequence := regexreplace(sequence, " ", "")
                sequence := regexreplace(sequence, "space", " ")
                lv_gettext(unicode, a_eventinfo, 3)

                S.selected_char := char
                S.selected_seq := sequence

                guicontrol text, ui_text_bigchar, %char%
                ; HACK: remove the non-printable character we added for sorting purposes
                desc := " " regexreplace(unicode, ".*U", "U") " " char "\n"
                desc .= substr("————————————————————", 1, strlen(unicode) + 4) "\n"
                desc .= _("Description:") " " get_description(sequence)
                guicontrol text, ui_text_desc, %desc%
            }
            refresh_gui()
        }
    }
    return

buttonclose:
guiclose:
guiescape:
    hide_seq_win()
    return
}

hide_seq_win()
{
    gui hide
}

refresh_gui()
{
    w := UI.seq_win.width
    h := UI.seq_win.height
    lb_w := UI.seq_win.listview.width
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
        if (a_index > strlen(S.selected_seq))
        {
            guicontrol hide, ui_keycap_%a_index%
            guicontrol hide, ui_keytext_%a_index%
        }
        else
        {
            char := substr(S.selected_seq, a_index, 1)

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
    if (S.disabled)
    {
        suspend on
        menu tray, check, % _("Disable")
        tmp := C.files.resources
        menu tray, icon
        menu tray, icon, %tmp%, 3, 1
        menu tray, tip, % _("@APP_NAME@ (disabled)")
    }
    else if (!S.typing)
    {
        ; Disable hotkeys; we only want them on during a compose sequence
        suspend on
        menu tray, uncheck, % _("Disable")
        tmp := C.files.resources
        menu tray, icon
        menu tray, icon, %tmp%, 1, 1
        menu tray, tip, % _("@APP_NAME@ (active)")
    }
    else ; if (S.typing)
    {
        suspend off
        menu tray, uncheck, % _("Disable")
        tmp := C.files.resources
        menu tray, icon
        menu tray, icon, %tmp%, 2
        menu tray, tip, % _("@APP_NAME@ (typing)")
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

