
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
        t := {}

        regread, locale, HKEY_CURRENT_USER, Control Panel\\International, localename
        files := [ "default", substr(locale, 1, 2), regexreplace(locale, "-", "_") ]

        FileEncoding UTF-8

        for ignored, file in files
        {
            section := ""
            loop read, % "locale/" file ".ini"
            {
                regex := "^[ \\t]*\\[([^ \\t\\]]*)[^ \\t\\]]*\\].*"
                newsection := regexreplace(a_loopreadline, regex, "$1", ret)
                if (ret == 1)
                {
                    section := newsection
                    continue
                }

                regex := "^[ \\t]*([^ \\t=]*)[ \\t]*=[ \\t]*(""(.*)""|(.*[^ ]))[ \\t]*$"
                key := regexreplace(a_loopreadline, regex, "$1", ret)
                val := regexreplace(a_loopreadline, regex, "$3$4", ret2)
                if (ret == 1 && ret2 == 1)
                {
                    t.insert(section "." key, val)
                    continue
                }
            }
        }
    }

    ret := t.haskey(str) ? t[str] : "[" str "]"

    ret := regexreplace(ret, "@APP_NAME@", app)
    ret := regexreplace(ret, "@APP_VERSION@", version)
    ret := regexreplace(ret, "@AHK_VERSION@", a_ahkversion)

    for index, arg in args
        ret := regexreplace(ret, "@" index "@", arg)

    return ret
}

