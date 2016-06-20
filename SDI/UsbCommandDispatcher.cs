using System;
using System.Text;

namespace SDI
{
    internal class UsbCommandDispatcher:ICommandDispatcher
    {
        private bool _isDisposed = false;
        private ISerialPortConnection SerialPortConnection { get; set; }

        public UsbCommandDispatcher(ISerialPortConnection serialPortConnection)
        {
            SerialPortConnection = serialPortConnection;
        }
        public UsbCommandDispatcher(string portName, bool openPort=true)
        {
            SerialPortConnection = new SerialPortConnection(portName, openPort);
        }
        public void SendCommand(CommandSubAddress subAddress, byte data)
        {
            if (SerialPortConnection != null)
            {
                SerialPortConnection.Write(new[] { (byte)subAddress, data }, 0, 2);
            }
        }
        public string SendQuery(CommandSubAddress subAddress, byte data)
        {
            switch (subAddress)
            {
                case CommandSubAddress.IDENTIFY:
                    const int IDENTIFY_STRING_LENGTH = 14;
                    SerialPortConnection.DiscardInputBuffer();
                    SendCommand(CommandSubAddress.IDENTIFY, 0x00);
                    var readBuffer = new byte[IDENTIFY_STRING_LENGTH];
                    SerialPortConnection.Read(readBuffer, 0, IDENTIFY_STRING_LENGTH);
                    return Encoding.ASCII.GetString(readBuffer, 0, IDENTIFY_STRING_LENGTH);
                default:
                    SendCommand(subAddress, data);
                    return null;
            }
            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UsbCommandDispatcher()
        {
            Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed )
            {
                SerialPortConnection.Dispose();
            }
            _isDisposed = true;
        }

    }
}
