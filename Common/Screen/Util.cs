using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Common.Win32;
using log4net;

namespace Common.Screen
{
    public static class Util
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Util));
        private static object sync = false;

        public static Bitmap CaptureScreenRectangle(Rectangle sourceRectangle)
        {
            if (sourceRectangle == Rectangle.Empty)
            {
                return null;
            }
            Bitmap toReturn = null;
            try
            {
                toReturn = new Bitmap(sourceRectangle.Width, sourceRectangle.Height);
                using (Graphics g = Graphics.FromImage(toReturn))
                {
                    g.CopyFromScreen(sourceRectangle.X, sourceRectangle.Y, 0, 0, sourceRectangle.Size);
                }
            }
            catch (Win32Exception e)
            {
                _log.Debug(e.Message, e);
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return toReturn;
        }

        public static string CleanDeviceName(string deviceName)
        {
            if (deviceName == null) return null;
            int firstNull = deviceName.IndexOf('\0');
            if (firstNull >= 0)
            {
                return deviceName.Substring(0, firstNull).Trim();
            }
            else
            {
                return deviceName;
            }
        }

        public static System.Windows.Forms.Screen FindScreen(string deviceName)
        {
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (CleanDeviceName(screen.DeviceName) == CleanDeviceName(deviceName))
                {
                    return screen;
                }
            }
            return System.Windows.Forms.Screen.PrimaryScreen;
        }

        public static void OpenFormOnSpecificMonitor(Form formToOpen, Form parentForm,
                                                     System.Windows.Forms.Screen screen, Point point, Size size,
                                                     bool hideFromTaskBar, bool hideFromAltTab)
        {
            OpenFormOnSpecificMonitor(formToOpen, parentForm, screen, point, size, hideFromTaskBar, hideFromAltTab,
                                      false);
        }

        public static void OpenFormOnSpecificMonitor(Form formToOpen, Form parentForm,
                                                     System.Windows.Forms.Screen screen, Point point, Size size,
                                                     bool hideFromTaskBar, bool hideFromAltTab,
                                                     bool makeCurrentThreadIntoUIThread)
        {
            OpenFormOnSpecificMonitor(formToOpen, parentForm, screen, point, size);
            if (hideFromTaskBar)
            {
                formToOpen.ShowInTaskbar = false;
            }
            if (hideFromAltTab)
            {
                formToOpen.ShowIcon = false;
                formToOpen.ShowInTaskbar = false;
            }
            if (hideFromAltTab)
            {
                NativeMethods.SetWindowLong(formToOpen.Handle, NativeMethods.GWL_EXSTYLE,
                                            (NativeMethods.GetWindowLong(formToOpen.Handle,
                                                                         NativeMethods.GWL_EXSTYLE) |
                                             NativeMethods.WS_EX_TOOLWINDOW) & ~NativeMethods.WS_EX_APPWINDOW);
            }
        }

        public static void OpenFormOnSpecificMonitor(Form formToOpen, Form parentForm,
                                                     System.Windows.Forms.Screen screen, Point point, Size size)
        {
            OpenFormOnSpecificMonitor(formToOpen, screen, ref point, ref size, false);
        }

        private static void OpenFormOnSpecificMonitor(Form formToOpen, System.Windows.Forms.Screen screen,
                                                      ref Point point, ref Size size, bool makeCurrentThreadIntoUIThread)
        {
            // Set the StartPosition to Manual otherwise the system will assign an automatic start position
            formToOpen.StartPosition = FormStartPosition.Manual;
            // Set the form location so it appears at Location (x, y) on the specified screen 
            Point l = screen.Bounds.Location;
            l.Offset(point);
            formToOpen.DesktopLocation = l;
            // Show the form
            formToOpen.Size = size;
            //formToOpen.Show(parentForm);
            if (makeCurrentThreadIntoUIThread)
            {
                System.Windows.Forms.Application.UseWaitCursor = false;
                System.Windows.Forms.Application.VisualStyleState = VisualStyleState.NoneEnabled;
                System.Windows.Forms.Application.Run(formToOpen);
            }
            else
            {
                formToOpen.Show();
            }
            formToOpen.Size = size;
            formToOpen.Owner = null;
        }
    }
}