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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace WinCompose
{
    public class BEBinaryReader : BinaryReader
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

    public class ColorGlyph : Canvas
    {
        public ColorGlyph(ColorFont font, int codepoint)
        {
            m_font = font;
            m_codepoint = codepoint;
        }

        protected override void OnRender(DrawingContext dc)
        {
            m_font.RenderGlyph(dc, m_codepoint, Width);
        }

        private ColorFont m_font;
        private int m_codepoint;
    }

    public class ColorFont
    {
        public ColorFont(string name)
        {
            Typeface tf = new Typeface(name);
            if (tf.TryGetGlyphTypeface(out m_gtf))
            {
                using (var s = m_gtf.GetFontStream())
                {
                    ReadFontStream(s);
                }
            }
        }

        public void RenderGlyph(DrawingContext dc, int codepoint, double size)
        {
            ushort gid = m_gtf.CharacterToGlyphMap[codepoint];
            int start = m_layer_indices[gid], stop = start + m_layer_counts[gid];

            Random rand = new Random();

            for (int i = start; i < stop; ++i)
            {
                GlyphRun r = new GlyphRun(m_gtf, 0, false, size, // pt size
                                          new ushort[] { m_glyph_layers[i] },
                                          new Point(-5, 22),
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

    // Inheriting from Span makes it easy to parse the tree for copy-paste
    public class Emoji : Span
    {
        static ColorFont m_font = new ColorFont("Segoe UI Emoji");

        // Need an empty constructor for serialisation (undo/redo)
        public Emoji() {}

        public Emoji(string alt)
        {
            BaselineAlignment = BaselineAlignment.Center;
            Text = alt;
        }

        public static Emoji MakeFromString(string s)
        {
            int codepoint = StringToCodepoint(s);
            return m_font.HasCodepoint(codepoint) ? new Emoji(s) : null;
        }

        private static int StringToCodepoint(string s)
        {
            if (s.Length >= 2 && s[0] >= 0xd800 && s[0] <= 0xdbff)
                return Char.ConvertToUtf32(s[0], s[1]);
            return s.Length == 0 ? 0 : s[0];
        }

        // Do not serialize our child element, as it is only for rendering
        protected new bool ShouldSerializeInlines(XamlDesignerSerializationManager m) => false;

        public string Text
        {
            get => m_text;
            set
            {
                m_text = value;

                ColorGlyph g = new ColorGlyph(m_font, StringToCodepoint(m_text));
                g.Width = g.Height = FontSize * 1.5;
                Inlines.Add(g);
            }
        }

        private string m_text;
    }

    public class EmojiRichTextBox : RichTextBox
    {
        public EmojiRichTextBox()
        {
            SetValue(Block.LineHeightProperty, 1.0);
            DataObject.AddCopyingHandler(this, new DataObjectCopyingEventHandler(OnCopy));
        }

        protected void OnCopy(object o, DataObjectCopyingEventArgs e)
        {
            string clipboard = "";

            for (TextPointer p = Selection.Start, next = null;
                 p != null && p.CompareTo(Selection.End) < 0;
                 p = next)
            {
                next = p.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next == null)
                    break;

                //var word = new TextRange(p, next);
                //Console.WriteLine("Word '{0}' Inline {1}", word.Text, word.Start.Parent is Emoji ? "Emoji" : "not Emoji");
                //Console.WriteLine(" ... p {0}", p.Parent is Emoji ? "Emoji" : p.Parent.GetType().ToString());

                var t = new TextRange(p, next);
                clipboard += t.Start.Parent is Emoji ? (t.Start.Parent as Emoji).Text
                                                     : t.Text;
            }

            Clipboard.SetText(clipboard);
            e.Handled = true;
            e.CancelCommand();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Send);
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += new EventHandler(delegate { timer.Stop(); FixEmojis(); });
            timer.Start();

            base.OnTextChanged(e);

            // FIXME: debug
            //Console.WriteLine(XamlWriter.Save(Document));
        }

        private bool m_pending_change = false;

        private void FixEmojis()
        {
            if (m_pending_change)
                return;

            /* This will prevent our operation from polluting the undo buffer, but it
             * will create an infinite undo stack... need to fix this. */
            BeginChange();

            m_pending_change = true;

            TextPointer cur = Document.ContentStart;
            while (cur.CompareTo(Document.ContentEnd) < 0)
            {
                TextPointer next = cur.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next == null)
                    break;

                TextRange word = new TextRange(cur, next);
                Emoji emoji = Emoji.MakeFromString(word.Text);
                if (emoji != null)
                    next = Replace(word, emoji);

                cur = next;
            }

            EndChange();

            m_pending_change = false;
        }

        public TextPointer Replace(TextRange range, Emoji emoji)
        {
            var run = range.Start.Parent as Run;
            if (run == null)
                return range.End;

            var before = new TextRange(run.ContentStart, range.Start).Text;
            var after = new TextRange(range.End, run.ContentEnd).Text;
            var inlines = run.SiblingInlines;

            /* Insert new inlines in reverse order after the run */
            if (!string.IsNullOrEmpty(after))
                inlines.InsertAfter(run, new Run(after));

            inlines.InsertAfter(run, emoji);

            if (!string.IsNullOrEmpty(before))
                inlines.InsertAfter(run, new Run(before));

            TextPointer ret = emoji.ContentEnd; // FIXME
            inlines.Remove(run);
            return ret;
        }
    }
}

