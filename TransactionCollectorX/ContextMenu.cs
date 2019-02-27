using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Enumeration;
using TransactionCollectorX.Properties;

namespace TransactionCollectorX
{
    /// <summary>
    /// 
    /// </summary>
    class ContextMenu
    {
        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>ContextMenuStrip</returns>
        public ContextMenuStrip Create()
        {
            // Add the default menu options.
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem item;
            ToolStripSeparator sep;

            // Refresh.
            item = new ToolStripMenuItem();
            item.Text = "Rescan";
            item.Click += new EventHandler(Rescan_Click);
            // item.Image = SystemIcons.Information.ToBitmap();
            item.Image = Properties.Resources.Rescan;
            menu.Items.Add(item);

            // About.
            item = new ToolStripMenuItem();
            item.Text = "About";
            item.Click += new EventHandler(About_Click);
            // item.Image = SystemIcons.Information.ToBitmap();
            item.Image = Properties.Resources.About;
            menu.Items.Add(item);

            // Separator.
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

            // Exit.
            item = new ToolStripMenuItem();
            item.Text = "Exit";
            item.Click += new System.EventHandler(Exit_Click);
            // item.Image = SystemIcons.Error.ToBitmap();
            item.Image = Properties.Resources.Exit;
            menu.Items.Add(item);

            return menu;
        }

        /// <summary>
        /// Handles the Click event of the About control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void About_Click(object sender, EventArgs e)
        {
            if (Program.f1.Visible) { Program.f1.Hide(); } else { Program.f1.Show(); }
        }

        /// <summary>
        /// Processes a menu item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void Exit_Click(object sender, EventArgs e)
        {
            // Quit without further ado.
            Application.Exit();
        }

        /// <summary>
        /// Processes a menu item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        async void Rescan_Click(object sender, EventArgs e)
        {
            // find all portable devices
            // try collecting files
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.PortableStorageDevice);
            foreach (var device in devices)
            {
                using (var collector = new Collector(device.Id))
                {
                    collector.Collect(); 
                };
            }
        }
    }
}