using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace wincompose.gui
{
    /// <summary>
    /// Interaction logic for mainwindow.xaml
    /// </summary>
    public partial class mainwindow : Window
    {
        public mainwindow()
        {
            InitializeComponent();
        }

        // Code from WinForms
        //        public gui()
        //{
        //    InitializeComponent();

        //    // Testing crap for now
        //    webbrowser.DocumentText = @"<h1 style=""text-decoration: underline;"">U+2603 ☃</h1><p>Description: SNOWMAN</p><p><span style=""font-size: 280px; text-align: center;"">☃</span></p>";
        //}

        //private void showSequencesToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (this.WindowState == FormWindowState.Minimized)
        //        this.WindowState = FormWindowState.Normal;

        //    this.Activate();
        //    this.Show();
        //}

        //private void notifyicon_MouseClick(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        if (this.Visible)
        //            this.Hide();
        //        else
        //        {
        //            if (this.WindowState == FormWindowState.Minimized)
        //                this.WindowState = FormWindowState.Normal;
        //            this.Activate();
        //            this.Show();
        //        }
        //    }
        //}

        //private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (this.WindowState == FormWindowState.Minimized)
        //        this.WindowState = FormWindowState.Normal;

        //    this.Activate();
        //    this.Show();
        //}

        //private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (m_about == null)
        //        m_about = new about();

        //    m_about.Activate();
        //    m_about.Show();
        //}

        //private about m_about;

    }
}
