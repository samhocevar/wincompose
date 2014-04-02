
;
; Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.

; Resource files
global compose_file  := "res/Compose.txt"
global keys_file     := "res/Keys.txt"
global resource_file := "res/resources.dll"

; List of keys that can be used for Compose
global valid_keys := { "LAlt"       : _("keys.lalt")
                     , "RAlt"       : _("keys.ralt")
                     , "LControl"   : _("keys.lcontrol")
                     , "RControl"   : _("keys.rcontrol")
                     , "LWin"       : _("keys.lwin")
                     , "RWin"       : _("keys.rwin")
                     , "CapsLock"   : _("keys.capslock")
                     , "NumLock"    : _("keys.numlock")
                     , "Pause"      : _("keys.pause")
                     , "AppsKey"    : _("keys.menu")
                     , "Esc"        : _("keys.esc")
                     , "ScrollLock" : _("keys.scrolllock")
                     , "`"          : _("keys.backtick") }

; Default key used as compose key
global default_key := "RAlt"

; List of numeric keypad keys
global num_keys := { "$Numpad0"    : "$0"
                   , "$Numpad1"    : "$1"
                   , "$Numpad2"    : "$2"
                   , "$Numpad3"    : "$3"
                   , "$Numpad4"    : "$4"
                   , "$Numpad5"    : "$5"
                   , "$Numpad6"    : "$6"
                   , "$Numpad7"    : "$7"
                   , "$Numpad8"    : "$8"
                   , "$Numpad9"    : "$9"
                   , "$NumpadDot"  : "$."
                   , "$NumpadDiv"  : "$/"
                   , "$NumpadMult" : "$*"
                   , "$NumpadAdd"  : "$+"
                   , "$NumpadSub"  : "$-" }

; List of timeout values
global valid_delays := { 500   : _("delays.500ms")
                       , 1000  : _("delays.1000ms")
                       , 2000  : _("delays.2000ms")
                       , 3000  : _("delays.3000ms")
                       , 5000  : _("delays.5000ms")
                       , 10000 : _("delays.10000ms")
                       , -1    : _("delays.infinite") }

; Default timeout value
global default_delay := 5000

; List of window names that must be treated specially because
; of how GTK+ handles Unicode input
global gdk_windows := [ "gdkWindowToplevel"
                      , "xchatWindowToplevel" ]

