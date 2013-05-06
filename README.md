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

Latest release: [WinCompose-Setup-0.4.1.exe](/WinCompose-Setup-0.4.1.exe)

Quick start
-----------

After installation, WinCompose should appear in the System Tray;
otherwise, launch it manually:

![Quick Launch](/web/shot1.png)

Press and release the **Right Alt** key to initiate a compose sequence; the
icon should change to indicate a compose sequence is in progress:

![In Progress](/web/shot2.png)

Type in the keys for a compose sequence, such as **A** then **E** for **Æ**:

![In Progress](/web/shot3.png)

If **Right Alt** is not suitable for you, right click on the tray icon to
choose another key.

Examples
--------

Compose rules are supposed to be intuitive. Here are some examples:

 - **` a** → **à**
 - **' e** → **é**
 - **^ i** → **î**
 - **~ n** → **ñ**
 - **/ o** → **ø**
 - **" u** → **ü**
 - **o c** → **©**
 - **+ -** → **±**
 - **: -** → **÷**
 - **C C C P** → **☭**
 - **( 2 3 )** → **㉓**
 - **< 3** → **♥**

The full list of rules can be found in the `Compose.txt` file shipped with
WinCompose, or by clicking on the WinCompose system tray icon or using the
“Show Sequences…” menu entry:

![Sequence List](/web/shot4.png)

The window allows you to filter the sequences being listed.

Features
--------

WinCompose supports the standard Compose file format. It ships with more than
1000 compose rules from [Xorg](http://www.x.org/wiki/).

WinCompose supports rules of more than 2 characters such as **( 3 )**
for **③**.

WinCompose supports early exits. For instance, **Compose &** will
immediately type **&** because there is currently no rule starting with **&**.

WinCompose does not emit unnecessary keystrokes, such as a backspace keystroke
to erase the composing characters. Only the final character is sent to the
current application.

Bugs
----

As of now, WinCompose does not support Unicode code points after U+FFFF.

Please report bugs to Sam Hocevar <sam@hocevar.net>

News for version 0.4.1 (6 May 2013)
--------------------------------------
 - “Menu”, “Escape” and “Backtick�� can now act as compose
   keys, too.
 - Improved the filtering logic.
 - Allow to choose the timeout delay from the context menu.

News for version 0.4.0 (4 May 2013)
--------------------------------------
 - “Pause” and “Scroll Lock” can now act as compose keys, too.
 - It is possible to filter sequences by keyword
 - The sequence window can be resized and is displayed with a simple
   click on the systray icon.
 - Cosmetic fixes in the GUI.

News for version 0.3.0 (26 April 2013)
--------------------------------------
 - The key used for composing is now customisable.

News for version 0.2.0 (24 April 2013)
--------------------------------------
 - Highly improved compatibility with games.
 - “Typing” and “Disabled” icons are now more easily told apart.

News for version 0.1.1 (23 April 2013)
--------------------------------------
 - Admin privileges are no longer required to start WinCompose at startup.

News for version 0.1 (22 April 2013)
------------------------------------
 - Initial release.

