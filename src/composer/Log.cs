//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2015 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Collections.Generic;

namespace WinCompose
{

static class Log
{
    public static void Debug(string format, params object[] args)
    {
#if DEBUG
        string msg = string.Format("{0:yyyy/MM/dd HH:mm:ss.fff} {1}",
                                   DateTime.Now, string.Format(format, args));
        System.Diagnostics.Debug.WriteLine(msg);
#endif
    }
}

}
