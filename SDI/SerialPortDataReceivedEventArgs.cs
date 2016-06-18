using System;

namespace SDI
{
    internal sealed class SerialPortDataReceivedEventArgs : EventArgs
    {
        public SerialPortDataReceivedEventArgs(){}
        public byte[] Data { get; set; }
    }
}