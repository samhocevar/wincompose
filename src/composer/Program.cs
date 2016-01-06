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

using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using WinForms = System.Windows.Forms;

namespace WinCompose
{
    static class Program
    {
        private static WinForms.NotifyIcon m_tray_icon;
        private static WinForms.MenuItem m_disable_item;
        private static WinForms.MenuItem m_update_menu;
        private static RemoteControl m_control;
        private static SequenceWindow m_sequencewindow;
        private static SettingsWindow m_optionswindow;

        [STAThread]
        static void Main()
        {
            // Do this before Composer.Init() because of the Disabled setting
            Settings.LoadConfig();

            Composer.Init();
            Settings.LoadSequences();
            KeyboardHook.Init();
            Updater.Init();

            Settings.StartWatchConfigFile();

            try
            {
                WinForms.Application.EnableVisualStyles();
                WinForms.Application.SetCompatibleTextRenderingDefault(false);

                m_control = new RemoteControl();
                m_control.DisableEvent += OnDisableEvent;
                m_control.ExitEvent += OnExitEvent;
                m_control.TriggerDisableEvent();

                m_tray_icon = new WinForms.NotifyIcon
                {
                    Visible = true,
                    Icon = GetCurrentIcon(),
                    Text = GetCurrentToolTip(),
                    ContextMenu = new WinForms.ContextMenu(new[]
                    {
                        new WinForms.MenuItem("WinCompose")
                        {
                            Enabled = false,
                        },
                        new WinForms.MenuItem("-"),
                        new WinForms.MenuItem(i18n.Text.ShowSequences, ShowSequencesClicked),
                        new WinForms.MenuItem(i18n.Text.ShowOptions, ShowOptionsClicked),
                        m_disable_item = /* Keep a reference on this entry */
                        new WinForms.MenuItem(i18n.Text.Disable, DisableClicked),
                        new WinForms.MenuItem(i18n.Text.About, AboutClicked),
                        new WinForms.MenuItem("-"),
                        new WinForms.MenuItem(i18n.Text.Updates, new[]
                        {
                            new WinForms.MenuItem(i18n.Text.VisitWebsite, delegate(object o, EventArgs e) { System.Diagnostics.Process.Start("http://wincompose.info/"); }),
                            m_update_menu = /* Keep a reference on this entry */
                            new WinForms.MenuItem(""),
                        }),
                        new WinForms.MenuItem(i18n.Text.Restart, RestartClicked),
                        new WinForms.MenuItem(i18n.Text.Exit, OnExitEvent),
                    })
                };
                m_tray_icon.DoubleClick += NotifyiconDoubleclicked;

                Composer.Changed += ComposerStateChanged;
                ComposerStateChanged(null, new EventArgs());

                Updater.Changed += UpdaterStateChanged;
                UpdaterStateChanged(null, new EventArgs());

                WinForms.Application.Run();
                m_tray_icon.Dispose();
            }
            finally
            {
                Composer.Changed -= ComposerStateChanged;
                Updater.Changed -= UpdaterStateChanged;
                m_control.DisableEvent -= OnDisableEvent;
                m_control.ExitEvent -= OnExitEvent;

                Settings.StopWatchConfigFile();
                Updater.Fini();
                KeyboardHook.Fini();
                Settings.SaveConfig();
                Composer.Fini();
                Updater.Fini();
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
            m_tray_icon.Icon = GetCurrentIcon();
            m_tray_icon.Text = GetCurrentToolTip();

            m_disable_item.Checked = Composer.IsDisabled();
        }

        private static string GetCurrentToolTip()
        {
            if (Composer.IsDisabled())
                return i18n.Text.DisabledToolTip;

            return String.Format(i18n.Text.TrayToolTip,
                                 Settings.ComposeKey.Value.FriendlyName,
                                 Settings.SequenceCount,
                                 Settings.Version);
        }

        private static System.Drawing.Icon GetCurrentIcon()
        {
            return GetIcon((Composer.IsDisabled() ?     0x1 : 0x0) |
                           (Composer.IsComposing() ?    0x2 : 0x0) |
                           (Updater.HasNewerVersion() ? 0x4 : 0x0));
        }

        private static System.Drawing.Icon GetIcon(int index)
        {
            if (m_icon_cache == null)
                m_icon_cache = new System.Drawing.Icon[8];

            if (m_icon_cache[index] == null)
            {
                // XXX: if you create new bitmap images here instead of using bitmaps from
                // resources, make sure the DPI settings match. Our PNGs are 72 DPI whereas
                // new Bitmap objects appear to use 96 by default (even if copy-constructed).
                // A reasonable workaround might be to use Clone().
                using (Bitmap bitmap = Properties.Resources.KeyEmpty)
                using (Graphics canvas = Graphics.FromImage(bitmap))
                {
                    // LED status: on or off
                    canvas.DrawImage(Composer.IsComposing() ? Properties.Resources.DecalActive
                                                            : Properties.Resources.DecalIdle, 0, 0);

                    // Large red cross indicating we’re disabled
                    if (Composer.IsDisabled())
                        canvas.DrawImage(Properties.Resources.DecalDisabled, 0, 0);

                    canvas.Save();
                    m_icon_cache[index] = Icon.FromHandle(bitmap.GetHicon());
                }
            }

            return m_icon_cache[index];
        }

        private static System.Drawing.Icon[] m_icon_cache;

        private static void UpdaterStateChanged(object sender, EventArgs e)
        {
            m_update_menu.Visible = Updater.HasNewerVersion();
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
                m_control.TriggerDisableEvent();

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

        private static void OnDisableEvent(object sender, EventArgs e)
        {
            if (!Composer.IsDisabled())
                Composer.ToggleDisabled();
        }

        private static void OnExitEvent(object sender, EventArgs e)
        {
            WinForms.Application.Exit();
        }
    }
}

