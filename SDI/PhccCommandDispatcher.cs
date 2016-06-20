﻿using System;

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
        

        public string SendCommand(CommandSubaddress subaddress, byte data)
        {
            SendCommandInternal(subaddress, data);
            return null;
        }
        private void SendCommandInternal(CommandSubaddress subaddress, byte data)
        {
            if (_phccDevice != null)
            {
                _phccDevice.DoaSendRaw(_sdiAddressOnDOA, (byte)subaddress, data);
            }
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