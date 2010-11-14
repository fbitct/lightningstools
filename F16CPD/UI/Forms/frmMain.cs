using System;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.Drawing;
using log4net;
namespace F16CPD.UI.Forms
{
    public partial class frmMain : Form, IDisposable
    {
        //TODO: add option to config screen to allow setting the initial transition altitude
        private F16CpdEngine _cpdEngine = null;
        private static ILog _log = LogManager.GetLogger(typeof(frmMain));
        protected bool _isDisposed = false;
        public frmMain()
        {
            InitializeComponent();
        }
        public string[] CommandLineSwitches
        {
            get;
            set;
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
                    _log.Error(ex.Message, ex);
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
                    _log.Error(ex.Message, ex);
                }
            }
            Properties.Settings.Default.LaunchAtWindowsStartup = chkLaunchAtSystemStartup.Checked;
            F16CPD.Util.SaveCurrentProperties();
        }
        private void chkStartWhenLaunched_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.StartWhenLaunched = chkStartWhenLaunched.Checked;
            F16CPD.Util.SaveCurrentProperties();

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            MinimizeToSystemTray();
            lblVersion.Text = "Version: " + Application.ProductVersion;
            nfyTrayIcon.Icon = this.Icon;
            foreach (string priority in Enum.GetNames(typeof(System.Threading.ThreadPriority)))
            {
                if (priority.ToLowerInvariant() != "highest")
                {
                    cbPriority.Items.Add(priority);
                }
            }
            cbOutputRotation.Items.Add("No rotation");
            cbOutputRotation.Items.Add("+90 degrees");
            cbOutputRotation.Items.Add("+180 degrees");
            cbOutputRotation.Items.Add("-90 degrees");
            cmdRescueOutputWindow.Enabled = false;
            LoadSettings();
            if (chkStartWhenLaunched.Checked)
            {
                Start();
            }
        }
        private void LoadSettings()
        {
            Properties.Settings.Default.Reload();
            chkStartWhenLaunched.Checked = Properties.Settings.Default.StartWhenLaunched;
            chkLaunchAtSystemStartup.Checked = Properties.Settings.Default.LaunchAtWindowsStartup;
            if (!Properties.Settings.Default.RunAsClient && !Properties.Settings.Default.RunAsServer)
            {
                rdoStandaloneMode.Checked = true;
            }
            rdoClientMode.Checked = Properties.Settings.Default.RunAsClient;
            rdoServerMode.Checked = Properties.Settings.Default.RunAsServer;
            txtServerIPAddress.Text = Properties.Settings.Default.ServerIPAddress;
            txtServerPortNum.Text = Properties.Settings.Default.ServerPortNum;
            nudPollFrequency.Value = Properties.Settings.Default.PollingFrequencyMillis;
            cbPriority.SelectedItem = Enum.GetName(typeof(System.Threading.ThreadPriority), Properties.Settings.Default.Priority);
            rdoNorth360.Checked = Properties.Settings.Default.DisplayNorthAsThreeSixZero;
            rdoNorth000.Checked = !Properties.Settings.Default.DisplayNorthAsThreeSixZero;
            RotateFlipType rotation = Properties.Settings.Default.Rotation;
            if (rotation == RotateFlipType.RotateNoneFlipNone)
            {
                cbOutputRotation.SelectedItem = "No rotation";
            }
            else if (rotation == RotateFlipType.Rotate90FlipNone)
            {
                cbOutputRotation.SelectedItem = "+90 degrees";
            }
            else if (rotation == RotateFlipType.Rotate180FlipNone)
            {
                cbOutputRotation.SelectedItem = "+180 degrees";
            }
            else if (rotation == RotateFlipType.Rotate270FlipNone)
            {
                cbOutputRotation.SelectedItem = "-90 degrees";
            }
            nudCourseHeadingAdjustmentSpeed.Value = Math.Min((int)Properties.Settings.Default.FastCourseAndHeadingAdjustSpeed, 5);
            if (Properties.Settings.Default.DisplayVerticalVelocityInDecimalThousands)
            {
                rdoVertVelocityInThousands.Checked = true;
                rdoVertVelocityInUnitFeet.Checked = false;
            }
            else
            {
                rdoVertVelocityInThousands.Checked = false;
                rdoVertVelocityInUnitFeet.Checked = true;
            }
            UpdateWindowsStartupRegKey();

        }
        private void rdoClientMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoClientMode.Checked)
            {
                txtServerIPAddress.Enabled = true;
                lblServerIPAddress.Enabled = true;
                txtServerPortNum.Enabled = true;
                lblPortNum.Enabled = true;
                grpPFDOptions.Enabled = true;
                Properties.Settings.Default.RunAsClient = true;
                Properties.Settings.Default.RunAsServer = false;
                F16CPD.Util.SaveCurrentProperties();
            }
        }

        private void rdoServerMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoServerMode.Checked)
            {
                txtServerIPAddress.Enabled = false;
                lblServerIPAddress.Enabled = false;
                txtServerPortNum.Enabled = true;
                lblPortNum.Enabled = true;
                grpPFDOptions.Enabled = false;
                cmdRescueOutputWindow.Enabled = false;
                Properties.Settings.Default.RunAsServer = true;
                Properties.Settings.Default.RunAsClient = false;
                F16CPD.Util.SaveCurrentProperties();
            }
        }

        private void MinimizeToSystemTray()
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.nfyTrayIcon.Visible = true;
            this.Hide();
        }
        private void RestoreFromSystemTray()
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.nfyTrayIcon.Visible = false;
            this.Show();
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            Start();
        }
        private void Stop()
        {
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Normal;
            btnStop.Enabled = false;
            cmdRescueOutputWindow.Enabled = false;
            mnuNfyStop.Enabled = false;
            if (_cpdEngine != null)
            {
                _cpdEngine.Stop();
                Common.Util.DisposeObject(_cpdEngine);
            }
            if (!Properties.Settings.Default.RunAsServer)
            {
                grpPFDOptions.Enabled = true;
                cbOutputRotation.Enabled = true;
            }

            gbNetworkingOptions.Enabled = true;
            gbStartupOptions.Enabled = true;
            gbPerformanceOptions.Enabled = true;

            cmdAssignInputs.Enabled = true;
            btnStart.Enabled = true;
            mnuNfyStart.Enabled = true;
            lblRotation.Enabled = true;

        }
        private void Start()
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

            int portNum = 21153;
            Int32.TryParse(Properties.Settings.Default.ServerPortNum, out portNum);
            btnStart.Enabled = false;
            mnuNfyStart.Enabled = false;
            if (_cpdEngine != null)
            {
                _cpdEngine.Stop();
                Common.Util.DisposeObject(_cpdEngine);
            }
            btnStart.Enabled = false;
            mnuNfyStart.Enabled = false;
            _cpdEngine = new F16CpdEngine();
            if (CommandLineSwitches != null)
            {
                foreach (string thisSwitch in CommandLineSwitches)
                {
                    if (!String.IsNullOrEmpty(thisSwitch))
                    {
                        if (thisSwitch.ToLowerInvariant() == "testmode")
                        {
                            _cpdEngine.TestMode = true;
                            break;
                        }
                    }
                }
            }
            lblRotation.Enabled = false;
            cbOutputRotation.Enabled = false;
            gbPerformanceOptions.Enabled = false;
            gbNetworkingOptions.Enabled = false;
            gbStartupOptions.Enabled = false;
            if (!Properties.Settings.Default.RunAsServer)
            {
                cmdRescueOutputWindow.Enabled = true;
            }
            cmdAssignInputs.Enabled = false;
            grpPFDOptions.Enabled = false;
            MinimizeToSystemTray();
            btnStop.Enabled = true;
            mnuNfyStop.Enabled = true;
            _cpdEngine.Start();
        }

        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                MinimizeToSystemTray();
            }
            else if (this.WindowState != FormWindowState.Minimized)
            {
                RestoreFromSystemTray();
            }
        }

        private void mnuNfyOptions_Click(object sender, EventArgs e)
        {
            RestoreFromSystemTray();
        }

        private void mnuNfyStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void mnuNfyStart_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void mnuNfyExit_Click(object sender, EventArgs e)
        {
            Quit();
        }
        private void Quit()
        {
            LogManager.Shutdown();
            try
            {
                this.nfyTrayIcon.Visible = false;
            }
            catch (Exception e)
            {
                _log.Debug(e.Message, e);
            }
            try
            {
                this.Dispose();
            }
            catch (Exception e)
            {
                _log.Debug(e.Message, e);
            }
            try
            {
                Application.Exit();
            }
            catch (Exception e)
            {
                _log.Debug(e.Message, e);
            }
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception e)
            {
                _log.Debug(e.Message, e);
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Quit();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void txtServerIPAddress_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.ServerIPAddress = txtServerIPAddress.Text;
            F16CPD.Util.SaveCurrentProperties();
        }


        private void cbPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            System.Threading.ThreadPriority selectedPriority = (System.Threading.ThreadPriority)Enum.Parse(typeof(System.Threading.ThreadPriority), (string)cbPriority.SelectedItem);
            Properties.Settings.Default.Priority = selectedPriority;
            F16CPD.Util.SaveCurrentProperties();
        }

        private void nudPollFrequency_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PollingFrequencyMillis = (int)nudPollFrequency.Value;
            F16CPD.Util.SaveCurrentProperties();
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
                F16CPD.Util.SaveCurrentProperties();
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

        private void rdoStandaloneMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoStandaloneMode.Checked)
            {
                txtServerIPAddress.Enabled = false;
                lblServerIPAddress.Enabled = false;
                txtServerPortNum.Enabled = false;
                lblPortNum.Enabled = false;
                grpPFDOptions.Enabled = true;
                Properties.Settings.Default.RunAsClient = false;
                Properties.Settings.Default.RunAsServer = false;
                F16CPD.Util.SaveCurrentProperties();
            }

        }
        #region Destructors
        /// <summary>
        /// Standard finalizer, which will call Dispose() if this object is not
        /// manually disposed.  Ordinarily called only by the garbage collector.
        /// </summary>
        ~frmMain()
        {
            if (nfyTrayIcon != null)
            {
                nfyTrayIcon.Visible = false;
            }
            Dispose();
        }
        /// <summary>
        /// Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">flag to indicate if we should actually perform disposal.  Distinguishes the private method signature from the public signature.</param>
        private void Dispose(bool disposing, bool myDispose) //bool myDispose is there to differentiate this method from another that it would otherwise hide
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //dispose of managed resources here
                    Common.Util.DisposeObject(_cpdEngine);
                }
            }
            // Code to dispose the un-managed resources of the class
            _isDisposed = true;

        }
        /// <summary>
        /// Public implementation of IDisposable.Dispose().  Cleans up managed
        /// and unmanaged resources used by this object before allowing garbage collection
        /// </summary>
        public new void Dispose()
        {
            Dispose(true, true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private void cmdAssignInputs_Click(object sender, EventArgs e)
        {
            frmInputs inputsForm = new frmInputs();
            inputsForm.ShowDialog(this);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                MinimizeToSystemTray();
            }

        }

        private void rdoNorth360_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayNorthAsThreeSixZero = rdoNorth360.Checked;
            F16CPD.Util.SaveCurrentProperties();
        }

        private void rdoNorth000_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayNorthAsThreeSixZero = !rdoNorth000.Checked;
            F16CPD.Util.SaveCurrentProperties();
        }

        private void cmdRescueOutputWindow_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this, "This will move the output window to the upper left corner of the current monitor, and will reset the window size to the default size.  Would you like to continue?", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.OK)
            {

                RotateFlipType rotation = Properties.Settings.Default.Rotation;
                Size newSize = Size.Empty;
                if (rotation == RotateFlipType.RotateNoneFlipNone || rotation == RotateFlipType.Rotate180FlipNone)
                {
                    newSize = new Size(800, 600);
                }
                else
                {
                    newSize = new Size(600, 800);
                }
                Screen thisScreen = Screen.FromPoint(this.Location);
                _cpdEngine.Location = thisScreen.WorkingArea.Location;
                _cpdEngine.Size = newSize;

                RestoreFromSystemTray();
            }
        }

        private void cbOutputRotation_SelectedIndexChanged(object sender, EventArgs e)
        {
            RotateFlipType oldRotation = Properties.Settings.Default.Rotation;
            RotateFlipType newRotation = RotateFlipType.RotateNoneFlipNone;
            if ((string)cbOutputRotation.SelectedItem == "No rotation")
            {
                newRotation = RotateFlipType.RotateNoneFlipNone;
            }
            else if ((string)cbOutputRotation.SelectedItem == "+90 degrees")
            {
                newRotation = RotateFlipType.Rotate90FlipNone;
            }
            else if ((string)cbOutputRotation.SelectedItem == "+180 degrees")
            {
                newRotation = RotateFlipType.Rotate180FlipNone;
            }
            else if ((string)cbOutputRotation.SelectedItem == "-90 degrees")
            {
                newRotation = RotateFlipType.Rotate270FlipNone;
            }
            Properties.Settings.Default.Rotation = newRotation;

            int oldWidth = Properties.Settings.Default.CpdWindowWidth;
            int oldHeight = Properties.Settings.Default.CpdWindowHeight;
            int newWidth = oldWidth;
            int newHeight = oldHeight;

            if (newRotation == RotateFlipType.RotateNoneFlipNone || newRotation == RotateFlipType.Rotate180FlipNone)
            {
                newWidth = Math.Min(oldHeight, oldWidth);
                newHeight = Math.Max(oldHeight, oldWidth);
            }
            else
            {
                newWidth = Math.Max(oldHeight, oldWidth);
                newHeight = Math.Min(oldHeight, oldWidth);
            }
            Properties.Settings.Default.CpdWindowWidth = newWidth;
            Properties.Settings.Default.CpdWindowHeight = newHeight;

            F16CPD.Util.SaveCurrentProperties();
        }

        private void nudCourseHeadingAdjustmentSpeed_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.FastCourseAndHeadingAdjustSpeed = (int)nudCourseHeadingAdjustmentSpeed.Value;
            F16CPD.Util.SaveCurrentProperties();
        }

        private void rdoVertVelocityInThousands_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoVertVelocityInThousands.Checked)
            {
                Properties.Settings.Default.DisplayVerticalVelocityInDecimalThousands = true;
            }
            else
            {
                Properties.Settings.Default.DisplayVerticalVelocityInDecimalThousands = false;
            }
            F16CPD.Util.SaveCurrentProperties();
        }

        private void rdoVertVelocityInUnitFeet_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoVertVelocityInThousands.Checked)
            {
                Properties.Settings.Default.DisplayVerticalVelocityInDecimalThousands = true;
            }
            else
            {
                Properties.Settings.Default.DisplayVerticalVelocityInDecimalThousands = false;
            }
            F16CPD.Util.SaveCurrentProperties();
        }



    }
}
