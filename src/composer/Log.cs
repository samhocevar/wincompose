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
using System.Collections.Specialized;
using System.ComponentModel;

namespace WinCompose
{

public class LogEntry : INotifyPropertyChanged
{
    public DateTime DateTime { get; set; }
    public string Message { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class LogList : ObservableCollection<LogEntry>
{
    // Override the CollectionChanged event so that we can track listeners.
    // FIXME: would a Log.MessageReceived event be more elegant?
    public override event NotifyCollectionChangedEventHandler CollectionChanged
    {
        add { ListenerCount += value?.GetInvocationList().Length ?? 0; base.CollectionChanged += value; }
        remove { ListenerCount -= value?.GetInvocationList().Length ?? 0; base.CollectionChanged -= value; }
    }

    public int ListenerCount { get; private set; }
}

public static class Log
{
    private static LogList m_entries = new LogList();
    public static LogList Entries => m_entries;

    public static void Debug(string format, params object[] args)
    {
#if !DEBUG
        // In release mode, we don’t do anything unless we have listeners
        if (m_entries.ListenerCount == 0)
            return;
#endif

        DateTime date = DateTime.Now;
        string msg = string.Format(format, args);

        while (m_entries.Count > 512)
            m_entries.RemoveAt(0);
        Entries.Add(new LogEntry() { DateTime = date, Message = msg });

#if DEBUG
        string msgf = string.Format("{0:yyyy/MM/dd HH:mm:ss.fff} {1}", date, msg);
        System.Diagnostics.Debug.WriteLine(msgf);
#endif
    }
}

}
