
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
    low := 0, high := 1

    ; Find an upper bound for the length
    while (array.haskey(high))
    {
        low := high
        high := high * 2
    }

    ; Bisect to find the real length
    while (low + 1 < high)
    {
        mid := floor((low + high + 1) / 2)
        if (array.haskey(mid))
            low := mid
        else
            high := mid
    }

    return low
}

