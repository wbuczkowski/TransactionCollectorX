using System;
using System.Windows.Forms;
using Windows.Storage;
using Windows.Devices.Enumeration;

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
            item = new ToolStripMenuItem
            {
                Text = "Rescan",
                // item.Image = SystemIcons.Information.ToBitmap();
                Image = Properties.Resources.Rescan
            };
            item.Click += new EventHandler(Rescan_Click);
            menu.Items.Add(item);

            // About.
            item = new ToolStripMenuItem
            {
                Text = "About",
                // item.Image = SystemIcons.Information.ToBitmap();
                Image = Properties.Resources.About
            };
            item.Click += new EventHandler(About_Click);
            menu.Items.Add(item);

            // Select Folder
            item = new ToolStripMenuItem
            {
                Text = "Select Folder",
                // item.Image = SystemIcons.Information.ToBitmap();
                //Image = Properties.Resources.About
            };
            item.Click += new EventHandler(Folder_Click);
            menu.Items.Add(item);


            // Separator.
            sep = new ToolStripSeparator();
            menu.Items.Add(sep);

            // Exit.
            item = new ToolStripMenuItem
            {
                Text = "Exit",
                // item.Image = SystemIcons.Error.ToBitmap();
                Image = Properties.Resources.Exit
            };
            item.Click += new System.EventHandler(Exit_Click);
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
            if (Program.F1.Visible) { Program.F1.Hide(); } else { Program.F1.Show(); }
        }

        /// <summary>
        /// Handles the Click event of the About control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        async void Folder_Click(object sender, EventArgs e)
        {
            StorageFolder folder = null;
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                // user cancelled - return
                if (result != DialogResult.OK) return;
                folder = await StorageFolder.GetFolderFromPathAsync(dialog.SelectedPath);
                if (folder == null) return;
            }
            // save setting .NET
            Properties.Settings.Default.DestinationPath = folder.Path;
            Properties.Settings.Default.Save();
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