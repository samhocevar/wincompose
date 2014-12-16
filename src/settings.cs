// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//   This program is free software; you can redistribute it and/or
//   modify it under the terms of the Do What The Fuck You Want To
//   Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.using System;

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace wincompose
{
    class settings
    {
        public static string get_config_dir()
        {
            return is_portable() ? "." : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        public static string get_data_dir()
        {
            return is_portable() ? "." : get_exe_dir();
        }

        public static string get_exe_dir()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(assembly.GetName().CodeBase);
        }

        public static bool is_portable()
        {
            return !File.Exists(Path.Combine(get_exe_dir(), "unins000.dat"));
        }
    }
}
