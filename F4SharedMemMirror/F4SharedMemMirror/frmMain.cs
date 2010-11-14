using System;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
namespace F4SharedMemMirror
{
    public partial class frmMain : Form
    {
        private Mirror _mirror = null;
        public frmMain()
        {
            InitializeComponent();
        }

        private void chkLaunchAtSystemStartup_CheckedChanged(object sender, EventArgs e)
        {
            UpdateWindowsStartupRegKey();
        }
        private void UpdateWindowsStartupRegKey()
        {
            if (chkLaunchAtSystemStartup.Checked)
            {
                //update the Windows Registry's Run-at-startup applications list according
                //to the new user settings
                Computer c = new Computer();
                try
                {
                    using (Microsoft.Win32.RegistryKey startupKey = c.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        startupKey.SetValue(Application.ProductName, Application.ExecutablePath, Microsoft.Win32.RegistryValueKind.String);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            else
            {
                Computer c = new Computer();
                try
                {
                    using (Microsoft.Win32.RegistryKey startupKey = c.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        startupKey.DeleteValue(Application.ProductName, false);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            Properties.Settings.Default.LaunchAtWindowsStartup = chkLaunchAtSystemStartup.Checked;
            Properties.Settings.Default.Save();
        }
        private void chkStartMirroringWhenLaunched_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.StartMirroringWhenLaunched = chkStartMirroringWhenLaunched.Checked;
            Properties.Settings.Default.Save();
        }

        private void chkMinimizeToSystemTray_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MinimizeToSystemTray = chkMinimizeToSystemTray.Checked;
            Properties.Settings.Default.Save();

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            lblVersion.Text = "Version: " + Application.ProductVersion;
            nfyTrayIcon.Icon = this.Icon;
            foreach (string priority in Enum.GetNames(typeof (System.Threading.ThreadPriority)))
            {
                if (priority.ToLowerInvariant() != "highest")
                {
                    cbPriority.Items.Add(priority);
                }
            }
            LoadSettings();
            if (chkStartMirroringWhenLaunched.Checked) 
            {
                StartMirroring();
            }
        }
        private void LoadSettings()
        {
            Properties.Settings.Default.Reload();
            chkMinimizeToSystemTray.Checked = Properties.Settings.Default.MinimizeToSystemTray;
            chkStartMirroringWhenLaunched.Checked = Properties.Settings.Default.StartMirroringWhenLaunched;
            chkLaunchAtSystemStartup.Checked = Properties.Settings.Default.LaunchAtWindowsStartup;
            chkRunMinimized.Checked = Properties.Settings.Default.RunMinimized;
            rdoClientMode.Checked = Properties.Settings.Default.RunAsClient;
            rdoServerMode.Checked = Properties.Settings.Default.RunAsServer;
            txtServerIPAddress.Text = Properties.Settings.Default.ServerIPAddress;
            txtServerPortNum.Text = Properties.Settings.Default.ServerPortNum;
            nudPollFrequency.Value = Properties.Settings.Default.PollingFrequencyMillis;
            cbPriority.SelectedItem= Enum.GetName(typeof (System.Threading.ThreadPriority), Properties.Settings.Default.Priority);
          
            UpdateWindowsStartupRegKey();

        }
        private void rdoClientMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoClientMode.Checked)
            {
                txtServerIPAddress.Enabled = true;
                txtServerPortNum.Enabled = true;
                Properties.Settings.Default.RunAsClient = rdoClientMode.Checked;
                Properties.Settings.Default.RunAsServer= !rdoClientMode.Checked;
                Properties.Settings.Default.Save();
            }
        }

        private void rdoServerMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoServerMode.Checked)
            {
                txtServerIPAddress.Enabled = false;
                Properties.Settings.Default.RunAsServer = rdoServerMode.Checked;
                Properties.Settings.Default.RunAsClient= !rdoServerMode.Checked;
                Properties.Settings.Default.Save();
            }
        }

        private void MinimizeToSystemTray()
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.nfyTrayIcon.Visible = true;
        }
        private void RestoreFromSystemTray()
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.nfyTrayIcon.Visible = false;
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            StartMirroring();
        }
        private void StopMirroring()
        {
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;
            btnStop.Enabled = false;
            mnuNfyStopMirroring.Enabled = false;
            if (_mirror != null)
            {
                _mirror.StopMirroring();
                try
                {
                    _mirror.Dispose();
                    _mirror = null;
                }
                catch (Exception e)
                {
                }
            }
            gbNetworkingOptions.Enabled = true;
            gbGeneralOptions.Enabled = true;
            gbPerformanceOptions.Enabled = true;
            btnStart.Enabled = true;
            mnuNfyStartMirroring.Enabled = true;
        }
        private void StartMirroring()
        {
            System.Threading.Thread.CurrentThread.Priority = Properties.Settings.Default.Priority;
            System.Net.IPAddress address = null;
            if (rdoClientMode.Checked)
            {
                bool validIpAddress = false;
                validIpAddress = System.Net.IPAddress.TryParse(txtServerIPAddress.Text, out address);
                if (String.IsNullOrEmpty(txtServerIPAddress.Text.Trim()) || !validIpAddress)
                {
                    MessageBox.Show("Please enter a valid IP address for the " + Application.ProductName + " server.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    RestoreFromSystemTray();
                    txtServerIPAddress.Focus();
                    return;
                }
                if (String.IsNullOrEmpty(txtServerPortNum.Text.Trim()))
                {
                    MessageBox.Show("Please enter the port number of the " + Application.ProductName + " server.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    RestoreFromSystemTray();
                    txtServerPortNum.Focus();
                    return;
                }
            }
            if (rdoServerMode.Checked)
            {
                if (String.IsNullOrEmpty(txtServerPortNum.Text.Trim()))
                {
                    MessageBox.Show("Please enter the port number to publish the " + Application.ProductName + " service on.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    RestoreFromSystemTray();
                    txtServerPortNum.Focus();
                    return;
                }
            }
            int portNum = 21142;
            Int32.TryParse(Properties.Settings.Default.ServerPortNum, out portNum);
            btnStart.Enabled = false;
            mnuNfyStartMirroring.Enabled = false;
            if (_mirror != null)
            {
                _mirror.StopMirroring();
            }
            btnStart.Enabled = false;
            mnuNfyStartMirroring.Enabled = false;
            _mirror = new Mirror();
            if (rdoClientMode.Checked)
            {
                _mirror.NetworkingMode = NetworkingMode.Client;
                _mirror.ClientIPAddress = address;
            }
            else if (rdoServerMode.Checked) 
            {
                _mirror.NetworkingMode = NetworkingMode.Server;
            }
            _mirror.PortNumber = (ushort)portNum;

            gbPerformanceOptions.Enabled = false;
            gbNetworkingOptions.Enabled = false;
            gbGeneralOptions.Enabled = false;
            if (chkRunMinimized.Checked)
            {
                if (chkMinimizeToSystemTray.Checked)
                {
                    MinimizeToSystemTray();
                }
                else
                {
                    this.WindowState = FormWindowState.Minimized;
                }
            }
            btnStop.Enabled = true;
            mnuNfyStopMirroring.Enabled = true;
            _mirror.StartMirroring();
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (chkMinimizeToSystemTray.Checked) 
                {
                    MinimizeToSystemTray();
                }
            }
            else if (this.WindowState != FormWindowState.Minimized)
            {
                RestoreFromSystemTray();
            }
        }

        private void mnuNfyRestore_Click(object sender, EventArgs e)
        {
            RestoreFromSystemTray();
        }

        private void mnuNfyStopMirroring_Click(object sender, EventArgs e)
        {
            StopMirroring();
        }

        private void mnuNfyStartMirroring_Click(object sender, EventArgs e)
        {
            StartMirroring();
        }

        private void mnuNfyExit_Click(object sender, EventArgs e)
        {
            Quit();
        }
        private void Quit()
        {
            this.nfyTrayIcon.Visible = false;
            Application.Exit();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Quit();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopMirroring();
        }

        private void txtServerIPAddress_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.ServerIPAddress = txtServerIPAddress.Text;
            Properties.Settings.Default.Save();
        }

        private void cbPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Threading.ThreadPriority selectedPriority = (System.Threading.ThreadPriority) Enum.Parse(typeof(System.Threading.ThreadPriority), (string)cbPriority.SelectedItem);
            Properties.Settings.Default.Priority = selectedPriority;
            Properties.Settings.Default.Save();
        }

        private void nudPollFrequency_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PollingFrequencyMillis = (int)nudPollFrequency.Value;
            Properties.Settings.Default.Save();
        }

        private void txtServerPortNum_Leave(object sender, EventArgs e)
        {
            int serverPortNum = -1;
            bool parsed = Int32.TryParse(txtServerPortNum.Text, out serverPortNum);
            if (!parsed || serverPortNum < 0 || serverPortNum > 65535)
            {
                MessageBox.Show("Invalid port number.  Port number must be between 0 and 65535", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtServerPortNum.Text = Properties.Settings.Default.ServerPortNum;
                txtServerPortNum.Focus();
            }
            else
            {
                Properties.Settings.Default.ServerPortNum = txtServerPortNum.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void txtServerPortNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            //e.Handled = !((e.KeyChar >= '0' && e.KeyChar <= '9') ;
        }

        private void nfyTrayIcon_DoubleClick(object sender, EventArgs e)
        {
            RestoreFromSystemTray();
        }

        private void chkRunMinimized_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RunMinimized = chkRunMinimized.Checked;
            Properties.Settings.Default.Save();
        }

    }
}
