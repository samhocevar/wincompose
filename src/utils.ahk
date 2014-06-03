
;
; Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.


; Return the length of an array, provided the data indices are 1, 2,
; 3, ... n, without any holes.
length(array)
{
    ret := array.maxindex()
    return ret ? ret : 0
}

; I can't believe AHK doesn't have these
min(a, b)
{
    return a < b ? a : b
}

max(a, b)
{
    return a > b ? a : b
}

; Convert string to uppercase
toupper(string)
{
    stringupper ret, string
    return ret
}

; Convert string to lowercase
tolower(string)
{
    stringlower ret, string
    return ret
}

; Unescape \ in string
unescape(string)
{
    replaces := { "\\n": "\n"
                , "\\r": "\r"
                , "\\""": """"
                , "\\\\": "\\" }
    for before, after in replaces
        string := regexreplace(string, before, after)
    return string
}

; Humanise sequence
humanize_sequence(string)
{
    string := regexreplace(string, "(.)", " $1")
    string := regexreplace(string, "  ", " space")
    string := regexreplace(string, "^ ", "")
    return string
}

; Dehumanise sequence
dehumanize_sequence(string)
{
    ; FIXME: this will fail if string contains "s p a c e", but do we care?
    string := regexreplace(string, " ", "")
    string := regexreplace(string, "space", " ")
    return string
}

; We need to encode our strings somehow because AutoHotKey objects have
; case-insensitive hash tables. How retarded is that? Also, make sure the
; first character is special.
string_to_hex(string)
{
    hex := "*"
    loop, parse, string
        hex .= num_to_hex(asc(a_loopfield), 2)
    return hex
}

; Convert a number to a hexadecimal string, padding with leading zeroes
; up to mindigits if necessary.
num_to_hex(x, mindigits)
{
    chars := "0123456789ABCDEF"
    ret := ""
    while (x > 0)
    {
        ret := substr(chars, 1 + mod(x, 16), 1) . ret
        x := floor(x / 16)
    }
    while (strlen(ret) < mindigits || strlen(ret) < 1)
        ret := "0" . ret
    return ret
}

;
; Handle Settings
;

load_all_settings()
{
    ; Read the compose key value and sanitise it if necessary
    tmp := load_setting("global", "compose_key")
    R.compose_key := C.keys.valid.haskey(tmp) ? tmp : C.keys.default

    ; Read the reset delay value and sanitise it if necessary
    tmp := load_setting("global", "reset_delay")
    R.reset_delay := C.delays.valid.haskey(tmp) ? tmp : C.delays.default

    ; Read the UI language value and sanitise it if necessary
    tmp := load_setting("global", "language")
    R.language := C.languages.valid.haskey(tmp) ? tmp : C.languages.default

    R.opt_case := load_setting("global", "case_insensitive", "false") == "true"
    R.opt_discard := load_setting("global", "discard_on_invalid", "false") == "true"
    R.opt_beep := load_setting("global", "beep_on_invalid", "false") == "true"

    save_all_settings()
}

save_all_settings()
{
    save_setting("global", "compose_key", R.compose_key)
    save_setting("global", "reset_delay", R.reset_delay)
    save_setting("global", "language", R.language)

    save_setting("global", "case_insensitive", R.opt_case ? "true" : "false")
    save_setting("global", "discard_on_invalid", R.opt_discard ? "true" : "false")
    save_setting("global", "beep_on_invalid", R.opt_beep ? "true" : "false")
}

; Read one entry from the configuration file
load_setting(section, key, default = "")
{
    ; We cannot read R.config_file from the configuration file, so we decide
    ; right now whether it'll be in %a_appdata% (installed application) or
    ; %a_workingdir% (portable application).
    if (R.config_file == "")
    {
        dir := fileexist("unins000.dat") ? a_appdata "\\" app : a_workingdir
        filecreatedir %dir%
        R.config_file := dir "\\settings.ini"
    }

    iniread tmp, % R.config_file, %section%, %key%, %default%
    return tmp
}

; Write one entry to the configuration file
save_setting(section, key, value)
{
    iniwrite %value%, % R.config_file, %section%, %key%
}

; Handle i18n
_(str, args*)
{
    static translations

    if (!translations)
    {
        ; HACK: we cannot rely on settings being loaded at this point, because
        ; they require the translation system to be initialised. There is no
        ; clean solution to this catch-22, so we just break the dependency
        ; cycle. The worst that can happen is probably a UI in English, and
        ; that will be fixed the next time the app is launched.
        translations := setlocale(load_setting("global", "language"))
    }

    ret := translations[string_to_hex(str)]
    ret := ret ? ret : str

    ret := regexreplace(ret, "@APP_NAME@", app)
    ret := regexreplace(ret, "@APP_VERSION@", version)
    ret := regexreplace(ret, "@AHK_VERSION@", a_ahkversion)

    for index, arg in args
        ret := regexreplace(ret, "@" index "@", arg)

    return ret
}

; Load appropriate .po file
setlocale(locale)
{
    t := {_:_}

    ; If no locale specified, try to autodetect
    if (!locale)
        regread, locale, HKEY_CURRENT_USER, Control Panel\\Desktop, PreferredUILanguages
    if (!locale)
        regread, locale, HKEY_CURRENT_USER, Control Panel\\Desktop\\MuiCached, MachinePreferredUILanguages
    if (!locale)
        regread, locale, HKEY_CURRENT_USER, Control Panel\\International, localename

    languages := [ substr(locale, 1, 2), regexreplace(locale, "-", "_") ]

    FileEncoding UTF-8

    for ignored, lang in languages
    {
        fuzzy := false
        msgstr := false
        loop read, % "po/" lang ".po"
        {
            if (regexmatch(a_loopreadline, "^ *$") > 0)
            {
                fuzzy := false
                msgstr := false
            }
            else if (regexmatch(a_loopreadline, "^#, fuzzy") > 0)
            {
                fuzzy := true
                msgstr := false
            }
            else if (regexmatch(a_loopreadline, "^#") > 0)
            {
                ; do nothing
            }
            else
            {
                s := regexreplace(a_loopreadline, "^msgid *""(.*)"".*", "$1", ret)
                if (ret == 1)
                {
                    src := s
                }

                s := regexreplace(a_loopreadline, "^msgstr *""(.*)"".*", "$1", ret)
                if (ret == 1)
                {
                    dst := s
                    msgstr := true
                }

                s := regexreplace(a_loopreadline, "^ *""(.*)"".*", "$1", ret)
                if (ret == 1)
                {
                    (msgstr ? dst : src) .= s
                }
            }

            ; Always try to insert the line, even if "dst" isn't fully built,
            ; because we may hit the last line of the script a bit too early
            if (msgstr && !fuzzy)
                t.insert(string_to_hex(unescape(src)), unescape(dst))
        }
    }

    return t
}

