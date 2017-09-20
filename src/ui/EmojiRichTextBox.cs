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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WinCompose
{
    /// <summary>
    /// A simple Image class with a better quality scaling algorithm.
    /// </summary>
    public class FilteredImage : Image
    {
        protected override void OnRender(DrawingContext dc)
        {
            VisualBitmapScalingMode = BitmapScalingMode.HighQuality;
            base.OnRender(dc);
        }
    }

    // Inheriting from Span makes it easy to parse the tree for copy-paste
    public class Emoji : Span
    {
        // Need an empty constructor for serialisation (undo/redo)
        public Emoji() {}

        public Emoji(string name, string alt)
        {
            BaselineAlignment = BaselineAlignment.Center;
            Text = alt;
        }

        public static Emoji MakeFromString(string text)
        {
            string filename;
            return m_emojis.TryGetValue(text, out filename) ? new Emoji(filename, text) : null;
        }

        // Do not serialize our child element, as it is only for rendering
        protected new bool ShouldSerializeInlines(XamlDesignerSerializationManager m) => false;

        public string Text
        {
            get => m_text;
            set
            {
                m_text = value;
                FilteredImage image = new FilteredImage();
                image.Source = new BitmapImage(new Uri("pack://application:,,,/res/" + m_emojis[value]));
                image.Stretch = Stretch.Uniform;
                image.Width = image.Height = FontSize * 1.5;
                Inlines.Add(image);
            }
        }

        private string m_text;

        private static Dictionary<string, string> m_emojis = new Dictionary<string, string>()
        {
            { "☺", "emoji_test_1.png" },
            { "⸚", "emoji_test_2.png" },
            { "☹", "emoji_test_3.png" },
        };
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

