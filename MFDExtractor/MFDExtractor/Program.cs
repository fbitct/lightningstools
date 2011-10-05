#region Using statements
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using MFDExtractor.UI;
using log4net;
#endregion

namespace MFDExtractor
{
    /// <summary>
    /// Main program class.  Contains the startup method for the application.
    /// </summary>
    public static class Program
    {
        #region Class variable declarations
        // private members
        private static Form mainForm;
        private static ILog _log = LogManager.GetLogger(typeof(Program));
        #endregion
        #region Static methods
        private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            _log.Error(e.ExceptionObject.ToString(), (Exception)e.ExceptionObject);
        }
        private static void App_UnhandledException(object sender, Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs e)
        {
            _log.Error(e.Exception.Message.ToString(), e.Exception);
            e.ExitApplication = false;
        }
        private static void UIThreadException(object sender, ThreadExceptionEventArgs t)
        {
            _log.Error(t.Exception.Message.ToString(), t.Exception);
        }

        private static Process PriorProcess()
        // Returns a System.Diagnostics.Process pointing to
        // a pre-existing process with the same name as the
        // current one, if any; or null if the current process
        // is unique.
        {
            Process curr = Process.GetCurrentProcess();
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if ((p.Id != curr.Id) &&
                    (p.MainModule.FileName == curr.MainModule.FileName))
                    return p;
            }
            return null;
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (PriorProcess() != null)
            {
                return;
            }

            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += new ThreadExceptionEventHandler(UIThreadException);

            // Set the unhandled exception mode to force all Windows Forms errors to go through
            // our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException +=
                new System.UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture= new System.Globalization.CultureInfo("en-us");
            mainForm = new frmMain();
            Thread.CurrentThread.Name = "MainThread";
            Application.EnableVisualStyles();

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                try
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpgradeNeeded = false;
                    Properties.Settings.Default.Save();
                }
                catch (Exception e)
                {
                    Properties.Settings.Default.Reset();
                    Properties.Settings.Default.UpgradeNeeded = false;
                    Properties.Settings.Default.Save();
                    MessageBox.Show("Error: Could not import settings from previous installation of " + Application.ProductName + ".\nThis can happen if the configuration file was incorrectly edited by hand.\nDefault settings will be used instead.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);  
                    _log.Error(e.Message, e);
                }
            }
        Application.Run(mainForm);
        }

        #endregion
    }
}