using System;
using System.Runtime.InteropServices;
using F4SharedMem.Win32;
using System.Collections.Generic;
namespace F4SharedMem
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public sealed class Reader : IDisposable
    {
        private string _primarySharedMemoryAreaFileName = "FalconSharedMemoryArea";
        private IntPtr _hPrimarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpPrimarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private string _secondarySharedMemoryFileName = "FalconSharedMemoryArea2";
        private IntPtr _hSecondarySharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpSecondarySharedMemoryAreaBaseAddress = IntPtr.Zero;
        private string _OsbSharedMemoryAreaFileName = "FalconSharedOsbMemoryArea";
        private IntPtr _hOsbSharedMemoryAreaFileMappingObject = IntPtr.Zero;
        private IntPtr _lpOsbSharedMemoryAreaBaseAddress = IntPtr.Zero;
        private bool _disposed = false;
        
        public Reader()
        {
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
                    else
                    {
                       return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
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
            List<byte> bytesRead = new List<byte>();
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                long fileSizeBytes = GetMaxMemFileSize(_lpOsbSharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(Headers.OSBData))) fileSizeBytes = Marshal.SizeOf(typeof(Headers.OSBData));
                for (int i = 0; i < fileSizeBytes; i++)
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
            byte[] toReturn = bytesRead.ToArray();
            if (toReturn.Length == 0)
            {
                return null;
            }
            else
            {
                return toReturn;
            }
        }
        
        private long GetMaxMemFileSize(IntPtr pMemAreaBaseAddr)
        {
            NativeMethods.MEMORY_BASIC_INFORMATION mbi = new NativeMethods.MEMORY_BASIC_INFORMATION();
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
            List<byte> bytesRead = new List<byte>();
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                long fileSizeBytes = GetMaxMemFileSize(_lpSecondarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(Headers.FlightData2))) fileSizeBytes = Marshal.SizeOf(typeof(Headers.FlightData2));
                for (int i = 0; i < fileSizeBytes; i++)
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
            byte[] toReturn = bytesRead.ToArray();
            if (toReturn.Length == 0)
            {
                return null;
            }
            else
            {
                return toReturn;
            }
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
            List<byte> bytesRead = new List<byte>();
            if (!_hPrimarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                long fileSizeBytes = GetMaxMemFileSize(_lpPrimarySharedMemoryAreaBaseAddress);
                if (fileSizeBytes > Marshal.SizeOf(typeof(Headers.BMS4FlightData))) fileSizeBytes = Marshal.SizeOf(typeof(Headers.BMS4FlightData));
                for (int i = 0; i < fileSizeBytes; i++)
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
            byte[] toReturn = bytesRead.ToArray();
            if (toReturn.Length == 0)
            {
                return null;
            }
            else
            {
                return toReturn;
            }
        }
        
        public FlightData GetCurrentData()
        {
            Type dataType=typeof(Headers.BMS4FlightData);

            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals (IntPtr.Zero)) {
                ConnectToFalcon();
            }
            if (_hPrimarySharedMemoryAreaFileMappingObject.Equals (IntPtr.Zero)) {
                return null;
            }
            object data = Convert.ChangeType(Marshal.PtrToStructure(_lpPrimarySharedMemoryAreaBaseAddress, dataType), dataType);
            var len = Marshal.SizeOf(dataType);
            FlightData toReturn=new FlightData((Headers.BMS4FlightData)data);
                  
            if (!_hSecondarySharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (Marshal.PtrToStructure(_lpSecondarySharedMemoryAreaBaseAddress, typeof(Headers.FlightData2)));
                toReturn.PopulateFromStruct(data);
            }
            if (!_hOsbSharedMemoryAreaFileMappingObject.Equals(IntPtr.Zero))
            {
                data = (Marshal.PtrToStructure(_lpOsbSharedMemoryAreaBaseAddress, typeof(Headers.OSBData)));
                toReturn.PopulateFromStruct(data);
            }
           
            return toReturn;
        }
        
        private void ConnectToFalcon()
        {
            Disconnect();
            _hPrimarySharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false, _primarySharedMemoryAreaFileName);
            _lpPrimarySharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hPrimarySharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);
            _hSecondarySharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false, _secondarySharedMemoryFileName);
            _lpSecondarySharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hSecondarySharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);
            _hOsbSharedMemoryAreaFileMappingObject = NativeMethods.OpenFileMapping(NativeMethods.SECTION_MAP_READ, false, _OsbSharedMemoryAreaFileName);
            _lpOsbSharedMemoryAreaBaseAddress = NativeMethods.MapViewOfFile(_hOsbSharedMemoryAreaFileMappingObject, NativeMethods.SECTION_MAP_READ, 0, 0, IntPtr.Zero);
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
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
