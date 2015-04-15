//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2015 Sam Hocevar <sam@hocevar.net>
//              2014—2015 Benjamin Litzelmann
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using WinForms = System.Windows.Forms;

namespace WinCompose
{
    static class Program
    {
        private static WinForms.NotifyIcon m_tray_icon;
        private static WinForms.MenuItem m_disable_item;
        private static InterProcessMessaging m_messaging;
        private static SequenceWindow m_sequencewindow;
        private static SettingsWindow m_optionswindow;

        [STAThread]
        static void Main()
        {
            Composer.Init();
            Settings.LoadConfig();
            Settings.LoadSequences();
            KeyboardHook.Init();

            Settings.StartWatchConfigFile();

            try
            {
                WinForms.Application.EnableVisualStyles();
                WinForms.Application.SetCompatibleTextRenderingDefault(false);

                m_messaging = new InterProcessMessaging();
                m_messaging.DisableEvent += DisableReceived;
                m_messaging.TriggerDisableEvent();

                m_disable_item = new WinForms.MenuItem(i18n.Text.Disable, DisableClicked);

                m_tray_icon = new WinForms.NotifyIcon
                {
                    Visible = true,
                    Icon = Properties.Resources.IconNormal,
                    ContextMenu = new WinForms.ContextMenu(new[]
                    {
                        new WinForms.MenuItem("WinCompose")
                        {
                            Enabled = false
                        },
                        new WinForms.MenuItem("-"),
                        new WinForms.MenuItem(i18n.Text.ShowSequences, ShowSequencesClicked),
                        new WinForms.MenuItem(i18n.Text.ShowOptions, ShowOptionsClicked),
                        m_disable_item,
                        new WinForms.MenuItem(i18n.Text.About, AboutClicked),
                        new WinForms.MenuItem("-"),
                        new WinForms.MenuItem(i18n.Text.Restart, RestartClicked),
                        new WinForms.MenuItem(i18n.Text.Exit, ExitClicked),
                    })
                };
                m_tray_icon.DoubleClick += NotifyiconDoubleclicked;

                Composer.Changed += ComposerStateChanged;

                WinForms.Application.Run();
                m_tray_icon.Dispose();
            }
            finally
            {
                Composer.Changed -= ComposerStateChanged;
                m_messaging.DisableEvent -= DisableReceived;

                Settings.StopWatchConfigFile();
                KeyboardHook.Fini();
                Settings.SaveConfig();
                Composer.Fini();
            }
        }

        private static void NotifyiconDoubleclicked(object sender, EventArgs e)
        {
            if (m_sequencewindow == null)
            {
                m_sequencewindow = new SequenceWindow();
                ElementHost.EnableModelessKeyboardInterop(m_sequencewindow);
            }

            if (m_sequencewindow.IsVisible)
            {
                m_sequencewindow.Hide();
            }
            else
            {
                m_sequencewindow.Show();
            }
        }

        private static void ComposerStateChanged(object sender, EventArgs e)
        {
            m_tray_icon.Icon = Composer.IsDisabled()  ? Properties.Resources.IconDisabled
                             : Composer.IsComposing() ? Properties.Resources.IconActive
                                                      : Properties.Resources.IconNormal;
            m_tray_icon.Text = Composer.IsDisabled()
                              ? i18n.Text.DisabledToolTip
                              : String.Format(i18n.Text.TrayToolTip,
                                        Settings.ComposeKey.Value.FriendlyName,
                                        Settings.GetSequenceCount());

            m_disable_item.Checked = Composer.IsDisabled();
        }

        private static void ShowSequencesClicked(object sender, EventArgs e)
        {
            if (m_sequencewindow == null)
            {
                m_sequencewindow = new SequenceWindow();
                ElementHost.EnableModelessKeyboardInterop(m_sequencewindow);
            }
            m_sequencewindow.Show();
        }

        private static void ShowOptionsClicked(object sender, EventArgs e)
        {
            if (m_optionswindow == null)
            {
                m_optionswindow = new SettingsWindow();
                ElementHost.EnableModelessKeyboardInterop(m_optionswindow);
            }
            m_optionswindow.Show();
        }

        private static void DisableClicked(object sender, EventArgs e)
        {
            if (Composer.IsDisabled())
                m_messaging.TriggerDisableEvent();

            Composer.ToggleDisabled();
        }

        private static void DisableReceived(object sender, EventArgs e)
        {
            if (!Composer.IsDisabled())
                Composer.ToggleDisabled();
        }

        private static void AboutClicked(object sender, EventArgs e)
        {
            var about_box = new AboutBox();
            about_box.ShowDialog();
        }

        private static void RestartClicked(object sender, EventArgs e)
        {
            WinForms.Application.Restart();
            Environment.Exit(0);
        }

        private static void ExitClicked(object sender, EventArgs e)
        {
            WinForms.Application.Exit();
        }
    }

    public class InterProcessMessaging : WinForms.Form
    {
        public InterProcessMessaging()
        {
            // Forcing access to the window handle will let us receive messages
            var unused = this.Handle;
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        protected override void WndProc(ref WinForms.Message m)
        {
            if (m.Msg == WM_WINCOMPOSE_DISABLE)
            {
                if (Process.GetCurrentProcess().Id != (int)m.WParam)
                    if (DisableEvent != null)
                        DisableEvent(null, new EventArgs());
                return;
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Send a message to all other processes to ask them to disable any
        /// WinCompose hooks they may have installed.
        /// </summary>
        public void TriggerDisableEvent()
        {
            NativeMethods.PostMessage((IntPtr)0xffff, WM_WINCOMPOSE_DISABLE,
                                      Process.GetCurrentProcess().Id, 0);
        }

        public event EventHandler DisableEvent;

        /// <summary>
        /// A custom message ID used for inter-process communication
        /// </summary>
        private static readonly uint WM_WINCOMPOSE_DISABLE
            = NativeMethods.RegisterWindowMessage("WM_WINCOMPOSE_DISABLE");
    }
}
