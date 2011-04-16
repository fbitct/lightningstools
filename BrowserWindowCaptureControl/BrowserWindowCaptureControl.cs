using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BrowserWindowCaptureControl
{

    #region IObjectSafety

    [Serializable]
    [ComVisible(true)]
    public enum ObjectSafetyOptions
    {
        INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001,
        INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002,
        INTERFACE_USES_DISPEX = 0x00000004,
        INTERFACE_USES_SECURITY_MANAGER = 0x00000008
    } ;

    //
    // MS IObjectSafety Interface definition
    //
    [ComImport]
    [Guid("CB5BDC81-93C1-11CF-8F20-00805F2CD064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IObjectSafety
    {
        [PreserveSig]
        long GetInterfaceSafetyOptions(ref Guid iid, out int pdwSupportedOptions, out int pdwEnabledOptions);

        [PreserveSig]
        long SetInterfaceSafetyOptions(ref Guid iid, int dwOptionSetMask, int dwEnabledOptions);
    } ;

    #endregion

    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("E578C92E-E449-11DE-AE26-6B6D56D89593")]
    public interface IBrowserWindowCaptureControl
    {
        [DispId(1)]
        string CaptureBrowserWindow();

        string ImageFormat { get; set; }
    }

    [ComVisible(true)]
    [Guid("09683C2A-E44A-11DE-9138-9C7D56D89593")]
    [ProgId("BrowserWindowCaptureControl.BrowserWindowCaptureControl")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof (IBrowserWindowCaptureControl))]
    [ComDefaultInterface(typeof (IBrowserWindowCaptureControl))]
    public partial class BrowserWindowCaptureControl : UserControl, IBrowserWindowCaptureControl, IObjectSafety
    {
        //marks our code as "Safe for Scripting" so IE doesn't whine

        private ImageFormat _imageFormat = System.Drawing.Imaging.ImageFormat.Png;

        private ObjectSafetyOptions m_options =
            ObjectSafetyOptions.INTERFACESAFE_FOR_UNTRUSTED_CALLER |
            ObjectSafetyOptions.INTERFACESAFE_FOR_UNTRUSTED_DATA;

        public BrowserWindowCaptureControl()
        {
            InitializeComponent();
            Size = new Size(0, 0);
        }

        #region IBrowserWindowCaptureControl Members

        public string ImageFormat
        {
            get { return _imageFormat.ToString(); }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                value = value.Trim();
                while (value.StartsWith("."))
                {
                    value = value.Substring(1, value.Length - 1);
                }
                value = value.ToUpperInvariant();
                ImageFormat realFormat = null;
                switch (value)
                {
                    case "EMF":
                        realFormat = System.Drawing.Imaging.ImageFormat.Emf;
                        break;
                    case "EXIF":
                        realFormat = System.Drawing.Imaging.ImageFormat.Exif;
                        break;
                    case "GIF":
                        realFormat = System.Drawing.Imaging.ImageFormat.Gif;
                        break;
                    case "ICO":
                        realFormat = System.Drawing.Imaging.ImageFormat.Icon;
                        break;
                    case "ICON":
                        realFormat = System.Drawing.Imaging.ImageFormat.Icon;
                        break;
                    case "JPG":
                        realFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                    case "JPEG":
                        realFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                    case "MEMORYBITMAP":
                        realFormat = System.Drawing.Imaging.ImageFormat.MemoryBmp;
                        break;
                    case "MEMORYBMP":
                        realFormat = System.Drawing.Imaging.ImageFormat.MemoryBmp;
                        break;
                    case "PNG":
                        realFormat = System.Drawing.Imaging.ImageFormat.Png;
                        break;
                    case "TIFF":
                        realFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                        break;
                    case "TIF":
                        realFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                        break;
                    case "WMF":
                        realFormat = System.Drawing.Imaging.ImageFormat.Wmf;
                        break;
                    case "BMP":
                        realFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                        break;
                    default:
                        throw new ArgumentException("value");
                }

                _imageFormat = realFormat;
            }
        }

        public string CaptureBrowserWindow()
        {
            var toReturn = new StringBuilder();
            try
            {
                //get rectangle representing this ActiveX control's client area, relative to the entire desktop window's upper left edge
                var clientRect = new NativeMethods.RECT();
                NativeMethods.GetClientRect(Handle, out clientRect);

                //get the window handle of the control that owns this screen real estate
                IntPtr containerHwnd =
                    NativeMethods.WindowFromPoint(new NativeMethods.POINT {x = clientRect.x, y = clientRect.y});
                int lastError = Marshal.GetLastWin32Error();

                //get the window handle of the rootmost window that owns this control's container control
                IntPtr rootWindowHwnd = NativeMethods.GetAncestor(Handle, NativeMethods.GA_ROOT);

                //get the rectangle representing this control's parent window's client area, relative to the entire desktop window's upper left edge
                var rootWindowClientRect = new NativeMethods.RECT();
                NativeMethods.GetClientRect(rootWindowHwnd, out rootWindowClientRect);
                lastError = Marshal.GetLastWin32Error();

                byte[] imageBytes = null;
                using (var bmp = new Bitmap(rootWindowClientRect.width, rootWindowClientRect.height))
                using (var ms = new MemoryStream())
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    //screen-capture this control's parent window's entire client area
                    g.CopyFromScreen(new Point(rootWindowClientRect.x, rootWindowClientRect.y), new Point(0, 0),
                                     new Size(rootWindowClientRect.width, rootWindowClientRect.height));
                    bmp.Save(ms, _imageFormat);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    imageBytes = ms.ToArray();
                }
                toReturn.Append(Convert.ToBase64String(imageBytes));
            }
            catch (Exception e)
            {
                toReturn.Append(e.ToString());
            }
            return toReturn.ToString();
        }

        #endregion

        #region IObjectSafety Members

        public long GetInterfaceSafetyOptions(ref Guid iid, out int pdwSupportedOptions, out int pdwEnabledOptions)
        {
            pdwSupportedOptions = (int) m_options;
            pdwEnabledOptions = (int) m_options;
            return 0;
        }

        public long SetInterfaceSafetyOptions(ref Guid iid, int dwOptionSetMask, int dwEnabledOptions)
        {
            return 0;
        }

        #endregion
    }
}