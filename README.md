WinCompose
==========

![Icon](/web/icon.png)

A robust compose key for Windows, written by Sam Hocevar.

It allows to easily write characters such as **é ž à ō û ø ☺ ¤
∅ « ♯ ⸘ Ⓚ ㊷ ♪ ♬** using short and often very intuitive key
combinations. For instance, **ö** is obtained using **o** and **"**, and
**♥** is obtained using **<** and **3**.

Download
--------

Latest release: [WinCompose-Setup-0.6.1.exe](/bin/WinCompose-Setup-0.6.1.exe)

Quick start
-----------

After installation, WinCompose should appear in the System Tray;
otherwise, launch it manually.

Press and release the **Right Alt** key to initiate a compose sequence; the
icon should change to indicate a compose sequence is in progress.

Then type in the keys for a compose sequence, such as **A** then **E** for **Æ**:

![Quick Launch](/web/shot1.png)

If **Right Alt** is not suitable for you, right click on the tray icon to
choose another key.

Examples
--------

Compose rules are supposed to be intuitive. Here are some examples:

 - `` ` `` `a` → `à`
 - `'` `e` → `é`
 - `^` `i` → `î`
 - `~` `n` → `ñ`
 - `/` `o` → `ø`
 - `"` `u` → `ü`
 - `o` `c` → `©`
 - `+` `-` → `±`
 - `:` `-` → `÷`
 - `(` `7` `)` → `⑦`
 - `C` `C` `C` `P` → `☭`
 - `<` `3` → `♥`

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

WinCompose supports rules of more than 2 characters such as **( 3 )**
for **③**.

WinCompose supports early exits. For instance, **Compose &** will
immediately type **&** because there is currently no rule starting with **&**.

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

Please report bugs to Sam Hocevar <sam@hocevar.net>

News
----

News for version 0.6.1 (14 April 2014)
 - Fix issues with the save files.
 - Czech translation.

News for version 0.6.0 (8 April 2014)
 - More than 600 new compose rules from the dotXcompose project.
 - The sequence window is now a lot more usable and appealing.

News for version 0.5.0 (3 April 2014)
 - Support for custom compose rules in `%userprofile%/.XCompose.txt`.
 - Using Alt as the compose key does not disable Alt-Tab, and other
   similar fixes.
 - Keeping the compose key pressed no longer causes the icon to flicker.

News for version 0.4.6 (26 February 2014)
 - Internationalisation support.
 - The key timeout can be entirely disabled.
 - Unicode characters in the sequence window can be copied to the clipboard.

News for version 0.4.5 (13 September 2013)
 - Support for numeric keypad keys.
 - Bugfix for pure ASCII compose sequences.
 - Cosmetic UI fixes.

News for version 0.4.4 (2 September 2013)
 - Support for Unicode codepoints above U+FFFF.

News for version 0.4.3 (23 August 2013)
 - Support for 32-bit Windows.
 - Support for the X-Chat IRC client.

News for version 0.4.2 (11 June 2013)
 - Better support for GTK+ applications such as Pidgin.
 - Minor GUI fixes.

News for version 0.4.1 (6 May 2013)
 - “Menu”, “Escape” and “Backtick” can now act as compose
   keys, too.
 - Improved the filtering logic.
 - Allow to choose the timeout delay from the context menu.

News for version 0.4.0 (4 May 2013)
 - “Pause” and “Scroll Lock” can now act as compose keys, too.
 - It is possible to filter sequences by keyword
 - The sequence window can be resized and is displayed with a simple
   click on the systray icon.
 - Cosmetic fixes in the GUI.

News for version 0.3.0 (26 April 2013)
 - The key used for composing is now customisable.

News for version 0.2.0 (24 April 2013)
 - Highly improved compatibility with games.
 - “Typing” and “Disabled” icons are now more easily told apart.

News for version 0.1.1 (23 April 2013)
 - Admin privileges are no longer required to start WinCompose at startup.

News for version 0.1 (22 April 2013)
 - Initial release.

