WinCompose
==========

![Icon](/web/icon.png)

A robust compose key for Windows, written by Sam Hocevar.

It allows to easily write characters such as **é ž à ō û ø ☺ ¤
∅ « ♯ ⸘ Ⓚ ㊷ ♪ ♬** using short and often very intuitive key
combinations. For instance, **ö** is obtained using <kbd>o</kbd> and <kbd>"</kbd>, and
**♥** is obtained using <kbd>&lt;</kbd> and <kbd>3</kbd>.

Download
--------

Latest release: [WinCompose-Setup-0.6.3.exe](https://github.com/samhocevar/wincompose/releases/download/v0.6.3/WinCompose-Setup-0.6.3.exe)

Quick start
-----------

After installation, WinCompose should appear in the System Tray;
otherwise, launch it manually.

Press and release the <kbd>Right Alt</kbd> key to initiate a compose sequence; the
icon should change to indicate a compose sequence is in progress.

Then type in the keys for a compose sequence, such as <kbd>A</kbd> then <kbd>E</kbd> for **Æ**:

![Quick Launch](/web/shot1.png)

If <kbd>Right Alt</kbd> is not suitable for you, right click on the tray icon to
choose another key.

Examples
--------

Compose rules are supposed to be intuitive. Here are some examples:

 - <kbd>\`</kbd> <kbd>a</kbd> → **à**
 - <kbd>'</kbd> <kbd>e</kbd> → **é**
 - <kbd>^</kbd> <kbd>i</kbd> → **î**
 - <kbd>~</kbd> <kbd>n</kbd> → **ñ**
 - <kbd>/</kbd> <kbd>o</kbd> → **ø**
 - <kbd>"</kbd> <kbd>u</kbd> → **ü**
 - <kbd>o</kbd> <kbd>c</kbd> → **©**
 - <kbd>+</kbd> <kbd>-</kbd> → **±**
 - <kbd>:</kbd> <kbd>-</kbd> → **÷**
 - <kbd>(</kbd> <kbd>7</kbd> <kbd>)</kbd> → **⑦**
 - <kbd>C</kbd> <kbd>C</kbd> <kbd>C</kbd> <kbd>P</kbd> → **☭**
 - <kbd>&lt;</kbd> <kbd>3</kbd> → **♥**

The full list of rules can be found by clicking on the WinCompose system tray
icon or using the “Show Sequences…” menu entry:

![Sequence List](/web/shot2.png)

The window allows you to filter the sequences being listed.

Features
--------

WinCompose supports the standard Compose file format. It provides more than
1500 compose rules from the [Xorg](http://www.x.org/wiki/) project and the
[dotXCompose](https://github.com/kragen/xcompose) project. You can add custom
rules by creating a file named `.XCompose` or `.XCompose.txt` in your
`%USERPROFILE%` directory.

WinCompose supports rules of more than 2 characters such as <kbd>(</kbd> <kbd>3</kbd> <kbd>)</kbd>
for **③**.

WinCompose supports early exits. For instance, <kbd>Compose</kbd> <kbd>&</kbd> will
immediately type **&** because there is currently no rule starting with <kbd>&</kbd>.

WinCompose does not emit unnecessary keystrokes, such as a backspace keystroke
to erase the composing characters. Only the final character is sent to the
current application.

Bugs
----

WinCompose is known to misbehave in the following situations:
 - When the Synergy keyboard sharing software is running.
 - When several keyboard layouts are installed on the computer and they are
   changed on the fly.

Most of these issues are actually bugs in the AutoHotKey software upon which
WinCompose is based.

Please report bugs to Sam Hocevar <sam@hocevar.net> or to the [GitHub issue
tracker](https://github.com/samhocevar/wincompose/issues).

