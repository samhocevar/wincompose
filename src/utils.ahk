
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

; We need to encode our strings somehow because AutoHotKey objects have
; case-insensitive hash tables. How retarded is that? Also, make sure the
; first character is special.
string_to_hex(str)
{
    hex := "*"
    loop, parse, str
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
    static t

    if (!t)
    {
        t := {_:_}

        regread, locale, HKEY_CURRENT_USER, Control Panel\\International, localename
        languages := [ substr(locale, 1, 2), regexreplace(locale, "-", "_") ]

        FileEncoding UTF-8

        for ignored, lang in languages
        {
            fuzzy := false
            src := false
            dst := false
            loop read, % "po/" lang ".po"
            {
                if (regexmatch(a_loopreadline, "^ *$") > 0)
                {
                    if (dst)
                        fuzzy := false
                }
                else if (regexmatch(a_loopreadline, "^#, fuzzy") > 0)
                {
                    fuzzy := true
                    src := false
                    dst := false
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
                       continue
                    }

                    s := regexreplace(a_loopreadline, "^msgstr *""(.*)"".*", "$1", ret)
                    if (ret == 1)
                    {
                       dst := s
                       continue
                    }

                    s := regexreplace(a_loopreadline, "^ *""(.*)"".*", "$1", ret)
                    if (ret == 1)
                    {
                        if (dst)
                            dst .= s
                        else
                            src .= s
                        continue
                    }
                }

                ; Always try to insert the line, even if "dst" isn't fully built,
                ; because we may hit the last line of the script a bit too early
                if (dst && !fuzzy)
                {
                    replaces := { "\\n": "\n"
                                , "\\r": "\r"
                                , "\\""": """"
                                , "\\\\": "\\" }
                    for before, after in replaces
                    {
                        src := regexreplace(src, before, after)
                        dst := regexreplace(dst, before, after)
                    }
                    t.insert(string_to_hex(src), dst)
                }
            }
        }
    }

    key := string_to_hex(str)
    ret := t.haskey(key) ? t[key] : str

    ret := regexreplace(ret, "@APP_NAME@", app)
    ret := regexreplace(ret, "@APP_VERSION@", version)
    ret := regexreplace(ret, "@AHK_VERSION@", a_ahkversion)

    for index, arg in args
        ret := regexreplace(ret, "@" index "@", arg)

    return ret
}

