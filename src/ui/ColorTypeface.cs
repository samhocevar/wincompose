//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2017 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace WinCompose
{
    public class ColorTypeface : Typeface
    {
        public ColorTypeface(string name)
          : base(name)
        {
            if (TryGetGlyphTypeface(out m_gtf))
            {
                using (var s = m_gtf.GetFontStream())
                {
                    ReadFontStream(s);
                }
            }
        }

        public void RenderGlyph(DrawingContext dc, int codepoint, double size, Point pos)
        {
            ushort gid = m_gtf.CharacterToGlyphMap[codepoint];
            int start = m_layer_indices[gid], stop = start + m_layer_counts[gid];

            Random rand = new Random();

            for (int i = start; i < stop; ++i)
            {
                GlyphRun r = new GlyphRun(m_gtf, 0, false, size, // pt size
                                          new ushort[] { m_glyph_layers[i] },
                                          pos, // position
                                          new double[] { 0 }, // advance
                                          null, null, null, // FIXME: check what this is?
                                          null, null, null);
                int palette = 0; // FIXME: support multiple palettes?
                Brush b = new SolidColorBrush(m_colors[m_palettes[palette] + m_glyph_palettes[i]]);

                dc.DrawGlyphRun(b, r);
            }
        }

        public bool HasCodepoint(int codepoint)
        {
            // Check that the character is mapped to a glyph, and that the glyph
            // has colour information. Otherwise, we are not interested.
            ushort glyph;
            return m_gtf.CharacterToGlyphMap.TryGetValue(codepoint, out glyph)
                    && m_layer_indices.ContainsKey(glyph);
        }

        /// <summary>
        /// Big-endian binary reader for TrueType/OTF fonts.
        /// </summary>
        private class BEBinaryReader : BinaryReader
        {
            public BEBinaryReader(Stream s) : base(s) {}

            public override short  ReadInt16()  => BitConverter.ToInt16(ReadReverse(2), 0);
            public override ushort ReadUInt16() => BitConverter.ToUInt16(ReadReverse(2), 0);
            public override int    ReadInt32()  => BitConverter.ToInt32(ReadReverse(4), 0);
            public override uint   ReadUInt32() => BitConverter.ToUInt32(ReadReverse(4), 0);
            public override long   ReadInt64()  => BitConverter.ToInt64(ReadReverse(8), 0);
            public override ulong  ReadUInt64() => BitConverter.ToUInt64(ReadReverse(8), 0);

            private byte[] ReadReverse(int count)
            {
                var b = ReadBytes(count); Array.Reverse(b); return b;
            }
        }

        private void ReadFontStream(Stream s)
        {
            int colr_offset = -1, colr_length = -1;
            int cpal_offset = -1, cpal_length = -1;

            // Read font header
            var b = new BEBinaryReader(s);
            uint header = b.ReadUInt32();
            int table_count = b.ReadUInt16();
            b.BaseStream.Seek(6, SeekOrigin.Current);

            // Read available table information
            for (int i = 0; i < table_count; ++i)
            {
                int table_header = b.ReadInt32();
                int table_checksum = b.ReadInt32();
                int table_offset = b.ReadInt32();
                int table_length = b.ReadInt32();

                if (table_header == 0x434f4c52) // COLR
                {
                    colr_offset = table_offset;
                    colr_length = table_length;
                }
                else if (table_header == 0x4350414c) // CPAL
                {
                    cpal_offset = table_offset;
                    cpal_length = table_length;
                }
            }

            if (colr_offset != -1 && cpal_offset != -1)
            {
                // Read the COLR table
                b.BaseStream.Seek(colr_offset, SeekOrigin.Begin);
                ushort version = b.ReadUInt16();
                int glyph_count = b.ReadUInt16();
                int glyphs_offset = b.ReadInt32();
                int layers_offset = b.ReadInt32();
                int layer_count = b.ReadUInt16();

                b.BaseStream.Seek(colr_offset + glyphs_offset, SeekOrigin.Begin);
                for (int i = 0; i < glyph_count; ++i)
                {
                    ushort gid = b.ReadUInt16();
                    m_layer_indices[gid] = b.ReadUInt16();
                    m_layer_counts[gid] = b.ReadUInt16();
                }

                b.BaseStream.Seek(colr_offset + layers_offset, SeekOrigin.Begin);
                m_glyph_layers = new ushort[layer_count];
                m_glyph_palettes = new ushort[layer_count];
                for (int i = 0; i < layer_count; ++i)
                {
                    m_glyph_layers[i] = b.ReadUInt16();
                    m_glyph_palettes[i] = b.ReadUInt16();
                }

                // Read the CPAL table
                b.BaseStream.Seek(cpal_offset, SeekOrigin.Begin);
                ushort palette_version = b.ReadUInt16();
                int entry_count = b.ReadUInt16();
                int palette_count = b.ReadUInt16();
                int color_count = b.ReadUInt16();
                int colors_offset = b.ReadInt32();

                m_palettes = new ushort[palette_count];
                for (int i = 0; i < palette_count; ++i)
                {
                    m_palettes[i] = b.ReadUInt16();
                }

                b.BaseStream.Seek(cpal_offset + colors_offset, SeekOrigin.Begin);
                m_colors = new Color[color_count];
                for (int i = 0; i < color_count; ++i)
                {
                    byte[] tmp = b.ReadBytes(4);
                    m_colors[i] = Color.FromArgb(tmp[3], tmp[2], tmp[1], tmp[0]);
                }
            }
        }

        private GlyphTypeface m_gtf;

        private Dictionary<ushort, ushort> m_layer_indices = new Dictionary<ushort, ushort>();
        private Dictionary<ushort, ushort> m_layer_counts = new Dictionary<ushort, ushort>();
        private ushort[] m_glyph_layers = new ushort[0];
        private ushort[] m_glyph_palettes = new ushort[0];
        private ushort[] m_palettes = new ushort[0];
        private Color[] m_colors = new Color[0];
    }
}

