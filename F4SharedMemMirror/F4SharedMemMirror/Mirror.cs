using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;
using F4SharedMem;
using F4SharedMemMirror.Properties;
using F4SharedMemMirror.Remoting;

namespace F4SharedMemMirror
{
    public enum NetworkingMode
    {
        Client,
        Server
    }

    public class Mirror : IDisposable
    {
        private readonly Writer _writer = new Writer();
        private volatile bool _disposed;
        private volatile bool _running;
        private Reader _smReader;

        public NetworkingMode NetworkingMode { get; set; }
        public ushort PortNumber { get; set; }
        public IPAddress ClientIPAddress { get; set; }

        public bool IsRunning
        {
            get { return _running; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void StartMirroring()
        {
            switch (NetworkingMode)
            {
                case NetworkingMode.Client:
                    RunClient();
                    break;
                case NetworkingMode.Server:
                    RunServer();
                    break;
                default:
                    break;
            }
        }

        public void StopMirroring()
        {
            _running = false;
        }

        private void RunClient()
        {
            if (_running) throw new InvalidOperationException();
            _running = true;
            SharedMemoryMirrorClient client = null;
            try
            {
                string serverIPAddress = Settings.Default.ServerIPAddress;
                string serverPortNum = Settings.Default.ServerPortNum;
                int portNum = 21142;
                Int32.TryParse(Settings.Default.ServerPortNum, out portNum);
                IPAddress serverAddress = IPAddress.Parse(serverIPAddress);
                var endpoint = new IPEndPoint(serverAddress, portNum);
                client = new SharedMemoryMirrorClient(endpoint, "F4SharedMemoryMirrorService");
            }
            catch (RemotingException e)
            {
                client = null;
            }

            while (_running)
            {
                try
                {
                    if (client == null)
                    {
                        try
                        {
                            string serverIPAddress = Settings.Default.ServerIPAddress;
                            string serverPortNum = Settings.Default.ServerPortNum;
                            int portNum = 21142;
                            Int32.TryParse(Settings.Default.ServerPortNum, out portNum);
                            IPAddress serverAddress = IPAddress.Parse(serverIPAddress);
                            var endpoint = new IPEndPoint(serverAddress, portNum);
                            client = new SharedMemoryMirrorClient(endpoint, "F4SharedMemoryMirrorService");
                        }
                        catch (RemotingException e)
                        {
                            client = null;
                        }
                    }
                    byte[] primaryFlightData = null;
                    byte[] flightData2 = null;
                    byte[] osbData = null;
                    if (client != null)
                    {
                        try
                        {
                            primaryFlightData = client.GetPrimaryFlightData();
                            flightData2 = client.GetFlightData2();
                            osbData = client.GetOSBData();
                        }
                        catch (RemotingException e)
                        {
                            Debug.WriteLine(e);
                        }

                        _writer.WritePrimaryFlightData(primaryFlightData);
                        _writer.WriteFlightData2(flightData2);
                        _writer.WriteOSBData(osbData);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                Thread.Sleep(Settings.Default.PollingFrequencyMillis);
                Application.DoEvents();
            }
        }

        private void RunServer()
        {
            if (_running) throw new InvalidOperationException();
            _running = true;
            _smReader = new Reader();

            string serverPortNum = Settings.Default.ServerPortNum;
            int portNum = 21142;
            Int32.TryParse(Settings.Default.ServerPortNum, out portNum);

            try
            {
                SharedMemoryMirrorServer.TearDownService(portNum);
            }
            catch (RemotingException e)
            {
            }

            try
            {
                SharedMemoryMirrorServer.CreateService("F4SharedMemoryMirrorService", portNum);
            }
            catch (RemotingException e)
            {
            }

            while (_running)
            {
                try
                {
                    byte[] primaryFlightData = _smReader.GetRawPrimaryFlightData();
                    byte[] flightData2 = _smReader.GetRawFlightData2();
                    byte[] osbData = _smReader.GetRawOSBData();
                    SharedMemoryMirrorServer.SetPrimaryFlightData(primaryFlightData);
                    SharedMemoryMirrorServer.SetFlightData2(flightData2);
                    SharedMemoryMirrorServer.SetOSBData(osbData);
                    Thread.Sleep(Settings.Default.PollingFrequencyMillis);
                    Application.DoEvents();
                }
                catch (RemotingException e)
                {
                    Debug.WriteLine(e);
                }
            }

            try
            {
                SharedMemoryMirrorServer.TearDownService(21142);
            }
            catch (RemotingException e)
            {
            }
        }

        internal void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_smReader != null)
                    {
                        try
                        {
                            _smReader.Dispose();
                        }
                        catch (Exception e)
                        {
                        }
                    }
                    if (_writer != null)
                    {
                        try
                        {
                            _smReader.Dispose();
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }

                _disposed = true;
            }
        }
    }
}