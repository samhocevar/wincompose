//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2021 Sam Hocevar <sam@hocevar.net>
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
    // FIXME: would a Logging.MessageReceived event be more elegant?
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

public static class Logging
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
            FileName = Path.Combine(Utils.LocalAppDataDir, "wincompose.log"),
            ConcurrentWrites = true,
            ArchiveEvery = FileArchivePeriod.Day,
            EnableArchiveFileCompression = true,
            ArchiveNumbering = ArchiveNumberingMode.Rolling,
            MaxArchiveFiles = 10,
        };
        config.AddTarget(file_target);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, file_target);

        var gui_target = new MethodCallTarget("MyTarget", Debug);
        config.AddTarget(gui_target);
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, gui_target);

        LogManager.Configuration = config;
    }

    public static LogList Entries { get; } = new LogList();

    public static void Debug(LogEventInfo lei, params object[] args)
    {
        // We don’t do anything unless we have listeners
        if (Entries.ListenerCount > 0)
        {
            ThreadPool.QueueUserWorkItem(x =>
                Entries.PreferredDispatcher.Invoke(DispatcherPriority.Background, DebugSTA, lei));
        }
    }

    private delegate void DebugDelegate(LogEventInfo lei);
    private static DebugDelegate DebugSTA = (lei) =>
    {
        var entry = new LogEntry() { DateTime = lei.TimeStamp, Message = lei.FormattedMessage };
        while (Entries.Count > 1024)
            Entries.RemoveAt(0);
        Entries.Add(entry);
    };
}

}
