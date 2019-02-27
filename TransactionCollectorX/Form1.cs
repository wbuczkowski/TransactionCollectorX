using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Devices.Portable;

using System.Runtime;
using System.Runtime.InteropServices;
// using System.IO;
using TransactionCollectorX.Properties;

// This is the code for your desktop app.
// Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

namespace TransactionCollectorX
{
    public partial class Form1 : Form
    {

        private IntPtr m_hNotifyDevNode;
        public ProcessIcon pi;
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.FormClosing += Form1_FormClosing;
            pi = new ProcessIcon();
        }

        // private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        // {
        //     // Click on the link below to continue learning how to build a desktop app using WinForms!
        //     System.Diagnostics.Process.Start("http://aka.ms/dotnet-get-started-desktop");
        // }

        // private void button1_Click(object sender, EventArgs e)
        // {
        //     MessageBox.Show("Thanks!");
        // }

        private void RegisterNotification(Guid guid)
        {
            Dbt.DEV_BROADCAST_DEVICEINTERFACE devIF = new Dbt.DEV_BROADCAST_DEVICEINTERFACE();
            IntPtr devIFBuffer;

            // Set to HID GUID
            devIF.dbcc_size = Marshal.SizeOf(devIF);
            devIF.dbcc_devicetype = Dbt.DBT_DEVTYP_DEVICEINTERFACE;
            devIF.dbcc_reserved = 0;
            devIF.dbcc_classguid = guid;

            // Allocate a buffer for DLL call
            devIFBuffer = Marshal.AllocHGlobal(devIF.dbcc_size);

            // Copy devIF to buffer
            Marshal.StructureToPtr(devIF, devIFBuffer, true);

            // Register for HID device notifications
            m_hNotifyDevNode = Dbt.RegisterDeviceNotification(this.Handle, devIFBuffer, Dbt.DEVICE_NOTIFY_WINDOW_HANDLE);

            // Copy buffer to devIF
            Marshal.PtrToStructure(devIFBuffer, devIF);

            // Free buffer
            Marshal.FreeHGlobal(devIFBuffer);
        }

        // Unregister HID device notification
        private void UnregisterNotification()
        {
            uint ret = Dbt.UnregisterDeviceNotification(m_hNotifyDevNode);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // GUID_DEVINTERFACE_WPD
            Guid wpdGuid = new Guid("6AC27878-A6FA-4155-BA85-F98F491D4F33");
            RegisterNotification(wpdGuid);
            // Hide the main form on start
            BeginInvoke(new MethodInvoker(() => { Hide(); }));
            // Display notification icon
            pi.Display();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Hide();
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            UnregisterNotification();
            pi.Dispose();
            base.Dispose(disposing);
        }

        protected override void WndProc(ref Message m)
        {
            // Intercept the WM_DEVICECHANGE message
            if (m.Msg == Dbt.WM_DEVICECHANGE)
            {
                // Get the message event type
                int nEventType = m.WParam.ToInt32();

                // Check for devices being connected or disconnected
                if (nEventType == Dbt.DBT_DEVICEARRIVAL ||
                    nEventType == Dbt.DBT_DEVICEREMOVECOMPLETE)
                {
                    Dbt.DEV_BROADCAST_HDR hdr = new Dbt.DEV_BROADCAST_HDR();

                    // Convert lparam to DEV_BROADCAST_HDR structure
                    Marshal.PtrToStructure(m.LParam, hdr);

                    if (hdr.dbch_devicetype == Dbt.DBT_DEVTYP_DEVICEINTERFACE)
                    {
                        Dbt.DEV_BROADCAST_DEVICEINTERFACE_1 devIF = new Dbt.DEV_BROADCAST_DEVICEINTERFACE_1();

                        // Convert lparam to DEV_BROADCAST_DEVICEINTERFACE structure
                        Marshal.PtrToStructure(m.LParam, devIF);

                        // Get the device path from the broadcast message
                        string devicePath = new string(devIF.dbcc_name);

                        // Remove null-terminated data from the string
                        int pos = devicePath.IndexOf((char)0);
                        if (pos != -1)
                        {
                            devicePath = devicePath.Substring(0, pos);
                        }

                        // An HID device was connected or removed
                        if (nEventType == Dbt.DBT_DEVICEREMOVECOMPLETE)
                        {
                            // MessageBox.Show("Device \"" + devicePath + "\" was removed");
                            pi.Show("Device \"" + devicePath + "\" was removed");
                            // TODO: stop processing at device disconnect
                        }
                        else if (nEventType == Dbt.DBT_DEVICEARRIVAL)
                        {
                            // MessageBox.Show("Device \"" + devicePath + "\" arrived");
                            pi.Show("Device \"" + devicePath + "\" arrived");
                            // TODO: store collectors with device paths to stop processing at device disconnection
                            using (var collector = new Collector(devicePath))
                            {
                                Task.Run(() => { collector.Collect(); });
                            };
                        }
                    }
                }
            }
            base.WndProc(ref m);
        }
    }
}
