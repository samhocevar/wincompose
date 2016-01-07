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
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using WinForms = System.Windows.Forms;

namespace WinCompose
{
    static class Program
    {
        private static WinForms.NotifyIcon m_tray_icon;
        private static WinForms.MenuItem m_help_item;
        private static WinForms.MenuItem m_disable_item;
        private static WinForms.MenuItem m_download_item;
        private static string m_download_url;
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
                m_control.DisableEvent += SysTrayUpdateCallback;
                m_control.ExitEvent += OnExitEvent;
                m_control.TriggerDisableEvent();

                m_tray_icon = new WinForms.NotifyIcon
                {
                    Visible = true,
                    //Icon = GetCurrentIcon(),
                    //Text = GetCurrentToolTip(),
                    ContextMenu = new WinForms.ContextMenu(new[]
                    {
                        new TitleMenuItem(),
                        new WinForms.MenuItem("-"),
                        new WinForms.MenuItem(i18n.Text.ShowSequences, ShowSequencesClicked),
                        new WinForms.MenuItem(i18n.Text.ShowOptions, ShowOptionsClicked),
                        m_help_item = /* Keep a reference on this entry */
                        new WinForms.MenuItem(i18n.Text.Help, new[]
                        {
                            new WinForms.MenuItem(i18n.Text.About, AboutClicked),
                            new WinForms.MenuItem(i18n.Text.VisitWebsite, delegate(object o, EventArgs e) { System.Diagnostics.Process.Start("http://wincompose.info/"); }),
                            m_download_item = /* Keep a reference on this entry */
                            new WinForms.MenuItem("", DownloadClicked)
                            {
                                Visible = false
                            },
                        }),
                        new WinForms.MenuItem("-"),
                        m_disable_item = /* Keep a reference on this entry */
                        new WinForms.MenuItem(i18n.Text.Disable, DisableClicked),
                        new WinForms.MenuItem(i18n.Text.Restart, RestartClicked),
                        new WinForms.MenuItem(i18n.Text.Exit, OnExitEvent),
                    })
                };
                m_tray_icon.DoubleClick += NotifyiconDoubleclicked;

                Composer.Changed += SysTrayUpdateCallback;
                Updater.Changed += SysTrayUpdateCallback;
                SysTrayUpdateCallback(null, new EventArgs());

                Updater.Changed += UpdaterStateChanged;
                UpdaterStateChanged(null, new EventArgs());

                WinForms.Application.Run();
                m_tray_icon.Dispose();
            }
            finally
            {
                Composer.Changed -= SysTrayUpdateCallback;
                Updater.Changed -= SysTrayUpdateCallback;
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

        private static void SysTrayUpdateCallback(object sender, EventArgs e)
        {
            m_tray_icon.Icon = GetCurrentIcon();

            // XXX: we cannot directly set m_tray_icon.Text because it has an
            // erroneous 64-character limitation (the underlying framework has
            // the correct 128-char limitation). So instead we use this hack,
            // taken from http://stackoverflow.com/a/580264/111461
            Type t = typeof(WinForms.NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(m_tray_icon, GetCurrentToolTip());
            if ((bool)t.GetField("added", hidden).GetValue(m_tray_icon))
                t.GetMethod("UpdateIcon", hidden).Invoke(m_tray_icon, new object[] { true });

            m_disable_item.Checked = Composer.IsDisabled();
        }

        private static string GetCurrentToolTip()
        {
            string ret = i18n.Text.DisabledToolTip;

            if (!Composer.IsDisabled())
                ret = string.Format(i18n.Text.TrayToolTip,
                                    Settings.ComposeKey.Value.FriendlyName,
                                    Settings.SequenceCount,
                                    Settings.Version);

            if (Updater.HasNewerVersion())
                ret += "\n" + i18n.Text.UpdatesToolTip;

            return ret;
        }

        private static System.Drawing.Icon GetCurrentIcon()
        {
            return GetIcon((Composer.IsDisabled() ?     0x1 : 0x0) |
                           (Composer.IsComposing() ?    0x2 : 0x0) |
                           (Updater.HasNewerVersion() ? 0x4 : 0x0));
        }

        public static System.Drawing.Icon GetIcon(int index)
        {
            if (m_icon_cache == null)
                m_icon_cache = new System.Drawing.Icon[8];

            if (m_icon_cache[index] == null)
            {
                bool is_disabled = (index & 0x1) != 0;
                bool is_composing = (index & 0x2) != 0;
                bool has_update = (index & 0x4) != 0;

                // XXX: if you create new bitmap images here instead of using bitmaps from
                // resources, make sure the DPI settings match. Our PNGs are 72 DPI whereas
                // new Bitmap objects appear to use 96 by default (even if copy-constructed).
                // A reasonable workaround might be to use Clone().
                using (Bitmap bitmap = Properties.Resources.KeyEmpty)
                using (Graphics canvas = Graphics.FromImage(bitmap))
                {
                    // LED status: on or off
                    canvas.DrawImage(is_composing ? Properties.Resources.DecalActive
                                                  : Properties.Resources.DecalIdle, 0, 0);

                    // Large red cross indicating we’re disabled
                    if (is_disabled)
                        canvas.DrawImage(Properties.Resources.DecalDisabled, 0, 0);

                    // Tiny yellow exclamation mark to advertise updates
                    if (has_update)
                        canvas.DrawImage(Properties.Resources.DecalUpdate, 0, 0);

                    canvas.Save();
                    m_icon_cache[index] = Icon.FromHandle(bitmap.GetHicon());
                }
            }

            return m_icon_cache[index];
        }

        private static System.Drawing.Icon[] m_icon_cache;

        private static void UpdaterStateChanged(object sender, EventArgs e)
        {
            m_download_item.Visible = false;
            if (Updater.HasNewerVersion())
            {
                var text = string.Format(i18n.Text.Download,
                                         Updater.Get("Latest") ?? "");
                var url = Settings.IsInstalled() ? Updater.Get("Installer")
                                                 : Updater.Get("Portable");
                if (url != null)
                {
                    m_download_item.Visible = true;
                    m_download_item.Text = text;
                    m_download_url = url;
                }
            }
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

        private static void DownloadClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(m_download_url);
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

    class TitleMenuItem : WinForms.MenuItem
    {
        public TitleMenuItem()
        {
            Text = "WinCompose";
            m_font = new Font(SystemFonts.DefaultFont.FontFamily,
                              SystemFonts.DefaultFont.Size * 1.1f,
                              FontStyle.Bold);
            OwnerDraw = true;
            Enabled = false;
        }

        protected override void OnMeasureItem(WinForms.MeasureItemEventArgs e)
        {
            var size = WinForms.TextRenderer.MeasureText(Text, m_font);
            e.ItemWidth = size.Width;
            e.ItemHeight = size.Height;
        }

        protected override void OnDrawItem(WinForms.DrawItemEventArgs e)
        {
            e.DrawBackground();

            var brush = new LinearGradientBrush(e.Bounds, Color.SkyBlue,
                                                Color.White, 30.0f);
            e.Graphics.FillRectangle(brush, e.Bounds);

            var icon_bounds = e.Bounds;
            icon_bounds.Width = e.Bounds.Height;
            e.Graphics.DrawIcon(Program.GetIcon(0), icon_bounds);

            var text_bounds = e.Bounds;
            text_bounds.Width -= e.Bounds.Height;
            text_bounds.X += e.Bounds.Height;
            e.Graphics.DrawString(Text, m_font, Brushes.Black, text_bounds);
        }

        private Font m_font;
    }
}

