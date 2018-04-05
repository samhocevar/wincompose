//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2018 Sam Hocevar <sam@hocevar.net>
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;

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
    // Override the CollectionChanged event so that we can track listeners and call
    // their delegates in the correct thread.
    // FIXME: would a Log.MessageReceived event be more elegant?
    public override event NotifyCollectionChangedEventHandler CollectionChanged
    {
        add
        {
            if (!m_listeners.ContainsKey(value))
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                var handler = new NotifyCollectionChangedEventHandler((o, e) => dispatcher.Invoke(value, o, e));
                m_listeners[value] = handler;
                base.CollectionChanged += handler;
            }
        }

        remove
        {
            NotifyCollectionChangedEventHandler handler;
            if (m_listeners.TryGetValue(value, out handler))
            {
                base.CollectionChanged -= handler;
                m_listeners.Remove(value);
            }
        }
    }

    public int ListenerCount => m_listeners.Count;

    private Dictionary<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventHandler> m_listeners
        = new Dictionary<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventHandler>();
}

public static class Log
{
    private static LogList m_entries = new LogList();
    public static LogList Entries => m_entries;

#if DEBUG
    static Log()
    {
        Entries.CollectionChanged += ConsoleDebug;
    }

    private static void ConsoleDebug(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (LogEntry entry in e.NewItems)
            {
                string msgf = $"{entry.DateTime:yyyy/MM/dd HH:mm:ss.fff} {entry.Message}";
                System.Diagnostics.Debug.WriteLine(msgf);
            }
        }
    }
#endif

    public static void Debug(string format, params object[] args)
    {
        // We don’t do anything unless we have listeners
        if (m_entries.ListenerCount > 0)
        {
            DateTime date = DateTime.Now;
            string msg = string.Format(format, args);

            while (m_entries.Count > 1024)
                m_entries.RemoveAt(0);
            Entries.Add(new LogEntry() { DateTime = date, Message = msg });
        }
    }
}

}
