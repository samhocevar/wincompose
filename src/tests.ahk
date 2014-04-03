
;
; Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
;   This program is free software; you can redistribute it and/or
;   modify it under the terms of the Do What The Fuck You Want To
;   Public License, Version 2, as published by the WTFPL Task Force.
;   See http://www.wtfpl.net/ for more details.

#singleinstance force
#escapechar \
#noenv
#notrayicon

#include utils.ahk

global test_count := 0, fail_count := 0, errors := ""

main()
return

;
; Run unit tests and print a message box with a report
;
main()
{
    test_arrays()

    title := "WinCompose Unit Tests"
    if (fail_count)
        msgbox 640, % title, % fail_count " test(s) failed out of " test_count ":\n" errors
    else
        msgbox 64, % title, % "All " test_count " tests passed successfully."

    exit
}

;
; Test array features
;
test_arrays()
{
    ; Check the array length function
    a := []
    check(length(a) == 0)
    a := [ 1 ]
    check(length(a) == 1)
    a := [ 1, 2 ]
    check(length(a) == 2)
    a := [ 1, 2, 3 ]
    check(length(a) == 3)

    a := []
    loop % 100000
    {
        a[a_index] := a_index
    }
    check(length(a) == 100000)
}

;
; Our main check() function
;
check(condition)
{
    test_count += 1

    if (condition)
        return

    fail_count += 1
    ex_cur := exception("", -1)
    ex_prev := exception("", -2)
    filereadline line, % ex_cur.file, % ex_cur.line
    errors .= ex_cur.file ":" ex_cur.line ", function " ex_prev.what "():" line "\n"
}

