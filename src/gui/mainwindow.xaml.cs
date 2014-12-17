using System;
using System.Windows;
using WinForms = System.Windows.Forms;
namespace WinCompose.gui
{
    /// <summary>
    /// Interaction logic for mainwindow.xaml
    /// </summary>
    public partial class mainwindow
    {
        public mainwindow()
        {
            InitializeComponent();
            var notifyicon = new WinForms.NotifyIcon
            {
                Visible = true,
                Icon = properties.resources.icon_normal
            };

            notifyicon.DoubleClick += notifyicon_doubleclicked;
            close_to_tray();
        }

        private void notifyicon_doubleclicked(object sender, EventArgs e)
        {
            if (!IsVisible)
            {
                open_from_tray();
            }
            else
            {
                close_to_tray();
            }
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            close_to_tray();
        }
        
        private void open_from_tray()
        {
            ShowInTaskbar = true;
            Show();
            Activate();
        }

        private void close_to_tray()
        {
            ShowInTaskbar = false;
            Hide();
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
