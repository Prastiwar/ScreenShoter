using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TP.ScreenShooter
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        const int MYACTION_HOTKEY_ID = 1;
        private NotifyIcon notifyIcon;
        private ContextMenu notifyContext;
        private MenuItem notifyContextItem1;
        private MenuItem notifyContextItem2;

        const int WM_HOTKEY = 0x0312;

        Bitmap screenshot;
        string filePath;
        string fileExtension;
        string savedFile;

        public Form1()
        {
            InitializeComponent();
            // Start on windows tray
            ShowInTaskbar = false;
            
            UnregisterHotKey(this.Handle, MYACTION_HOTKEY_ID);
            // register ctrl + printscreen
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID, 2, (int)Keys.Snapshot);

            // register run on startup
            RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            reg.SetValue("Screenshooter", Application.ExecutablePath.ToString());

            notifyIcon.ContextMenu = this.notifyContext;

            // Make sure app is in windows tray on start
            Form1_Resize(this, null);
        }

        // Shortcut event
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == MYACTION_HOTKEY_ID)
            {
                // Do on shortcut
                button1_Click(this, null);
            }
            base.WndProc(ref m);
        }

        //  The NotifyIcon tray event
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon.Visible = true;
                this.Hide();
            }
        }
        // NotifyIcon tray show App
        private void notifyIcon_MouseDoubleClick(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }
        // NotifyIcon exit app
        private void exitNotification_Click(object Sender, EventArgs e)
        {
            Application.Exit();
        }
        // NotifyIcon take snapshot
        private void printscreenNotification_Click(object Sender, EventArgs e)
        {
            button1_Click(this, null);
        }
        // Minimize on close
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
            }
        }

        // Take screenshot
        private void button1_Click(object sender, EventArgs e)
        {
            // Make sure there is clear screen - without app and its notifications
            WindowState = FormWindowState.Minimized;
            notifyIcon.Visible = false;

            string fileName = "Screenshot";
            string extensionName = "png";

            TakeSnapshot();
            SetFileNames(fileName, extensionName);
            SaveFile();

            // Notifing user, where file was saved
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(0, null, "Zapisano zdjęcie jako: \n" + savedFile, ToolTipIcon.Info);

        }
        // Take screen
        private void TakeSnapshot()
        {
            screenshot = new Bitmap(SystemInformation.VirtualScreen.Width,
                               SystemInformation.VirtualScreen.Height,
                               PixelFormat.Format32bppArgb);
            Graphics screenGraph = Graphics.FromImage(screenshot);
            screenGraph.CopyFromScreen(SystemInformation.VirtualScreen.X,
                                       SystemInformation.VirtualScreen.Y,
                                       0,
                                       0,
                                       SystemInformation.VirtualScreen.Size,
                                       CopyPixelOperation.SourceCopy);
        }
        // Set path to desktop with filename
        private void SetFileNames(string fileName, string extensionName)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            filePath = path + ("\\" + fileName);
            fileExtension = "." + extensionName;
        }
        // Save file to path
        private void SaveFile()
        {
            int index = 1;
            while (true)
            {
                // Check if first screenshoot exist, so save it
                if (!File.Exists(filePath + fileExtension))
                {
                    savedFile = filePath + fileExtension;
                    screenshot.Save(savedFile, ImageFormat.Png);
                    break;
                }
                else
                {
                    // if first screenshot exist - check if file with index exist, so save it
                    if (!File.Exists(filePath + index + fileExtension))
                    {
                        savedFile = filePath + index + fileExtension;
                        screenshot.Save(savedFile, ImageFormat.Png);
                        break;
                    }
                    else
                    {
                        // if file with index exist - check if file with index + 1 exist, so save it
                        if (!File.Exists(filePath + index + fileExtension))
                        {
                            savedFile = filePath + (index + 1) + fileExtension;
                            screenshot.Save(savedFile, ImageFormat.Png);
                            break;
                        }
                    }
                }
                // if file with index + 1 exist - add next index and repeat checking
                index++;
            }
        }
    }
}
