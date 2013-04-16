
;
; Copyright: (c) 2013 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.

#SingleInstance force
#EscapeChar \
#Persistent
#NoEnv

;
; Configuration
;

; Compose Key: one of RAlt, LAlt, LControl, RControl, RWin, LWin, Esc,
; Insert, Numlock, Tab
compose_key := "RAlt"

; File containing compose rules
compose_file := "Compose"

; Reset Delay: milliseconds until reset
reset_delay := 5000

; Activate debug messages?
have_debug := false

;
; Initialisation
;

; Read sequences from file
read_sequences(compose_file)

; Build the menu
Menu, Tray, Click, 1
Menu, Tray, NoStandard
Menu, Tray, Add, &Disable, ToggleCallback
Menu, Tray, Add, &Restart, RestartCallback
Menu, Tray, Add
Menu, Tray, Add, &About, AboutCallback
Menu, Tray, Add, E&xit, ExitCallback
Menu, Tray, Icon, wc.ico

; Workaround for an AHK bug that prevents "::`:::" from working in hotstrings
HotKey, $:, workaround_hotkey

; Activate the compose key for real
#Hotstring ? * c b0
Hotkey, %compose_key%, ComposeCallback

; End of initialisation
return

workaround_hotkey:
send_char(substr(a_thishotkey, strlen(a_thishotkey)))
return

;
; Utility functions
;

send_char(char)
{
  global reset_delay
  static sequence := ""
  static compose := false
  static active := true

  if (!active || (!compose && char != "compose"))
  {
    if (char != "compose")
      send_raw(char)
    sequence =
    compose := false
    return
  }

  if (char = "compose")
  {
    debug("Compose key pressed")
    sequence =
    compose := !compose
    if (compose)
    {
      SetTimer, ResetCallback, %reset_delay%
      Menu, Tray, Icon, wca.ico
    }
    else
      Menu, Tray, Icon, wc.ico
    return
  }

  sequence .= char

  debug("Sequence: [ " sequence " ]")

  if (has_sequence(sequence))
  {
    tmp := get_sequence(sequence)
    Send %tmp%
    sequence =
    compose := false
    Menu, Tray, Icon, wc.ico
  }
  else if (!has_prefix(sequence))
  {
    debug("Disabling Dead End Sequence [ " sequence " ]")
    send_raw(sequence)
    sequence =
    compose := false
    Menu, Tray, Icon, wc.ico
  }

  return

ResetCallback:
  sequence =
  compose := false
  Menu, Tray, Icon, wc.ico
  SetTimer, ResetCallback, Off
  return

ToggleCallback:
  active := !active
  if (active)
  {
    Menu, Tray, Uncheck, &Disable
    Menu, Tray, Icon, wc.ico
  }
  else
  {
    Menu, Tray, Check, &Disable
    Menu, Tray, Icon, wcd.ico
  }
  return
}

send_raw(string)
{
  Loop, parse, string
  {
    if (a_loopfield = " ")
      Send {Space}
    else
      SendRaw %a_loopfield%
  }
}

info(string)
{
  TrayTip, WinCompose, %string%, 10, 1
}

debug(string)
{
  global have_debug
  if (have_debug)
    TrayTip, WinCompose, %string%, 10, 1
}

; We need to encode our strings somehow because AutoHotKey objects have
; case-insensitive hash tables. How retarded is that? Also, make sure the
; first character is special
to_hex(str)
{
  hex = *
  loop, parse, str
    hex .= asc(a_loopfield)
  return hex
}

; Read compose sequences from an X11 compose file
read_sequences(file)
{
  FileEncoding UTF-8
  count := 0
  Loop read, %file%
  {
    ; Check whether we get a character between quotes after a colon,
    ; that's our destination character.
    r_right := "^[^"":#]*:[^""#]*""(\\.|[^\\""])*"".*$"
    if (!regexmatch(a_loopreadline, r_right))
      continue
    right := regexreplace(a_loopreadline, r_right, "$1")
  
    ; Everything before that colon is our sequence, only keep it if it
    ; starts with "<Multi_key>".
    r_left := "^[ \\t]*<Multi_key>([^:]*):.*$"
    if (!regexmatch(a_loopreadline, r_left))
      continue
    left := regexreplace(a_loopreadline, r_left, "$1")
    left := regexreplace(left, "[ \\t]*", "")
  
    ; Now replace all special key names to build our sequence
    valid := true
    seq =
    loop, parse, left, "<>"
    {
      decoder := { "space":        " " ; 0x20
                 , "exclam":       "!" ; 0x21
                 , "quotedbl":    """" ; 0x22
                 , "numbersign":   "#" ; 0x23
                 , "dollar":       "$" ; 0x24
                 , "percent":      "%" ; 0x25
                 , "ampersand":    "&" ; 0x26 XXX: Is this the right name?
                 , "apostrophe":   "'" ; 0x27
                 , "parenleft":    "(" ; 0x28
                 , "parenright":   ")" ; 0x29
                 , "asterisk":     "*" ; 0x2a
                 , "plus":         "+" ; 0x2b
                 , "comma":        "," ; 0x2c
                 , "minus":        "-" ; 0x2d
                 , "period":       "." ; 0x2e
                 , "slash":        "/" ; 0x2f
                 , "colon":        ":" ; 0x3a
                 , "semicolon":    ";" ; 0x3b
                 , "less":         "<" ; 0x3c
                 , "equal":        "=" ; 0x3d
                 , "greater":      ">" ; 0x3e
                 , "question":     "?" ; 0x3f
                 , "bracketleft":  "[" ; 0x5b
                 , "backslash":   "\\" ; 0x5c
                 , "bracketright": "]" ; 0x5d
                 , "asciicircum":  "^" ; 0x5e
                 , "underscore":   "_" ; 0x5f
                 , "grave":        "`" ; 0x60
                 , "braceleft":    "{" ; 0x7b
                 , "bar":          "|" ; 0x7c
                 , "braceright":   "}" ; 0x7d
                 , "asciitilde":   "~" } ; 0x7e
      if (strlen(a_loopfield) <= 1)
        seq .= a_loopfield
      else if (decoder.haskey(a_loopfield))
        seq .= decoder[a_loopfield]
      else
        valid := false
    }
  
    ; If still not valid, drop it
    if (!valid)
      continue
  
    add_sequence(seq, right)
    count += 1
  }

  info("Loaded " count " Sequences")
}

; Register a compose sequence, and add all substring prefixes to our list
; of valid prefixes so that we can cancel invalid sequences early on.
add_sequence(key, val)
{
  global s, p
  if (!s)
  {
    s := {}
    p := {}
  }

  s.insert(to_hex(key), val)
  loop % strlen(key) - 1
    p.insert(to_hex(substr(key, 1, a_index)), true)
}

has_sequence(key)
{
  global s
  return s.HasKey(to_hex(key))
}

get_sequence(key)
{
  global s
  return s[to_hex(key)]
}

has_prefix(key)
{
  global p
  return p.HasKey(to_hex(key))
}

ComposeCallback:
  send_char("compose")
  return

RestartCallback:
  Reload
  return

AboutCallback:
  MsgBox, 64, WinCompose, WinCompose\nby Sam Hocevar <sam@hocevar.net>
  return

ExitCallback:
  ExitApp
  return

; Activate hotstrings for all ASCII characters that may
; be used in a compose sequence; these hotstrings just feed
; the character to the underlying engine.
#Hotstring ? * c b
:: ::
send_char(" ")
return
::!::
send_char("!")
return
::"::
send_char("""")
return
::#::
send_char("#")
return
::$::
send_char("$")
return
::%::
send_char("%")
return
::&::
send_char("&")
return
::'::
send_char("'")
return
::(::
send_char("(")
return
::)::
send_char(")")
return
::\*::
send_char("*")
return
::+::
send_char("+")
return
::,::
send_char(",")
return
::-::
send_char("-")
return
::.::
send_char(".")
return
::/::
send_char("/")
return
::0::
send_char("0")
return
::1::
send_char("1")
return
::2::
send_char("2")
return
::3::
send_char("3")
return
::4::
send_char("4")
return
::5::
send_char("5")
return
::6::
send_char("6")
return
::7::
send_char("7")
return
::8::
send_char("8")
return
::9::
send_char("9")
return
; XXX: disabled on purpose because AHK can't parse this
;:::::
;send_char(":")
;return
::;::
send_char(";")
return
::<::
send_char("<")
return
::=::
send_char("=")
return
::>::
send_char(">")
return
::?::
send_char("?")
return
::@::
send_char("@")
return
::A::
send_char("A")
return
::B::
send_char("B")
return
::C::
send_char("C")
return
::D::
send_char("D")
return
::E::
send_char("E")
return
::F::
send_char("F")
return
::G::
send_char("G")
return
::H::
send_char("H")
return
::I::
send_char("I")
return
::J::
send_char("J")
return
::K::
send_char("K")
return
::L::
send_char("L")
return
::M::
send_char("M")
return
::N::
send_char("N")
return
::O::
send_char("O")
return
::P::
send_char("P")
return
::Q::
send_char("Q")
return
::R::
send_char("R")
return
::S::
send_char("S")
return
::T::
send_char("T")
return
::U::
send_char("U")
return
::V::
send_char("V")
return
::W::
send_char("W")
return
::X::
send_char("X")
return
::Y::
send_char("Y")
return
::Z::
send_char("Z")
return
::[::
send_char("[")
return
::\\::
send_char("\\")
return
::]::
send_char("]")
return
::^::
send_char("^")
return
::_::
send_char("_")
return
::\`::
send_char("\`")
return
::a::
send_char("a")
return
::b::
send_char("b")
return
::c::
send_char("c")
return
::d::
send_char("d")
return
::e::
send_char("e")
return
::f::
send_char("f")
return
::g::
send_char("g")
return
::h::
send_char("h")
return
::i::
send_char("i")
return
::j::
send_char("j")
return
::k::
send_char("k")
return
::l::
send_char("l")
return
::m::
send_char("m")
return
::n::
send_char("n")
return
::o::
send_char("o")
return
::p::
send_char("p")
return
::q::
send_char("q")
return
::r::
send_char("r")
return
::s::
send_char("s")
return
::t::
send_char("t")
return
::u::
send_char("u")
return
::v::
send_char("v")
return
::w::
send_char("w")
return
::x::
send_char("x")
return
::y::
send_char("y")
return
::z::
send_char("z")
return
::{::
send_char("{")
return
::|::
send_char("|")
return
::}::
send_char("}")
return
::~::
send_char("~")
return

