WinCompose
==========

![Icon](/res/icon.png)

A robust compose key for Windows, written by Sam Hocevar.

Quick start
-----------

 1. Run WinCompose.exe
 2. Press the Right Alt key to initiate a compose sequence
 3. Press the keys for the compose sequence; for instance:
    "," then "c" will type "ç"

Examples
--------

Compose rules are supposed to be intuitive. Here are some examples:

 - ` a → à
 - ' e → é
 - ^ i → î
 - ~ n → ñ
 - / o → ø
 - " u → ü
 - o c → ©
 - + - → ±
 - : - → ÷
 - C C C P → ☭
 - ( 2 3 ) → ㉓
 - < 3 → ♥

The full list of rules can be found in the "Compose" file shipped with WinCompose.

Features
--------

WinCompose supports the standard Compose file format. It ships with more than
1000 compose rules from [Xorg](http://www.x.org/wiki/).

WinCompose supports rules of more than 2 characters such as "(", "3", ")" for "③".

WinCompose supports early exits. For instance, Compose + "&" will immediately
type "&" because there is currently no rule starting with "&".

WinCompose does not emit unnecessary keystrokes, such as a backspace keystroke
to erase the composing characters. Only the final character is sent to the
current application.

Bugs
----

Customising the script requires AutoHotKey (http://www.autohotkey.com/) for now.
The original WinCompose.ahk script file is provided.

Please report bugs to Sam Hocevar <sam@hocevar.net>

