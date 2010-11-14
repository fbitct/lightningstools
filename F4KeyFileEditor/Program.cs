#region Using statements
using System.Windows.Forms;
using System;
using Microsoft.VisualBasic.ApplicationServices;
using System.Threading;
using System.Diagnostics;
using log4net;
using Common.Application;
#endregion

namespace F4KeyFileEditor
{
    /// <summary>
    /// Main program class.  Contains the startup method for the F4 KeyFile Editor application.
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
            LogException((Exception)e.ExceptionObject);
        }
        private static void App_UnhandledException(object sender, Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs e)
        {
            LogException((Exception)e.Exception);
            e.ExitApplication = false;
        }
        private static void UIThreadException(object sender, ThreadExceptionEventArgs t)
        {
            LogException((Exception)t.Exception);
        }
        private static void LogException(Exception e)
        {
            if (e is ThreadAbortException || e is ThreadInterruptedException) return;
            _log.Error(e.Message, e);
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += new ThreadExceptionEventHandler(UIThreadException);

            // Set the unhandled exception mode to force all Windows Forms errors to go through
            // our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException +=
                new System.UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");
            Thread.CurrentThread.Name = "MainThread";

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
            // ensure only a single instance of this app runs.
            SingleInstanceApplication app = new SingleInstanceApplication();
            app.StartupNextInstance += new StartupNextInstanceEventHandler(OnAppStartupNextInstance);
            app.UnhandledException += new Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventHandler(App_UnhandledException);

            mainForm = new frmMain();
            app.Run(mainForm);
        }

        /// <summary>
        /// Event handler for processing when the another application instance tries
        /// to startup. Bring the previous instance of the app to the front and 
        /// process any command-line that's needed.
        /// </summary>
        /// <param name="sender">Object sending this message.</param>
        /// <param name="e">Event argument for this message.</param>
        private static void OnAppStartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            e.BringToForeground = true;
        }

        #endregion
    }
}