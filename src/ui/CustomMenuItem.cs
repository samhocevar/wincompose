//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2016 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System.Drawing;
using System.Drawing.Drawing2D;
using WinForms = System.Windows.Forms;

namespace WinCompose
{
    // Prevent Visual Studio from thinking this is a Designer class
    class Dummy {}

    class CustomMenuItem : WinForms.MenuItem
    {
        public CustomMenuItem()
        {
            OwnerDraw = true;
        }

        protected override void OnMeasureItem(WinForms.MeasureItemEventArgs e)
        {
            var size = WinForms.TextRenderer.MeasureText(Text, TextFont);
            e.ItemWidth = size.Width;
            e.ItemHeight = size.Height;
        }

        protected override void OnDrawItem(WinForms.DrawItemEventArgs e)
        {
            e.DrawBackground();

            var brush = SystemBrushes.Menu;
            if (Gradient)
                brush = new LinearGradientBrush(e.Bounds, Color.SkyBlue,
                                                Color.White, 30.0f);
            e.Graphics.FillRectangle(brush, e.Bounds);

            if (MenuIcon != null)
            {
                var icon_bounds = e.Bounds;
                icon_bounds.Width = e.Bounds.Height;
                e.Graphics.DrawIcon(Program.GetIcon(0), icon_bounds);
            }

            var text_bounds = e.Bounds;
            text_bounds.Width -= e.Bounds.Height;
            text_bounds.X += e.Bounds.Height;
            e.Graphics.DrawString(Text, TextFont, Brushes.Black, text_bounds);
        }

        public Font TextFont
        {
            get
            {
                return new Font(SystemFonts.MenuFont.FontFamily,
                                SystemFonts.MenuFont.Size * Scale,
                                Bold ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        public Icon MenuIcon = null;
        public float Scale = 1.0f;
        public bool Gradient = false;
        public bool Bold = false;
    }
}

