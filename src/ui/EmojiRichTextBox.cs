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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace WinCompose
{
    public class ColorGlyph : Canvas
    {
        public ColorGlyph(ColorTypeface font, int codepoint)
        {
            m_font = font;
            m_codepoint = codepoint;
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            m_fontsize = ((Parent as InlineUIContainer).Parent as Emoji).FontSize;
            Width = m_fontsize * m_font.Widths[m_codepoint];
            Height = m_fontsize * m_font.Height;
        }

        protected override void OnRender(DrawingContext dc)
        {
            // Debug the bounding box
            //dc.DrawRectangle(Brushes.Bisque, new Pen(Brushes.LightCoral, 1.0), new Rect(0, 0, Width, Height));
            m_font.RenderGlyph(dc, m_codepoint, m_fontsize);
        }

        private ColorTypeface m_font;
        private double m_fontsize;
        private int m_codepoint;
    }

    // Inheriting from Span makes it easy to parse the tree for copy-paste
    public class Emoji : Span
    {
        static ColorTypeface m_font = new ColorTypeface("Segoe UI Emoji");

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
                int codepoint = StringToCodepoint(m_text);
                Inlines.Add(new ColorGlyph(m_font, codepoint));
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

