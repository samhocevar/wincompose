
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
      , sequences : [ "res/Xorg.txt"
                    , "res/Xcompose.txt"
                    , "res/WinCompose.txt" ]
      , keys      : [ "res/Keys.txt" ]
      , resources : "res/resources.dll" }

    ;
    ; Available translation languages
    ;

  , languages : { _:_
        ; List of available languages for the UI
      , valid : { ""   : _("Autodetect")
                , "cs" : "Čeština"
                , "de" : "Deutsch"
                , "el" : "Ελληνικά"
                , "en" : "English"
                , "fr" : "Français"
                , "id" : "Bahasa Indonesia" }
        ; Default language means try to autodetect
      , default : "" }

    ;
    ; Keyboard-related constants
    ;

  , keys : { _:_
        ; List of keys that can be used for Compose
      , valid : { "lalt"       : _("Left Alt")
                , "ralt"       : _("Right Alt")
                , "lcontrol"   : _("Left Control")
                , "rcontrol"   : _("Right Control")
                , "lwin"       : _("Left Windows")
                , "rwin"       : _("Right Windows")
                , "capslock"   : _("Caps Lock")
                , "numlock"    : _("Num Lock")
                , "pause"      : _("Pause")
                , "appskey"    : _("Menu")
                , "esc"        : _("Escape")
                , "scrolllock" : _("Scroll Lock")
                , "`"          : _("Grave Accent `") }
        ; Default key used as compose key
      , default : "ralt"
        ; List of virtual keycodes that lead to printable things
      , printable : [ [ 0x20, 0x21 ] ; Space
                    , [ 0x30, 0x3a ] ; 0-9
                    , [ 0x41, 0x5b ] ; A-Z
                    , [ 0x60, 0x70 ] ; keypad
                    , [ 0xba, 0xc1 ] ; misc
                    , [ 0xdb, 0xe0 ] ; misc
                    , [ 0xe1, 0xe7 ] ] ; misc
        ; List of special keys that we need to hijack
      , special : [ "tab", "esc" ; for Alt-Tab and Alt-Escapce
                  , "left", "right", "up", "down" ; for Windows-Left etc.
                  , "f1",  "f2",  "f3",  "f4",  "f5",  "f6" ; for Alt-F4 etc.
                  , "f7",  "f8",  "f9",  "f10", "f11", "f12"
                  , "f13", "f14", "f15", "f16", "f17", "f18"
                  , "f19", "f20", "f21", "f22", "f23", "f24" ] }

    ;
    ; Timeout-related constants
    ;

  , delays : { _:_
        ; List of timeout values
      , valid : { 500   : _("500 milliseconds")
                , 1000  : _("1 second")
                , 2000  : _("2 seconds")
                , 3000  : _("3 seconds")
                , 5000  : _("5 seconds")
                , 10000 : _("10 seconds")
                , -1    : _("None") }
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

