using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Net;
namespace F4SharedMemMirror
{
    public enum NetworkingMode 
    {
        Client,
        Server
    }
    public class Mirror:IDisposable
    {
        private F4SharedMem.Reader _smReader = null;
        private volatile bool _running = false;
        private volatile bool _disposed = false;
        private Writer _writer = new Writer();
        public Mirror():base()
        {
        }
        public NetworkingMode NetworkingMode
        {
            get;
            set;
        }
        public ushort PortNumber
        {
            get;
            set;
        }
        public System.Net.IPAddress ClientIPAddress
        {
            get;
            set;
        }
        public bool IsRunning
        {
            get
            {
                return _running;
            }
        }
        public void StartMirroring()
        {
            switch (this.NetworkingMode)
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
            Remoting.SharedMemoryMirrorClient client = null;
            try
            {
                string serverIPAddress = Properties.Settings.Default.ServerIPAddress;
                string serverPortNum = Properties.Settings.Default.ServerPortNum;
                int portNum = 21142;
                Int32.TryParse((string)Properties.Settings.Default.ServerPortNum, out portNum);
                IPAddress serverAddress = IPAddress.Parse(serverIPAddress);
                IPEndPoint endpoint = new IPEndPoint(serverAddress, portNum);
                client = new F4SharedMemMirror.Remoting.SharedMemoryMirrorClient(endpoint, "F4SharedMemoryMirrorService");
            }
            catch (System.Runtime.Remoting.RemotingException e)
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
                            string serverIPAddress = Properties.Settings.Default.ServerIPAddress;
                            string serverPortNum = Properties.Settings.Default.ServerPortNum;
                            int portNum = 21142;
                            Int32.TryParse((string)Properties.Settings.Default.ServerPortNum, out portNum);
                            IPAddress serverAddress = IPAddress.Parse(serverIPAddress);
                            IPEndPoint endpoint = new IPEndPoint(serverAddress, portNum);
                            client = new F4SharedMemMirror.Remoting.SharedMemoryMirrorClient(endpoint, "F4SharedMemoryMirrorService");
                        }
                        catch (System.Runtime.Remoting.RemotingException e)
                        {
                            client = null;
                        }
                    }
                    byte[] primaryFlightData = null;
                    byte[] flightData2= null;
                    byte[] osbData=null;
                    if (client != null)
                    {
                        try
                        {
                            primaryFlightData = client.GetPrimaryFlightData();
                            flightData2 = client.GetFlightData2();
                            osbData = client.GetOSBData();
                        }
                        catch (System.Runtime.Remoting.RemotingException e)
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
                System.Threading.Thread.Sleep(Properties.Settings.Default.PollingFrequencyMillis);
                Application.DoEvents();
            }
        }

        private void RunServer()
        {
            if (_running) throw new InvalidOperationException();
            _running = true;
            _smReader = new F4SharedMem.Reader();

            string serverPortNum = Properties.Settings.Default.ServerPortNum;
            int portNum = 21142;
            Int32.TryParse((string)Properties.Settings.Default.ServerPortNum, out portNum);

            try
            {
                F4SharedMemMirror.Remoting.SharedMemoryMirrorServer.TearDownService(portNum);
            }
            catch (System.Runtime.Remoting.RemotingException e)
            {
            }

            try
            {
                F4SharedMemMirror.Remoting.SharedMemoryMirrorServer.CreateService("F4SharedMemoryMirrorService", portNum);
            }
            catch (System.Runtime.Remoting.RemotingException e)
            {
            }

            while (_running)
            {
                try
                {
                    byte[] primaryFlightData = _smReader.GetRawPrimaryFlightData();
                    byte[] flightData2 = _smReader.GetRawFlightData2();
                    byte[] osbData = _smReader.GetRawOSBData();
                    Remoting.SharedMemoryMirrorServer.SetPrimaryFlightData(primaryFlightData);
                    Remoting.SharedMemoryMirrorServer.SetFlightData2(flightData2);
                    Remoting.SharedMemoryMirrorServer.SetOSBData(osbData);
                    Thread.Sleep(Properties.Settings.Default.PollingFrequencyMillis);
                    Application.DoEvents();
                }
                catch (System.Runtime.Remoting.RemotingException e)
                {
                    Debug.WriteLine(e);
                }
            } 

            try
            {
                F4SharedMemMirror.Remoting.SharedMemoryMirrorServer.TearDownService(21142);
            }
            catch (System.Runtime.Remoting.RemotingException e)
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
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
