using System;
using System.Windows.Forms;

namespace NWS_Alerts
{
    public class NotifyTray
    {
        // Notification Tray
        public static NotifyIcon notifyIcon { get; set; }
        public static ContextMenuStrip TrayIconContextMenu { get; set; }
        public static ToolStripMenuItem CloseMenuItem { get; set; }
        public static ToolStripMenuItem MuteMenuItem { get; set; }
        public static ToolStripMenuItem AboutMenuItem { get; set; }
        public static ToolStripMenuItem SettingsMenuItem { get; set; }
        public static ToolStripMenuItem AlertsWindowMenuItem { get; set; }
        public static ToolStripMenuItem LocationWindowMenuItem { get; set; }

        public static void TrayIconCreate()
        {
            notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = Properties.Resources.DayNotify
            };

            notifyIcon.MouseClick += MainWindow.NotifyIcon_MouseClick;
            notifyIcon.MouseDoubleClick += MainWindow.NotifyIcon_MouseDoubleClick;

            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            MuteMenuItem = new ToolStripMenuItem();
            AboutMenuItem = new ToolStripMenuItem();
            SettingsMenuItem = new ToolStripMenuItem();
            AlertsWindowMenuItem = new ToolStripMenuItem();
            LocationWindowMenuItem = new ToolStripMenuItem();
            TrayIconContextMenu.SuspendLayout();
            // 
            // TrayIconContextMenu
            // 
            TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {
                AlertsWindowMenuItem,
                new ToolStripSeparator(),
            SettingsMenuItem,
            LocationWindowMenuItem,
                MuteMenuItem,
                new ToolStripSeparator(),
                AboutMenuItem,
                new ToolStripSeparator(),
                CloseMenuItem
            });
            TrayIconContextMenu.Name = "TrayIconContextMenu";
            TrayIconContextMenu.Size = new System.Drawing.Size(153, 70);
            // 
            // CloseMenuItem
            // 
            CloseMenuItem.Name = "CloseMenuItem";
            CloseMenuItem.Size = new System.Drawing.Size(152, 22);
            CloseMenuItem.Text = "Exit";
            CloseMenuItem.Click += new EventHandler(MainWindow.CloseMenuItem_Click);
            // 
            // AlertsWindowMenuItem
            // 
            AlertsWindowMenuItem.Name = "AlertsWindowMenuItem";
            AlertsWindowMenuItem.Size = new System.Drawing.Size(152, 22);
            AlertsWindowMenuItem.Text = "View Active Alerts";
            AlertsWindowMenuItem.Click += new EventHandler(MainWindow.AlertsWindowMenuItem_Click);
            // 
            // SettingsMenuItem
            // 
            SettingsMenuItem.Name = "SettingsMenuItem";
            SettingsMenuItem.Size = new System.Drawing.Size(152, 22);
            SettingsMenuItem.Text = "Settings";
            SettingsMenuItem.Click += new EventHandler(MainWindow.SettingsMenuItem_Click);
            // 
            // LocationWindowMenuItem
            // 
            LocationWindowMenuItem.Name = "LocationWindowMenuItem";
            LocationWindowMenuItem.Size = new System.Drawing.Size(152, 22);
            LocationWindowMenuItem.Text = "Set Location";
            LocationWindowMenuItem.Click += new EventHandler(MainWindow.LocationWindowMenuItem_Click);
            // 
            // AboutMenuItem
            // 
            AboutMenuItem.Name = "AboutMenuItem";
            AboutMenuItem.Size = new System.Drawing.Size(152, 22);
            AboutMenuItem.Text = "About";
            AboutMenuItem.Click += new EventHandler(MainWindow.AboutMenuItem_Click);
            // 
            // MuteMenuItem
            // 
            MuteMenuItem.Name = "MuteMenuItem";
            MuteMenuItem.Size = new System.Drawing.Size(152, 22);
            MuteMenuItem.Text = "Mute Alerts";
            MuteMenuItem.Click += new EventHandler(MainWindow.MuteMenuItem_Click);

            TrayIconContextMenu.ResumeLayout(false);
            notifyIcon.ContextMenuStrip = TrayIconContextMenu;
        }
    }
}
