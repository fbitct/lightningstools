using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing;

namespace UrlToImage
{
    internal class NativeMethods
    {
        public const int SRCCOPY = 0xCC0020;

        [DllImport("user32.dll", SetLastError=true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(
             IntPtr hdcDest, // handle to destination DC
             int nXDest, // x-coord of destination upper-left corner
             int nYDest, // y-coord of destination upper-left corner
             int nWidth, // width of destination rectangle
             int nHeight, // height of destination rectangle
             IntPtr hdcSrc, // handle to source DC
             int nXSrc, // x-coordinate of source upper-left corner
             int nYSrc, // y-coordinate of source upper-left corner
             int dwRop // raster operation code
        );

        
    }
    public enum HRESULTS : long
    {
        S_OK = 0,
        S_FALSE = 1,

        E_NOTIMPL = 0x80004001,
        E_OUTOFMEMORY = 0x8007000E,
        E_INVALIDARG = 0x80070057,
        E_NOINTERFACE = 0x80004002,
        E_POINTER = 0x80004003,
        E_HANDLE = 0x80070006,
        E_ABORT = 0x80004004,
        E_FAIL = 0x80004005,
        E_ACCESSDENIED = 0x80070005,

        // IConnectionPoint errors

        CONNECT_E_FIRST = 0x80040200,
        CONNECT_E_NOCONNECTION,  // there is no connection for this connection id
        CONNECT_E_ADVISELIMIT,   // this implementation's limit for advisory connections has been reached
        CONNECT_E_CANNOTCONNECT, // connection attempt failed
        CONNECT_E_OVERRIDDEN,    // must use a derived interface to connect

        // DllRegisterServer/DllUnregisterServer errors
        SELFREG_E_TYPELIB = 0x80040200, // failed to register/unregister type library
        SELFREG_E_CLASS,        // failed to register/unregister class

        // INET errors

        INET_E_INVALID_URL = 0x800C0002,
        INET_E_NO_SESSION = 0x800C0003,
        INET_E_CANNOT_CONNECT = 0x800C0004,
        INET_E_RESOURCE_NOT_FOUND = 0x800C0005,
        INET_E_OBJECT_NOT_FOUND = 0x800C0006,
        INET_E_DATA_NOT_AVAILABLE = 0x800C0007,
        INET_E_DOWNLOAD_FAILURE = 0x800C0008,
        INET_E_AUTHENTICATION_REQUIRED = 0x800C0009,
        INET_E_NO_VALID_MEDIA = 0x800C000A,
        INET_E_CONNECTION_TIMEOUT = 0x800C000B,
        INET_E_INVALID_REQUEST = 0x800C000C,
        INET_E_UNKNOWN_PROTOCOL = 0x800C000D,
        INET_E_SECURITY_PROBLEM = 0x800C000E,
        INET_E_CANNOT_LOAD_DATA = 0x800C000F,
        INET_E_CANNOT_INSTANTIATE_OBJECT = 0x800C0010,
        INET_E_USE_DEFAULT_PROTOCOLHANDLER = 0x800C0011,
        INET_E_DEFAULT_ACTION = 0x800C0011,
        INET_E_USE_DEFAULT_SETTING = 0x800C0012,
        INET_E_QUERYOPTION_UNKNOWN = 0x800C0013,
        INET_E_REDIRECT_FAILED = 0x800C0014,//INET_E_REDIRECTING
        INET_E_REDIRECT_TO_DIR = 0x800C0015,
        INET_E_CANNOT_LOCK_REQUEST = 0x800C0016,
        INET_E_USE_EXTEND_BINDING = 0x800C0017,
        INET_E_ERROR_FIRST = 0x800C0002,
        INET_E_ERROR_LAST = 0x800C0017,
        INET_E_CODE_DOWNLOAD_DECLINED = 0x800C0100,
        INET_E_RESULT_DISPATCHED = 0x800C0200,
        INET_E_CANNOT_REPLACE_SFP_FILE = 0x800C0300,

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct COMRECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

  /// <summary> 
  /// The IViewObject interface enables an object to display itself directly 
  /// without passing a data object to the caller. In addition, this interface 
  /// can create and manage a connection with an advise sink so the caller 
  /// can be notified of changes in the view object. 
  /// </summary> 
  /// <remarks>This is implemented through com import, because this 
  /// custom marshalling ensures correct work, the OLE.Interop implementation 
  /// seems not to be valid. Please google for more information.</remarks> 
  /// <seealso cref="Microsoft.VisualStudio.OLE.Interop.IViewObject"/> 
  [ComVisible(true), ComImport()] 
  [GuidAttribute("0000010d-0000-0000-C000-000000000046")] 
  [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)] 
  public interface IViewObject 
  { 
    /// <summary> 
    /// Draws a representation of an object onto the specified device context. 
    /// </summary> 
    /// <param name="drawAspect">Aspect to be drawn</param> 
    /// <param name="index">Part of the object of interest in the draw operation</param> 
    /// <param name="aspectPointer">Pointer to DVASPECTINFO structure or NULL</param> 
    /// <param name="ptd">Pointer to target device in a structure</param> 
    /// <param name="hdcTargetDev">Information context for the target device</param> 
    /// <param name="hdcDraw">Device context on which to draw</param> 
    /// <param name="lprcBounds">Ref. The <see cref="RECTL"/> in which the object is drawn.</param> 
    /// <param name="lprcWBounds">Ref. Pointer to the window extent and window origin 
    /// when drawing a metafile.</param> 
    /// <param name="pfnContinue">Pointer to the callback function for canceling 
    /// or continuing the drawing</param> 
    /// <param name="doContinue">Value to pass to the callback function</param> 
    /// <returns>An error code.</returns> 
    [return: MarshalAs(UnmanagedType.I4)] 
    [PreserveSig] 
    int Draw( 
      [MarshalAs(UnmanagedType.U4)] DVASPECT drawAspect, 
      int index, 
      IntPtr aspectPointer, 
      [In] IntPtr ptd, 
      IntPtr hdcTargetDev, 
      IntPtr hdcDraw, 
      [MarshalAs(UnmanagedType.Struct)] ref COMRECT lprcBounds,
      [MarshalAs(UnmanagedType.Struct)] ref COMRECT lprcWBounds, 
      IntPtr pfnContinue, 
      [MarshalAs(UnmanagedType.U4)] uint doContinue); 
 
    /// <summary> 
    /// Returns the logical palette that the object will use 
    /// for drawing in its IViewObject::Draw method with the corresponding parameters. 
    /// </summary> 
    /// <param name="drawAspect">How the object is to be represented</param> 
    /// <param name="index">Part of the object of interest in the draw operation</param> 
    /// <param name="aspectPointer">Always NULL</param> 
    /// <param name="ptd">Pointer to target device in a structure</param> 
    /// <param name="hicTargetDev">Information context for the target device</param> 
    /// <param name="colorSet">Out.Requested LOGPALETTE structure</param> 
    void GetColorSet( 
      [MarshalAs(UnmanagedType.U4)] uint drawAspect, 
      int index, 
      IntPtr aspectPointer, 
      [MarshalAs(UnmanagedType.Struct)] DVTARGETDEVICE ptd, 
      IntPtr hicTargetDev, 
      [Out, MarshalAs(UnmanagedType.Struct)] out LOGPALETTE colorSet); 
 
    /// <summary> 
    /// Freezes a certain aspect of the object's presentation so that 
    /// it does not change until the IViewObject::Unfreeze method is called. 
    /// The most common use of this method is for banded printing. 
    /// </summary> 
    /// <param name="drawAspect">How the object is to be represented</param> 
    /// <param name="index">Part of the object of interest in the draw operation</param> 
    /// <param name="aspectPointer">Always NULL</param> 
    /// <param name="pdwFreeze">Points to location containing an identifying key</param> 
    void Freeze( 
      [MarshalAs(UnmanagedType.U4)] uint drawAspect, 
      int index, 
      IntPtr aspectPointer, 
      out IntPtr pdwFreeze); 
 
    /// <summary> 
    /// Releases a previously frozen drawing. The most common use 
    /// of this method is for banded printing. 
    /// </summary> 
    /// <param name="freeze">Contains key that determines view object to unfreeze</param> 
    void Unfreeze([MarshalAs(UnmanagedType.U4)] int freeze); 
 
    /// <summary> 
    /// Sets up a connection between the view object and an advise 
    /// sink so that the advise sink can be notified about 
    /// changes in the object's view. 
    /// </summary> 
    /// <param name="aspects">View for which notification is being requested</param> 
    /// <param name="advf">Information about the advise sink</param> 
    /// <param name="adviseSink"><see cref="IAdviseSink"/> that is to receive change notifications</param> 
    void SetAdvise( 
      [MarshalAs(UnmanagedType.U4)] int aspects, 
      [MarshalAs(UnmanagedType.U4)] int advf, 
      [MarshalAs(UnmanagedType.Interface)] IAdviseSink adviseSink); 
 
    /// <summary> 
    /// Retrieves the existing advisory connection on the object if 
    /// there is one. This method simply returns the parameters used in 
    /// the most recent call to the IViewObject::SetAdvise method. 
    /// </summary> 
    /// <param name="paspects">Pointer to where dwAspect parameter from 
    /// previous SetAdvise call is returned</param> 
    /// <param name="advf">Pointer to where advf parameter from 
    /// previous SetAdvise call is returned</param> 
    /// <param name="adviseSink">Out. Receives the <see cref="IAdviseSink"/> interface</param> 
    void GetAdvise( 
      IntPtr paspects, 
      IntPtr advf, 
      [Out, MarshalAs(UnmanagedType.Interface)] out IAdviseSink adviseSink); 
  }
  [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct DVTARGETDEVICE
  {
      [MarshalAs(UnmanagedType.U4)]
      public int tdSize;
      [MarshalAs(UnmanagedType.U2)]
      public short tdDriverNameOffset;
      [MarshalAs(UnmanagedType.U2)]
      public short tdDeviceNameOffset;
      [MarshalAs(UnmanagedType.U2)]
      public short tdPortNameOffset;
      [MarshalAs(UnmanagedType.U2)]
      public short tdExtDevmodeOffset;
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]
      public byte[] tdData;
  }
  [StructLayout(LayoutKind.Sequential)]
  public struct PALETTEENTRY
  {
      public byte peRed;
      public byte peGreen;
      public byte peBlue;
      public byte peFlags;
      public int numColors;
  }
  [StructLayout(LayoutKind.Sequential)]
  public struct LOGPALETTE
  {
      public short PALVERSION;
      public int palNumEntries;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public PALETTEENTRY[] palPalEntry;
  }
}
