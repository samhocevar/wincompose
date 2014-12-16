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
