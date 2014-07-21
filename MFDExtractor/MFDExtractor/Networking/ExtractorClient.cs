using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using Common.Imaging;
using F4SharedMem;

namespace MFDExtractor.Networking
{
    public class ExtractorClient : IExtractorClient
    {
        private readonly IPEndPoint _serverEndpoint;
        private readonly string _serviceName;
        private BackgroundWorker _connectionTestingBackgroundWorker;
        private DateTime _lastConnectionCheckTime = DateTime.Now.Subtract(new TimeSpan(0, 5, 0));
        private IExtractorServer _server;
        private bool _wasConnected;

        public ExtractorClient(IPEndPoint serverEndpoint, string serviceName)
        {
            _serverEndpoint = serverEndpoint;
            _serviceName = serviceName;
            EnsureConnected();
        }

        [DebuggerHidden]
        public bool IsConnected
        {
            get
            {
                var toReturn = false;
                var secondsSinceLastCheck = (int) DateTime.Now.Subtract(_lastConnectionCheckTime).TotalSeconds;
                if (secondsSinceLastCheck > 0 && secondsSinceLastCheck < 5)
                {
                    return _wasConnected;
                }
                if (_server != null)
                {
                    try
                    {
                        _lastConnectionCheckTime = DateTime.Now;
                        Application.DoEvents();
                        if (_connectionTestingBackgroundWorker == null)
                        {
                            _connectionTestingBackgroundWorker = new BackgroundWorker();
                            _connectionTestingBackgroundWorker.DoWork += _connectionTestingBackgroundWorker_DoWork;
                        }
                        if (_connectionTestingBackgroundWorker != null && !_connectionTestingBackgroundWorker.IsBusy)
                        {
                            _connectionTestingBackgroundWorker.RunWorkerAsync();
                        }
                        toReturn = _wasConnected;
                    }
                    catch (Exception)
                    {
                    }
                }
                return toReturn;
            }
        }

        public Image GetInstrumentImage(InstrumentType instrumentType)
        {
            if (_server != null)
            {
                var raw = _server.GetInstrumentImageBytes(instrumentType);
                return Util.BitmapFromBytes(raw);
            }
            return null;
        }

        public FlightData GetFlightData()
        {
            return _server != null ? _server.GetFlightData() : null;
        }

        public void SendMessageToServer(Message message)
        {
            EnsureConnected();
            if (IsConnected)
            {
                try
                {
                    if (_server != null)
                    {
                        _server.SubmitMessageToServerFromClient(message);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void ClearPendingMessagesToClientFromServer()
        {
            EnsureConnected();
            if (!IsConnected) return;
            try
            {
                if (_server != null)
                {
                    _server.ClearPendingMessagesToClientFromServer();
                }
            }
            catch
            {
            }
        }

        public Message GetNextMessageToClientFromServer()
        {
            EnsureConnected();
            Message toReturn = null;
            if (IsConnected)
            {
                try
                {
                    if (_server != null)
                    {
                        toReturn = _server.GetNextPendingMessageToClientFromServer();
                    }
                }
                catch
                {
                }
            }
            return toReturn;
        }

        [DebuggerHidden]
        private void EnsureConnected()
        {
            if (_serverEndpoint == null || _serviceName == null) return;
            if (!IsConnected)
            {
                IDictionary prop = new Hashtable();
                prop["port"] = _serverEndpoint.Port;
                prop["machineName"] = _serverEndpoint.Address.ToString();
                prop["priority"] = 100;
                prop["timeout"] = (uint) 1;
                prop["retryCount"] = 0;
                prop["useIpAddress"] = 1;
                TcpClientChannel chan = null;
                try
                {
                    chan = new TcpClientChannel();
                }
                catch
                {
                }
                try
                {
                    if (chan != null)
                    {
                        ChannelServices.RegisterChannel(chan, false);
                    }
                }
                catch (Exception)
                {
                }
                try
                {
                    RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
                }
                catch
                {
                }
                try
                {
                    // Create an instance of the remote object
                    _server = (ExtractorServer) Activator.GetObject(
                        typeof (ExtractorServer),
                        "tcp://"
                        + _serverEndpoint.Address
                        + ":"
                        + _serverEndpoint.Port.ToString(CultureInfo.InvariantCulture)
                        + "/"
                        + _serviceName);
                }
                catch
                {
                }
            }
        }

        [DebuggerHidden]
        private void _connectionTestingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_server != null)
            {
                try
                {
                    _wasConnected = _server.TestConnection();
                }
                catch
                {
                }
            }
        }
    }
}