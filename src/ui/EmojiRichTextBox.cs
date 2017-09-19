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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WinCompose
{
    public class FilteredImage : Image
    {
        // Override scaling algorithm for better quality
        protected override void OnRender(DrawingContext dc)
        {
            VisualBitmapScalingMode = BitmapScalingMode.HighQuality;
            base.OnRender(dc);
        }
    }

    // Inheriting from Span makes it easy to parse the tree for copy-paste
    public class Emoji : Span
    {
        public Emoji(string name, string alt, double font_size)
        {
            FilteredImage image = new FilteredImage();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/res/" + name));
            image.Stretch = Stretch.Uniform;
            image.Width = image.Height = font_size * 1.25;
            Inlines.Add(image);

            Text = alt;
        }

        // Need an empty constructor for serialisation (undo/redo)
        public Emoji() {}

        public readonly string Text;
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

            //e.DataObject.SetData(DataFormats.Text, clipboard);
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
        }

        private Dictionary<string, string> m_emojis = new Dictionary<string, string>()
        {
            { "☺", "emoji_test_1.png" },
            { "⸚", "emoji_test_2.png" },
            { "☹", "emoji_test_3.png" },
        };

        private bool m_pending_change = false;

        private void FixEmojis()
        {
            if (m_pending_change)
                return;

            // This will prevent our operation from polluting the undo buffer, but it
            // will create an infinite undo stack... need to fix this.
            BeginChange();

            m_pending_change = true;

            TextPointer cur = Document.ContentStart;
            while (cur.CompareTo(Document.ContentEnd) < 0)
            {
                TextPointer next = cur.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next == null)
                    break;

                var word = new TextRange(cur, next);
                string filename;
                if (m_emojis.TryGetValue(word.Text, out filename))
                    next = ReplaceWithImage(word, filename);

                cur = next;
            }

            EndChange();

            m_pending_change = false;
        }

        public TextPointer ReplaceWithImage(TextRange range, string name)
        {
            var parent = range.Start.Parent as Run;

            if (parent == null || range.Start.Paragraph == null)
                return range.End;

            var before = new TextRange(parent.ContentStart, range.Start).Text;
            var after = new TextRange(range.End, parent.ContentEnd).Text;
            var inlines = range.Start.Paragraph.Inlines;

            /* Insert new inlines in reverse order after the parent */
            if (!string.IsNullOrEmpty(after))
                inlines.InsertAfter(parent, new Run(after));

            inlines.InsertAfter(parent, new Emoji(name, range.Text, FontSize));
            inlines.LastInline.BaselineAlignment = BaselineAlignment.Center;

            //inlines.Add(new Run(range.Text));
            //inlines.LastInline.IsEnabled = false;

            if (!string.IsNullOrEmpty(before))
                inlines.InsertAfter(parent, new Run(before));

            TextPointer ret = parent.ContentStart;
            inlines.Remove(parent);
            return ret;
        }
    }
}

