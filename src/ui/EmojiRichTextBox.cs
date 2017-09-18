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
    public class EmojiImage : Image
    {
        public EmojiImage(string name, double font_size)
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/res/" + name));
            Stretch = Stretch.Uniform;
            Width = Height = font_size * 1.25;
            //Margin = new System.Windows.Thickness(0, font_size * 0.5, 0, 0);
        }

        // Override scaling algorithm for better quality
        protected override void OnRender(DrawingContext dc)
        {
            VisualBitmapScalingMode = BitmapScalingMode.HighQuality;
            base.OnRender(dc);
        }
    }

    public class EmojiRichTextBox : RichTextBox
    {
        public EmojiRichTextBox()
        {
            SetValue(Block.LineHeightProperty, 1.0);
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);
#if FALSE
            string clip = "";
            var tp = Selection.Start;
            while (tp != null && tp.CompareTo(Selection.End) < 0)
            {
                //Console.WriteLine(tp.CompareTo(Selection.End));
                var tp2 = tp.GetNextInsertionPosition(LogicalDirection.Forward);
                if (tp2 == null)
                    break;
                clip += new TextRange(tp, tp2).Text;
                tp = tp2;
            }
            //Console.WriteLine(clip);
#endif
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

            m_pending_change = true;

            TextPointer cur = Document.ContentStart;
            while (cur.CompareTo(Document.ContentEnd) < 0)
            {
                TextPointer next = cur.GetNextInsertionPosition(LogicalDirection.Forward);
                if (next == null)
                    break;

                var word = new TextRange(cur, next);
                string image;
                if (m_emojis.TryGetValue(word.Text, out image))
                    cur = ReplaceWithImage(word, image);
                else
                    cur = next;
            }

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

            if (!string.IsNullOrEmpty(before))
                inlines.Add(new Run(before));

            //inlines.Add(new Run(range.Text));
            //inlines.LastInline.IsEnabled = false;

            inlines.Add(new EmojiImage(name, FontSize));
            inlines.LastInline.BaselineAlignment = BaselineAlignment.Center;

            if (!string.IsNullOrEmpty(after))
                inlines.Add(new Run(after));

            inlines.Remove(parent);

            return inlines.LastInline.ContentEnd;
        }
    }
}

