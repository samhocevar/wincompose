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

using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace WinCompose
{
    // FIXME: deprecate this class once it is properly exposed by Emoji.Wpf
    internal class GZipResourceStream : StreamReader
    {
        public GZipResourceStream(string name)
          : base(new GZipStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(name),
                                CompressionMode.Decompress))
        {
        }

        protected override void Dispose(bool disposing)
        {
            var gzip_stream = BaseStream as GZipStream;
            var resource_stream = gzip_stream?.BaseStream as Stream;

            base.Dispose(disposing);

            if (!m_disposed)
            {
                if (disposing)
                {
                    gzip_stream.Dispose();
                    resource_stream.Dispose();
                }

                m_disposed = true;
            }
        }

        private bool m_disposed = false;
    }
}
