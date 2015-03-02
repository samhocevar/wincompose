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

namespace WinCompose
{
    /// <summary>
    /// Identifies a page of the GUI.
    /// </summary>
    public enum GuiPage
    {
        /// <summary>
        /// Identifies an empty page.
        /// </summary>
        None,

        /// <summary>
        /// Identifies the page displaying sequences.
        /// </summary>
        Sequences,

        /// <summary>
        /// Identifies the page displaying settings.
        /// </summary>
        Settings,
    }
}
