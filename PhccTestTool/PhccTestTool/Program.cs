﻿using System;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace PhccTestTool
{
    /// <summary>
    /// Main program class.  Contains the startup method for the application.
    /// </summary>
    public static class Program
    {
        #region Class variable declarations
        // private members
        private static Form mainForm;
        #endregion
        #region Static methods
        private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            Debug.Write(e.ExceptionObject);
        }
        private static void App_UnhandledException(object sender, Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs e)
        {
            e.ExitApplication = false;
        }
        private static void UIThreadException(object sender, ThreadExceptionEventArgs t)
        {
            Debug.WriteLine(t.Exception);
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
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");
            mainForm = new frmMain();
            Thread.CurrentThread.Name = "MainThread";
            Application.EnableVisualStyles();
            Application.Run(mainForm);
        }

        #endregion
    }
}
