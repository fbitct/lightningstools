using System;

namespace SDI
{
    internal class PhccCommandDispatcher:ICommandDispatcher
    {
        private bool _isDisposed = false;
        private Phcc.Device _phccDevice;
        private byte _sdiAddressOnDOA;
        public PhccCommandDispatcher(string portName, byte sdiAddressOnDOA, bool openPort=true)
        {
            _phccDevice = new Phcc.Device(portName, openPort);
            _sdiAddressOnDOA = sdiAddressOnDOA;
        }
        
        public void SendCommand(CommandSubAddress subAddress, byte data)
        {
            if (_phccDevice != null)
            {
                _phccDevice.DoaSendRaw(_sdiAddressOnDOA, (byte)subAddress, data);
            }
        }
        public string SendQuery(CommandSubAddress subAddress, byte data)
        {
            SendCommand(subAddress, data);
            return null;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
       
        ~PhccCommandDispatcher()
        {
            Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _phccDevice.Dispose();
            }
            _isDisposed = true;
        }

    }
}
