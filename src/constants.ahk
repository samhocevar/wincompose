
;
; Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.


;
; All system-wide constants
;
global C := { _:_

    ;
    ; Various resource files we need
    ;

  , files : { _:_
      , sequences : [ "res/Xorg.txt", "res/Xcompose.txt" ]
      , keys      : [ "res/Keys.txt" ]
      , resources : "res/resources.dll" }

    ;
    ; Keyboard-related constants
    ;

  , keys : { _:_
        ; List of keys that can be used for Compose
      , valid : { "lalt"       : _("keys.lalt")
                , "ralt"       : _("keys.ralt")
                , "lcontrol"   : _("keys.lcontrol")
                , "rcontrol"   : _("keys.rcontrol")
                , "lwin"       : _("keys.lwin")
                , "rwin"       : _("keys.rwin")
                , "capslock"   : _("keys.capslock")
                , "numlock"    : _("keys.numlock")
                , "pause"      : _("keys.pause")
                , "appskey"    : _("keys.menu")
                , "esc"        : _("keys.esc")
                , "scrolllock" : _("keys.scrolllock")
                , "`"          : _("keys.backtick") }
        ; Default key used as compose key
      , default : "ralt"
        ; List of numeric keypad keys
      , numpad : { "numpad0"    : "0"
                 , "numpad1"    : "1"
                 , "numpad2"    : "2"
                 , "numpad3"    : "3"
                 , "numpad4"    : "4"
                 , "numpad5"    : "5"
                 , "numpad6"    : "6"
                 , "numpad7"    : "7"
                 , "numpad8"    : "8"
                 , "numpad9"    : "9"
                 , "numpaddot"  : "."
                 , "numpaddiv"  : "/"
                 , "numpadmult" : "*"
                 , "numpadadd"  : "+"
                 , "numpadsub"  : "-" }
        ; List of special keys that we need to hijack
      , special : [ "tab" ; for Alt-Tab
                  , "left", "right", "up", "down" ; for Windows-Left etc.
                  , "f1",  "f2",  "f3",  "f4",  "f5",  "f6"
                  , "f7",  "f8",  "f9",  "f10", "f11", "f12"
                  , "f13", "f14", "f15", "f16", "f17", "f18"
                  , "f19", "f20", "f21", "f22", "f23", "f24" ] } ; for Alt-F4 etc.

    ;
    ; Timeout-related constants
    ;

  , delays : { _:_
        ; List of timeout values
      , valid : { 500   : _("delays.500ms")
                , 1000  : _("delays.1000ms")
                , 2000  : _("delays.2000ms")
                , 3000  : _("delays.3000ms")
                , 5000  : _("delays.5000ms")
                , 10000 : _("delays.10000ms")
                , -1    : _("delays.infinite") }
        ; Default timeout value
      , default : 5000 }

    ;
    ; Hacks
    ;

  , hacks : { _:_
        ; List of window names that must be treated specially because
        ; of how GTK+ handles Unicode input
      , gdk_classes : [ "gdkWindowToplevel"
                      , "xchatWindowToplevel" ] } }

