using System;
using System.ComponentModel;
using System.Windows.Forms;
using SimLinkup.Properties;

namespace SimLinkup.UI
{
    public partial class frmMain : Form
    {
        public static Runtime.Runtime SharedRuntime;

        public frmMain()
        {
            InitializeComponent();
        }


        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            FileExit();
        }

        private void mnuActionsStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void mnuActionsStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void mnuToolsOptions_Click(object sender, EventArgs e)
        {
            ToolsOptions();
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            HelpAbout();
        }

        private void FileExit()
        {
            Close();
        }

        private void Start()
        {
            StopAndDisposeRuntime();
            btnStop.Enabled = true;
            mnuActionsStop.Enabled = true;
            mnuTrayStop.Enabled = true;

            mnuToolsOptions.Enabled = false;
            btnOptions.Enabled = false;
            mnuTrayOptions.Enabled = false;

            btnStart.Enabled = false;
            mnuActionsStart.Enabled = false;
            mnuTrayStart.Enabled = false;

            if (Settings.Default.MinimizeWhenStarted)
            {
                WindowState = FormWindowState.Minimized;
            }

            CreateAndStartRuntime();
        }

        private void CreateAndStartRuntime()
        {
            CreateRuntime();
            SharedRuntime.Start();
        }

        private void CreateRuntime()
        {
            if (SharedRuntime != null) StopAndDisposeRuntime();
            SharedRuntime = new Runtime.Runtime();
        }

        private void StopAndDisposeRuntime()
        {
            if (SharedRuntime != null)
            {
                if (SharedRuntime.IsRunning)
                {
                    Stop();
                }
                DisposeRuntime();
            }
        }

        private void DisposeRuntime()
        {
            if (SharedRuntime != null)
            {
                Common.Util.DisposeObject(SharedRuntime);
                SharedRuntime = null;
            }
        }

        private void Stop()
        {
            if (SharedRuntime != null && SharedRuntime.IsRunning)
            {
                SharedRuntime.Stop();
                Common.Util.DisposeObject(SharedRuntime);
                SharedRuntime = null;
            }
            btnStop.Enabled = false;
            mnuActionsStop.Enabled = false;
            mnuTrayStop.Enabled = false;

            mnuToolsOptions.Enabled = true;
            mnuTrayOptions.Enabled = true;
            btnOptions.Enabled = true;

            mnuActionsStart.Enabled = true;
            btnStart.Enabled = true;
            mnuTrayStart.Enabled = true;
        }

        private void ToolsOptions()
        {
            new frmOptions().ShowDialog(this);
        }

        private void HelpAbout()
        {
            new HelpAbout().ShowDialog(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (SharedRuntime != null && SharedRuntime.IsRunning)
            {
                Stop();
            }
            base.OnClosing(e);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            ToolsOptions();
        }

        private void nfyTrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                RestoreFromTray();
            }
        }

        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                if (Settings.Default.MinimizeToSystemTray)
                {
                    MinimizeToTray();
                }
            }
            else
            {
                RestoreFromTray();
            }
        }

        private void RestoreFromTray()
        {
            Visible = true;
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            nfyTrayIcon.Visible = false;
        }

        private void MinimizeToTray()
        {
            nfyTrayIcon.Visible = true;
            WindowState = FormWindowState.Minimized;
            Visible = false;
            ShowInTaskbar = false;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            nfyTrayIcon.Text = Application.ProductName;
            Text = Application.ProductName + " v" + Application.ProductVersion;
            if (Settings.Default.StartRunningWhenLaunched)
            {
                Start();
            }
        }

        private void mnuTrayStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void mnuTrayStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void mnuTrayOptions_Click(object sender, EventArgs e)
        {
            ToolsOptions();
        }

        private void mnuTrayExit_Click(object sender, EventArgs e)
        {
            FileExit();
        }

        private void mnuTrayRestore_Click(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                CreateRuntime();
                var chooser = new SignalPicker();
                chooser.ScriptingContext = SharedRuntime.ScriptingContext;
                DialogResult result = chooser.ShowDialog(this);
                if ((result & DialogResult.OK) == 0)
                {
                    
                }

            }
            finally
            {
                DisposeRuntime();
            }
        }
    }
}