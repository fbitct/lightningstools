using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using log4net;
using Microsoft.VisualBasic.Devices;
using Common.HardwareSupport;
using Common.SimSupport;

namespace SimLinkup.UI
{
    public partial class frmOptions : Form
    {
        private static ILog _log = LogManager.GetLogger(typeof(frmOptions));

        public frmOptions()
        {
            InitializeComponent();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private bool ValidateSettings()
        {
            return true;
        }
        private void cmdOK_Click(object sender, EventArgs e)
        {
            bool valid = ValidateSettings();
            if (valid)
            {
                SaveSettings();
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            this.Text = Application.ProductName + " v" + Application.ProductVersion + " Options";
            LoadSettings();
            DiscoverPlugins();
        }
        private void DiscoverPlugins()
        {
            IHardwareSupportModule[] hsms = Runtime.Runtime.GetRegisteredHardwareSupportModules();
            StringBuilder hsmSb = new StringBuilder();
            if (hsms != null)
            {
                foreach (var hsm in hsms)
                {
                    if (hsm != null)
                    {
                        hsmSb.Append("\u2022 " +hsm.FriendlyName + "\n");
                    }
                }
            }
            lblHardwareSupportModules.Text = hsmSb.ToString();


            SimSupportModule[] ssms = Runtime.Runtime.GetRegisteredSimSupportModules();
            StringBuilder ssmSb = new StringBuilder();
            if (ssms!= null)
            {
                foreach (var ssm in ssms)
                {
                    if (ssm != null)
                    {
                        ssmSb.Append("\u2022 " + ssm.FriendlyName + "\n");
                    }
                }
            }
            lblSimSupportModules.Text = ssmSb.ToString();

        }
        private void LoadSettings()
        {
            chkLaunchAtSystemStartup.Checked = Properties.Settings.Default.LaunchAtWindowsStartup;
            chkMinimizeToSystemTray.Checked = Properties.Settings.Default.MinimizeToSystemTray;
            chkMinimizeWhenStarted.Checked = Properties.Settings.Default.MinimizeWhenStarted;
            chkStartAutomaticallyWhenLaunched.Checked = Properties.Settings.Default.StartRunningWhenLaunched;
        }
        private void SaveSettings()
        {
            Properties.Settings.Default.StartRunningWhenLaunched = chkStartAutomaticallyWhenLaunched.Checked;
            Properties.Settings.Default.MinimizeToSystemTray = chkMinimizeToSystemTray.Checked;
            Properties.Settings.Default.MinimizeWhenStarted = chkMinimizeWhenStarted.Checked;
            UpdateWindowsStartupRegKey();
            Properties.Settings.Default.Save();    
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
                    _log.Debug(ex.Message, ex);
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
                    _log.Debug(ex.Message, ex);
                }
            }
            Properties.Settings.Default.LaunchAtWindowsStartup = chkLaunchAtSystemStartup.Checked;
            Properties.Settings.Default.Save();
        }

    }
}
