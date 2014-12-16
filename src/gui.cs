// Copyright: (c) 2013-2014 Sam Hocevar <sam@hocevar.net>
//   This program is free software; you can redistribute it and/or
//   modify it under the terms of the Do What The Fuck You Want To
//   Public License, Version 2, as published by the WTFPL Task Force.
//   See http://www.wtfpl.net/ for more details.using System;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace wincompose
{
    public partial class gui : Form
    {
        public gui()
        {
            InitializeComponent();

            // Testing crap for now
            webbrowser.DocumentText = @"<h1 style=""text-decoration: underline;"">U+2603 ☃</h1><p>Description: SNOWMAN</p><p><span style=""font-size: 280px; text-align: center;"">☃</span></p>";
        }

        private void showSequencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            this.Activate();
            this.Show();
        }

        private void notifyicon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.Visible)
                    this.Hide();
                else
                {
                    if (this.WindowState == FormWindowState.Minimized)
                        this.WindowState = FormWindowState.Normal;
                    this.Activate();
                    this.Show();
                }
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            this.Activate();
            this.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_about == null)
                m_about = new about();

            m_about.Activate();
            m_about.Show();
        }

        private about m_about;
    }
}
