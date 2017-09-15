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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        private int zob = 0;

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (zob++ <= 0)
                FixEmojis();

            base.OnTextChanged(e);
            --zob;
        }

        private void FixEmojis()
        {
            var tp = Document.ContentStart;
            while (tp.GetNextInsertionPosition(LogicalDirection.Forward) != null)
            {
                var tp2 = tp.GetNextInsertionPosition(LogicalDirection.Forward);
                var word = new TextRange(tp, tp2);
                if (word.Text == "☺")
                    ReplaceWithImage(word, "emoji_test_1.png");
                else if (word.Text == "⸚")
                    ReplaceWithImage(word, "emoji_test_2.png");
                else if (word.Text == "☹")
                    ReplaceWithImage(word, "emoji_test_3.png");

                tp = tp2;
            }
        }

       public void ReplaceWithImage(TextRange range, string name)
        {
            if (!(range.Start.Parent is Run) || range.Start.Paragraph == null)
                return;

            var parent = range.Start.Parent as Run;
            var before = new Run(new TextRange(parent.ContentStart, range.Start).Text);
            var after = new Run(new TextRange(range.End, parent.ContentEnd).Text);

            range.Start.Paragraph.Inlines.Remove(parent);
            range.Start.Paragraph.Inlines.Add(before);
            range.Start.Paragraph.Inlines.Add(new EmojiImage(name, FontSize));
            range.Start.Paragraph.Inlines.Add(after);

            CaretPosition = after.ContentEnd;
        }
    }
}

