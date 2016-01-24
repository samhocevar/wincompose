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
using System.ComponentModel;
using System.Windows.Input;

namespace WinCompose
{
    /// <summary>
    /// A minimal implementation of the <see cref="INotifyPropertyChanged"/> interface
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action<Object> m_execute_handler;

        public DelegateCommand(Action<Object> execute_handler)
        {
            m_execute_handler = execute_handler;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged = delegate {};

        public void Execute(object parameter)
        {
            m_execute_handler(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged(this, new EventArgs());
        }
    }
}
