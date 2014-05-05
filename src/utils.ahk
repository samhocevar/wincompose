
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
        iniread tmp, %config_file%, Global, % "language", % ""

        translations := setlocale(tmp)
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

