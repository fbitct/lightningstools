using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using F4SharedMem.Headers;
using F4SharedMem.Win32;

namespace F4SharedMem
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public sealed class Reader : IDisposable
    {
        private const string PrimarySharedMemoryAreaFileName = "FalconSharedMemoryArea";
        private const string SecondarySharedMemoryFileName = "FalconSharedMemoryArea2";
        private const string OsbSharedMemoryAreaFileName = "FalconSharedOsbMemoryArea";
        private bool _disposed;
        private IntPtr _hOsbSharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hPrimarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hSecondarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpOsbSharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpPrimarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpSecondarySharedMemoryAreaBaseAddress = IntPtr.Zero;

        public bool IsFalconRunning
        {
            get
            {
                try
                {
                    ConnectToFalcon();
                    return _lpPrimarySharedMemoryAreaBaseAddress != IntPtr.Zero;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
                var fileSizeBytes = GetMaxMemFileSize(_lpOsbSharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof (OSBData))) fileSizeBytes = Marshal.SizeOf(typeof (OSBData));
                for (var i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpOsbSharedMemoryAreaBaseAddress, i));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            var toReturn = bytesRead.ToArray();
            return toReturn.Length == 0 ? null : toReturn;
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
                var fileSizeBytes = GetMaxMemFileSize(_lpSecondarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof (FlightData2)))
                    fileSizeBytes = Marshal.SizeOf(typeof (FlightData2));
                for (var i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpSecondarySharedMemoryAreaBaseAddress, i));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            var toReturn = bytesRead.ToArray();
            return toReturn.Length == 0 ? null : toReturn;
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
                var fileSizeBytes = GetMaxMemFileSize(_lpPrimarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof (BMS4FlightData)))
                    fileSizeBytes = Marshal.SizeOf(typeof (BMS4FlightData));
                for (var i = 0; i < fileSizeBytes; i++)
                {
                    try
                    {
                        bytesRead.Add(Marshal.ReadByte(_lpPrimarySharedMemoryAreaBaseAddress, i));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            var toReturn = bytesRead.ToArray();
            return toReturn.Length == 0 ? null : toReturn;
        }

        public FlightData GetCurrentData()
        {
            var dataType = typeof (BMS4FlightData);

            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return null;
            }
            var data = Convert.ChangeType(Marshal.PtrToStructure(_lpPrimarySharedMemoryAreaBaseAddress, dataType),
                dataType);
            Marshal.SizeOf(dataType);
            var toReturn = new FlightData((BMS4FlightData) data);

            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (Marshal.PtrToStructure(_lpSecondarySharedMemoryAreaBaseAddress, typeof (FlightData2)));
                toReturn.PopulateFromStruct(data);
            }
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (Marshal.PtrToStructure(_lpOsbSharedMemoryAreaBaseAddress, typeof (OSBData)));
                toReturn.PopulateFromStruct(data);
            }

            return toReturn;
        }

        private void ConnectToFalcon()
        {
            if (!_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero)) return;
            _hPrimarySharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ,
                false, PrimarySharedMemoryAreaFileName);
            _lpPrimarySharedMemoryAreaBaseAddress =
                NativeMethods.MapViewOfFile(_hPrimarySharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ,
                    0, 0, IntPtr.Zero);
            _hSecondarySharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(
                NativeMethods.SECTION_MAP_READ, false, SecondarySharedMemoryFileName);
            _lpSecondarySharedMemoryAreaBaseAddress =
                NativeMethods.MapViewOfFile(_hSecondarySharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ,
                    0, 0, IntPtr.Zero);
            _hOsbSharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false,
                OsbSharedMemoryAreaFileName);
            _lpOsbSharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hOsbSharedMemoryAreaFileMappingObject,
                NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);
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
            if (_disposed) return;
            if (disposing)
            {
                Disconnect();
            }

            _disposed = true;
        }

        ~Reader()
        {
            Dispose(false);
        }
    }
}