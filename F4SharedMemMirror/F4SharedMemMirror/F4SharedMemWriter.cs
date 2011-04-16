using System;
using System.Runtime.InteropServices;

namespace F4SharedMemMirror
{
    public sealed class Writer : IDisposable
    {
        private string _OsbSharedMemoryAreaFileName = "FalconSharedOsbMemoryArea";
        private bool _disposed;
        private IntPtr _hOsbSharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hPrimarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _hSecondarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpOsbSharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpPrimarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private IntPtr _lpSecondarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private string _primarySharedMemoryAreaFileName = "FalconSharedMemoryArea";
        private string _secondarySharedMemoryFileName = "FalconSharedMemoryArea2";

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void WritePrimaryFlightData(byte[] primaryFlightData)
        {
            if (primaryFlightData == null || primaryFlightData.Length == 0) return;
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                CreatePrimarySharedMemoryArea((ushort) primaryFlightData.Length);
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return;
            }
            for (int i = 0; i < primaryFlightData.Length; i++)
            {
                Marshal.WriteByte(_lpPrimarySharedMemoryAreaBaseAddress, i, primaryFlightData[i]);
            }
        }

        public void WriteOSBData(byte[] osbData)
        {
            if (osbData == null || osbData.Length == 0) return;
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                CreateOsbSharedMemoryArea((ushort) osbData.Length);
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return;
            }
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                for (int i = 0; i < osbData.Length; i++)
                {
                    Marshal.WriteByte(_lpOsbSharedMemoryAreaBaseAddress, i, osbData[i]);
                }
            }
        }

        public void WriteFlightData2(byte[] flightData2)
        {
            if (flightData2 == null || flightData2.Length == 0) return;
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                CreateSecondarySharedMemoryArea((ushort) flightData2.Length);
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                return;
            }
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                for (int i = 0; i < flightData2.Length; i++)
                {
                    Marshal.WriteByte(_lpSecondarySharedMemoryAreaBaseAddress, i, flightData2[i]);
                }
            }
        }

        private void CreateOsbSharedMemoryArea(ushort length)
        {
            _hOsbSharedMemoryAreaFileMappingObject =
                NativeMethods.CreateFileMapping(new IntPtr(NativeMethods.INVALID_HANDLE_VALUE), IntPtr.Zero,
                                                NativeMethods.PageProtection.ReadWrite, 0, length,
                                                _OsbSharedMemoryAreaFileName);
            _lpOsbSharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hOsbSharedMemoryAreaFileMappingObject,
                                                                            NativeMethods.SECTION_MAP_READ |
                                                                            NativeMethods.SECTION_MAP_WRITE, 0, 0,
                                                                            IntPtr.Zero);
        }

        private void CreateSecondarySharedMemoryArea(ushort length)
        {
            _hSecondarySharedMemoryAreaFileMappingObject =
                NativeMethods.CreateFileMapping(new IntPtr(NativeMethods.INVALID_HANDLE_VALUE), IntPtr.Zero,
                                                NativeMethods.PageProtection.ReadWrite, 0, length,
                                                _secondarySharedMemoryFileName);
            _lpSecondarySharedMemoryAreaBaseAddress =
                NativeMethods.MapViewOfFile(_hSecondarySharedMemoryAreaFileMappingObject,
                                            NativeMethods.SECTION_MAP_READ | NativeMethods.SECTION_MAP_WRITE, 0, 0,
                                            IntPtr.Zero);
        }

        private void CreatePrimarySharedMemoryArea(ushort length)
        {
            _hPrimarySharedMemoryAreaFileMappingObject =
                NativeMethods.CreateFileMapping(new IntPtr(NativeMethods.INVALID_HANDLE_VALUE), IntPtr.Zero,
                                                NativeMethods.PageProtection.ReadWrite, 0, length,
                                                _primarySharedMemoryAreaFileName);
            _lpPrimarySharedMemoryAreaBaseAddress =
                NativeMethods.MapViewOfFile(_hPrimarySharedMemoryAreaFileMappingObject,
                                            NativeMethods.SECTION_MAP_READ | NativeMethods.SECTION_MAP_WRITE, 0, 0,
                                            IntPtr.Zero);
        }

        private void CloseSharedMemFiles()
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
                    CloseSharedMemFiles();
                }

                _disposed = true;
            }
        }
    }
}