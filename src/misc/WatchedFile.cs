//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.IO;
using System.Threading;

namespace WinCompose
{
    class WatchedFile : IDisposable
    {
        public WatchedFile(string path)
        {
            FullPath = path;
            var dirname = Path.GetDirectoryName(path);
            var filename = Path.GetFileName(path);

            if (Utils.EnsureDirectory(dirname))
            {
                m_watcher = new FileSystemWatcher(dirname, filename);
                m_watcher.NotifyFilter = NotifyFilters.LastWrite;
                m_watcher.Changed += (o, e) =>
                {
                    // This event is triggered multiple times. We defer its
                    // handling so that it is not called more than once every
                    // 500 milliseconds.
                    Log.Debug($"File {filename} changed, scheduling reload.");
                    m_reload_timer.Change(500, Timeout.Infinite);
                };
                m_watcher.EnableRaisingEvents = true;
                m_reload_timer = new Timer(o => OnFileChanged?.Invoke());
            }
        }

        public readonly string FullPath;

        public event Action OnFileChanged;

        #region IDisposable Support
        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !m_disposed)
            {
                m_watcher?.Dispose();
                m_watcher = null;
                m_reload_timer?.Dispose();
                m_reload_timer = null;
            }

            m_disposed = true;
        }

        private bool m_disposed = false;
        #endregion

        private FileSystemWatcher m_watcher;
        private Timer m_reload_timer;
    }
}

