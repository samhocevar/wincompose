//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2021 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.IO;
using System.Xml.Serialization;

namespace WinCompose
{
    class XmlFile : WatchedFile
    {
        public XmlFile(string path)
          : base(path)
        { }

        public T Load<T>() where T: class, new()
        {
            try
            {
                if (!File.Exists(FullPath))
                    return new T();
                var xs = new XmlSerializer(typeof(T));
                using (TextReader tr = new StreamReader(FullPath))
                    return xs.Deserialize(tr) as T;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to load {FullPath}");
            }

            return new T();
        }

        public void Save<T>(T o) where T: class
        {
            try
            {
                Utils.EnsureDirectory(Utils.AppDataDir);
                var xs = new XmlSerializer(typeof(T));
                using (TextWriter tw = new StreamWriter(FullPath))
                    xs.Serialize(tw, o);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, $"Failed to save {FullPath}");
            }
        }

        private static NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
    }
}
