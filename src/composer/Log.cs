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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WinCompose
{

public class PropertyChangedBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class LogEntry : PropertyChangedBase
{
    public DateTime DateTime { get; set; }
    public string Message { get; set; }
}

public static class Log
{
    private static ObservableCollection<LogEntry> m_logs = new ObservableCollection<LogEntry>();
    public static ObservableCollection<LogEntry> Entries => m_logs;

    public static void Debug(string format, params object[] args)
    {
        DateTime date = DateTime.Now;
        string msg = string.Format(format, args);

        while (m_logs.Count > 100)
            m_logs.RemoveAt(0);
        m_logs.Add(new LogEntry() { DateTime = date, Message = msg });

#if DEBUG
        string msgf = string.Format("{0:yyyy/MM/dd HH:mm:ss.fff} {1}", date, msg);
        System.Diagnostics.Debug.WriteLine(msgf);
#endif
    }
}

}
