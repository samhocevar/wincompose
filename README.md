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

Latest release: [WinCompose-Setup-0.2.0.exe](/WinCompose-Setup-0.2.0.exe)

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

The full list of rules can be found in the `Compose.txt` file shipped with WinCompose,
or using the “Show Sequences…” menu entry:

![Sequence List](/web/shot4.png)

Features
--------

WinCompose supports the standard Compose file format. It ships with more than
1000 compose rules from [Xorg](http://www.x.org/wiki/).

WinCompose supports rules of more than 2 characters such as **(** + **3** + **)**
for **③**.

WinCompose supports early exits. For instance, **Compose** + **&** will
immediately type **&** because there is currently no rule starting with **&**.

WinCompose does not emit unnecessary keystrokes, such as a backspace keystroke
to erase the composing characters. Only the final character is sent to the
current application.

Bugs
----

Customising the script requires AutoHotKey (http://www.autohotkey.com/) for now.
The original WinCompose.ahk script file is provided.

Please report bugs to Sam Hocevar <sam@hocevar.net>

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

