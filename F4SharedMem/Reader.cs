using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using F4SharedMem.Headers;
using F4SharedMem.Win32;

namespace F4SharedMem
{
    [ComVisible(true)]
    public enum FalconDataFormats
    {
        OpenFalcon = 0,
        BMS3 = 0,
        BMS2 = 1,
        AlliedForce = 2,
        BMS4 = 3,
        FreeFalcon5 = 4
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public sealed class Reader : IDisposable
    {
        private const string OSB_SHARED_MEMORY_AREA_FILE_NAME = "FalconSharedOsbMemoryArea";
        private FalconDataFormats _dataFormat;
        private bool _disposed;
        private IntPtr _hOsbSharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hPrimarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hSecondarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpOsbSharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpPrimarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpSecondarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private const string PRIMARY_SHARED_MEMORY_AREA_FILE_NAME = "FalconSharedMemoryArea";
        private const string SECONDARY_SHARED_MEMORY_FILE_NAME = "FalconSharedMemoryArea2";

        public Reader()
        {
        }

        public Reader(FalconDataFormats dataFormat)
        {
            _dataFormat = dataFormat;
        }

        public bool IsFalconRunning
        {
            get
            {
                try
                {
                    ConnectToFalcon();
                    if (_lpPrimarySharedMemoryAreaBaseAddress != IntPtr.Zero)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public FalconDataFormats DataFormat
        {
            get { return _dataFormat; }
            set { _dataFormat = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        [ComVisible(false)]
        public byte[] GetRawOSBData()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var bytesRead = new List<byte>();
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                long fileSizeBytes = GetMaxMemFileSize(_lpOsbSharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof (OSBData))) fileSizeBytes = Marshal.SizeOf(typeof (OSBData));
                for (int i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpOsbSharedMemoryAreaBaseAddress, i));
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            byte[] toReturn = bytesRead.ToArray();
            if (toReturn.Length == 0)
            {
                return null;
            }
            return toReturn;
        }

        private static long GetMaxMemFileSize(IntPtr pMemAreaBaseAddr)
        {
            var mbi = new NativeMethods.MEMORY_BASIC_INFORMATION();
            NativeMethods.VirtualQuery(ref pMemAreaBaseAddr, ref mbi, new IntPtr(Marshal.SizeOf(mbi)));
            return mbi.RegionSize.ToInt64();
        }

        [ComVisible(false)]
        public byte[] GetRawFlightData2()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var bytesRead = new List<byte>();
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                long fileSizeBytes = GetMaxMemFileSize(_lpSecondarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof (BMS4FlightData2)))
                    fileSizeBytes = Marshal.SizeOf(typeof (BMS4FlightData2));
                for (int i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpSecondarySharedMemoryAreaBaseAddress, i));
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            byte[] toReturn = bytesRead.ToArray();
            if (toReturn.Length == 0)
            {
                return null;
            }
            return toReturn;
        }

        [ComVisible(false)]
        public byte[] GetRawPrimaryFlightData()
        {
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var bytesRead = new List<byte>();
            if (!_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                long fileSizeBytes = GetMaxMemFileSize(_lpPrimarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof (BMS4FlightData)))
                    fileSizeBytes = Marshal.SizeOf(typeof (BMS4FlightData));
                for (int i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpPrimarySharedMemoryAreaBaseAddress, i));
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            byte[] toReturn = bytesRead.ToArray();
            if (toReturn.Length == 0)
            {
                return null;
            }
            return toReturn;
        }

        public FlightData GetCurrentData()
        {
            Type dataType = null;
            switch (_dataFormat)
            {
                case FalconDataFormats.AlliedForce:
                    dataType = typeof (AFFlightData);
                    break;
                case FalconDataFormats.BMS2:
                    dataType = typeof (BMS2FlightData);
                    break;
                case FalconDataFormats.BMS3:
                    dataType = typeof (BMS3FlightData);
                    break;
                case FalconDataFormats.BMS4:
                    dataType = typeof (BMS4FlightData);
                    break;
                case FalconDataFormats.FreeFalcon5:
                    dataType = typeof (FreeFalcon5FlightData);
                    break;
                default:
                    break;
            }
            if (dataType == null)
            {
                return null;
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            object data = Convert.ChangeType(Marshal.PtrToStructure(_lpPrimarySharedMemoryAreaBaseAddress, dataType),
                                             dataType);

            FlightData toReturn = null;
            switch (_dataFormat)
            {
                case FalconDataFormats.AlliedForce:
                    toReturn = new FlightData((AFFlightData) data);
                    break;
                case FalconDataFormats.BMS2:
                    toReturn = new FlightData((BMS2FlightData) data);
                    break;
                case FalconDataFormats.BMS3:
                    toReturn = new FlightData((BMS3FlightData) data);
                    break;
                case FalconDataFormats.BMS4:
                    toReturn = new FlightData((BMS4FlightData) data);
                    break;
                case FalconDataFormats.FreeFalcon5:
                    toReturn = new FlightData((FreeFalcon5FlightData) data);
                    break;
                default:
                    break;
            }
            if (toReturn == null)
            {
                return null;
            }
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = _dataFormat == FalconDataFormats.BMS4 ? (Marshal.PtrToStructure(_lpSecondarySharedMemoryAreaBaseAddress, typeof (BMS4FlightData2))) : (Marshal.PtrToStructure(_lpSecondarySharedMemoryAreaBaseAddress, typeof (FlightData2)));
                toReturn.PopulateFromStruct(data);
            }
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (Marshal.PtrToStructure(_lpOsbSharedMemoryAreaBaseAddress, typeof (OSBData)));
                toReturn.PopulateFromStruct(data);
            }
            toReturn.DataFormat = _dataFormat;
            return toReturn;
        }

        private void ConnectToFalcon()
        {
            Disconnect();
            _hPrimarySharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ,
                                                                                       false,
                                                                                       PRIMARY_SHARED_MEMORY_AREA_FILE_NAME);
            _lpPrimarySharedMemoryAreaBaseAddress =
                NativeMethods.MapViewOfFile(_hPrimarySharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ,
                                            0, 0, IntPtr.Zero);
            if (_dataFormat == FalconDataFormats.OpenFalcon || _dataFormat == FalconDataFormats.BMS4)
            {
                _hSecondarySharedMemoryAreaFileMappingObject =
                    NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false, SECONDARY_SHARED_MEMORY_FILE_NAME);
                _lpSecondarySharedMemoryAreaBaseAddress =
                    NativeMethods.MapViewOfFile(_hSecondarySharedMemoryAreaFileMappingObject,
                                                NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);
                _hOsbSharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ,
                                                                                       false,
                                                                                       OSB_SHARED_MEMORY_AREA_FILE_NAME);
                _lpOsbSharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hOsbSharedMemoryAreaFileMappingObject,
                                                                                NativeMethods.SECTION_MAP_READ, 0, 0,
                                                                                IntPtr.Zero);
            }
        }

        private void Disconnect()
        {
            if (!_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpPrimarySharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hPrimarySharedMemoryAreaFileMappingObject);
            }
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpSecondarySharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hSecondarySharedMemoryAreaFileMappingObject);
            }
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                NativeMethods.UnmapViewOfFile(_lpOsbSharedMemoryAreaBaseAddress);
                NativeMethods.CloseHandle(_hOsbSharedMemoryAreaFileMappingObject);
            }
        }

        internal void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }

                _disposed = true;
            }
        }
    }
}