using System;

namespace SDI
{
    internal interface ISerialPortConnection:IDisposable
    {
        string COMPort { get; set; }
        int BytesAvailable { get; }
        event EventHandler<SerialPortDataReceivedEventArgs> DataReceived;
        void Read(byte[] buffer, int index, int count, int timeout = -1);
        void Write(byte[] buffer, int index, int count);
        void DiscardInputBuffer();
    }
}