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

using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace WinCompose
{

public class LogEntry
{
    public DateTime DateTime { get; set; }
    public string Message { get; set; }
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
            if (Dispatcher.CurrentDispatcher.Thread.GetApartmentState() == System.Threading.ApartmentState.STA)
                PreferredDispatcher = Dispatcher.CurrentDispatcher;
            ListenerCount += value?.GetInvocationList().Length ?? 0;
            base.CollectionChanged += value;
        }

        remove
        {
            base.CollectionChanged -= value;
            ListenerCount -= value?.GetInvocationList().Length ?? 0;
        }
    }

    public Dispatcher PreferredDispatcher = Dispatcher.CurrentDispatcher;
    public int ListenerCount { get; set; }
}

public static class Log
{
    public static void Init()
    {
        LoggingConfiguration config = new LoggingConfiguration();
        string log_fmt = "${date:format=yyyy/MM/dd HH\\:mm\\:ss\\:fff} [${processid}] ${level}: "
                       + "${message}${onexception:${newline}}${exception:format=tostring}";

#if DEBUG
        var dbg_target = new DebuggerTarget("Debugger") { Layout = log_fmt };
        config.AddTarget(dbg_target);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, dbg_target);
#endif

        var console_target = new ConsoleTarget("Console") { Layout = log_fmt };
        config.AddTarget(console_target);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, console_target);

        var file_target = new FileTarget("File")
        {
            Layout = log_fmt,
            FileName = Path.Combine(Utils.AppDataDir, "wincompose.log"),
            ConcurrentWrites = true,
            ArchiveEvery = FileArchivePeriod.Day,
            EnableArchiveFileCompression = true,
            ArchiveNumbering = ArchiveNumberingMode.Rolling,
            MaxArchiveFiles = 10,
        };
        config.AddTarget(file_target);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, file_target);

        LogManager.Configuration = config;
    }

    private static ILogger Logger = LogManager.GetCurrentClassLogger();

    private static LogList m_entries = new LogList();
    public static LogList Entries => m_entries;

    public static void Info(string msg) => Info("{0}", msg);

    public static void Info(string format, params object[] args) => Logger.Info(format, args);

    public static void Warn(string msg) => Warn("{0}", msg);

    public static void Warn(string format, params object[] args) => Logger.Warn(format, args);

    public static void Debug(string msg) => Debug("{0}", msg);

    public static void Debug(string format, params object[] args)
    {
        Logger.Debug(format, args);

        // We don’t do anything unless we have listeners
        if (m_entries.ListenerCount > 0)
        {
            DateTime date = DateTime.Now;
            var msg = string.Format(format, args);
            ThreadPool.QueueUserWorkItem(x =>
            {
                m_entries.PreferredDispatcher.Invoke(DispatcherPriority.Background, DebugSTA, date, msg);
            });
        }
    }

    private delegate void DebugDelegate(DateTime date, string msg);
    private static DebugDelegate DebugSTA = (date, msg) =>
    {
        var entry = new LogEntry() { DateTime = date, Message = msg };
        while (m_entries.Count > 1024)
            m_entries.RemoveAt(0);
        m_entries.Add(entry);
    };
}

}
