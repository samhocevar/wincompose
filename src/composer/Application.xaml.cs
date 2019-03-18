//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2018 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Windows.Threading;

namespace WinCompose
{
    /// <summary>
    /// Interaction logic for WinCompose.xaml
    /// </summary>
    public partial class Application : System.Windows.Application
    {
        public Application()
        {
            InitializeComponent();
        }

        protected void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show(e.Exception.ToString(), "Error");
        }

        public static RemoteControl RemoteControl => (Current as Application).RC;
    }
}
