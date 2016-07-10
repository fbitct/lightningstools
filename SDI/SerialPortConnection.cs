using System;
using System.IO.Ports;
using System.Threading;

namespace SDI
{
    internal class SerialPortConnection:ISerialPortConnection
    {
        private readonly object _serialPortLock = new object();
        private bool _isDisposed;
        private string _COMPort;
        private SerialPort _serialPort;

        private SerialPortConnection(){}
        public SerialPortConnection(string COMPort) :this()
        {
            _COMPort = COMPort;
        }

        public event EventHandler<SerialPortDataReceivedEventArgs> DataReceived;

        public string COMPort
        {
            get { return _COMPort; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException(
                        "must contain a string that identifies a valid serial port on the machine (i.e. COM1, COM2, etc.)",
                        "value");
                }
                ClosePort();
                _COMPort = value;
            }
        }

        public void DiscardInputBuffer()
        {
            EnsurePortIsReady();
            lock (_serialPortLock)
            {
                _serialPort.DiscardInBuffer();
            }
        }

        public void Write(byte[] buffer, int index, int count)
        {
            EnsurePortIsReady();
            lock (_serialPortLock)
            {
                _serialPort.Write(buffer, index, count);
            }
        }
        public int BytesAvailable
        {
            get
            {
                lock (_serialPortLock)
                {
                    return (_serialPort != null && _serialPort.IsOpen) ? _serialPort.BytesToRead : 0;
                }
            }
        }

        public void Read(byte[] buffer, int index, int count)
        {
            lock (_serialPortLock)
            {
                EnsurePortIsReady();
                var timeout = _serialPort.ReadTimeout;
                var startTime = DateTime.UtcNow;
                var bytesAvailable = 0;
                while (bytesAvailable < count)
                {
                    bytesAvailable = BytesAvailable;
                    if (DateTime.UtcNow > startTime.AddMilliseconds(timeout) && timeout != Timeout.Infinite)
                    {
                        throw new TimeoutException();
                    }
                }
                _serialPort.Read(buffer, index, count);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void InitializeSerialPort()
        {
            lock (_serialPortLock)
            {
                ClosePort();
                _serialPort = new SerialPort();
                _serialPort.PortName = _COMPort;
                _serialPort.BaudRate = 115200;
                _serialPort.DataBits = 8;
                _serialPort.Parity = Parity.None;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                _serialPort.ReceivedBytesThreshold = 1;
                _serialPort.RtsEnable = true;
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;
                _serialPort.DataReceived += _serialPort_DataReceived;
                _serialPort.ErrorReceived += _serialPort_ErrorReceived;
                _serialPort.Open();
                GC.SuppressFinalize(_serialPort.BaseStream);
            }
        }

        private void EnsurePortIsReady()
        {
            lock (_serialPortLock)
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    InitializeSerialPort();
                }
            }
        }
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_serialPortLock)
            {
                try
                {
                    OnSerialPortDataReceived(sender, e);
                }
                catch { }
            }
        }
        private void OnSerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(this, new SerialPortDataReceivedEventArgs());
            }
        }
        private void ClosePort()
        {
            lock (_serialPortLock)
            {
                if (_serialPort != null)
                {
                    try {
                        _serialPort.DataReceived -= _serialPort_DataReceived;
                        _serialPort.ErrorReceived -= _serialPort_ErrorReceived;
                    }
                    catch { }
                    try
                    {
                        if (_serialPort.IsOpen)
                        {
                            try
                            {
                                GC.ReRegisterForFinalize(_serialPort.BaseStream);
                            }
                            catch { }
                            _serialPort.Close();
                        }
                    }
                    catch { }
                    finally
                    {
                        _serialPort.Dispose();
                    }
                    _serialPort = null;
                    Thread.Sleep(500);
                }
            }
        }
       private void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) { }

        ~SerialPortConnection()
        {
            Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                ClosePort(); 
            }
            _isDisposed = true;
        }

    }
}
