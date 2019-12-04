//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2019 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Windows;
using System.Windows.Controls.Primitives;

namespace WinCompose
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        public Popup()
        {
            ShowInTaskbar = false;
            InitializeComponent();

            // Ensure OnKey() is called on our dispatcher thread
            var trigger = DispatcherTrigger.Create(OnKey);
            Loaded += (o, e) => Composer.Key += trigger;
            Closed += (o, e) => Composer.Key -= trigger;
        }

        private void OnKey()
        {
            Rect caret;
            if (!Composer.IsComposing || (caret = CaretControl.GetInfo(this)).IsEmpty)
            {
                Hide();
                return;
            }

            // Position popup near the cursor
            var ps = PresentationSource.FromVisual(this);
            var mat = ps.CompositionTarget.TransformFromDevice;
            var p1 = mat.Transform(new Point(caret.Left, caret.Top));
            var p2 = mat.Transform(new Point(caret.Right, caret.Bottom));
            Left = p1.X - 5;
            Top = p2.Y + 5;

            PopupText.Text = string.Format("({0}, {1}) {2}x{3}",
                    caret.Left, caret.Top, caret.Width, caret.Height);

            TestBorder.Placement = PlacementMode.Absolute;
            TestBorder.HorizontalOffset = p1.X;
            TestBorder.VerticalOffset = p1.Y;
            TestBorder.Width = p2.X - p1.X;
            TestBorder.Height = p2.Y - p1.Y;
            Show();
        }
    }
}
