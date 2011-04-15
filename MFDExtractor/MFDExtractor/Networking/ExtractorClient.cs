using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net;
using System.Drawing;
using F4SharedMem;

namespace MFDExtractor.Networking
{
    public class ExtractorClient : IExtractorClient
    {
        private IExtractorServer _server = null;
        private IPEndPoint _serverEndpoint = null;
        private string _serviceName = null;
        private DateTime _lastConnectionCheckTime = DateTime.Now.Subtract(new TimeSpan(0, 5, 0));
        private bool _wasConnected = false;
        private BackgroundWorker _connectionTestingBackgroundWorker = null;

        public ExtractorClient(IPEndPoint serverEndpoint, string serviceName)
        {
            _serverEndpoint = serverEndpoint;
            _serviceName = serviceName;
            EnsureConnected();
        }
        public Image GetInstrumentImage(string instrumentName)
        {
            if (_server != null)
            {
                byte[] raw = _server.GetInstrumentImageBytes(instrumentName);
                return Common.Imaging.Util.BitmapFromBytes(raw);
            }
            else
            {
                return null;
            }
        }
        public FlightData GetFlightData()
        {
            if (_server != null)
            {
                return _server.GetFlightData();
            }
            else
            {
                return null;
            }
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
            if (IsConnected)
            {
                try
                {
                    if (_server != null)
                    {
                        _server.ClearPendingMessagesToClientFromServer();
                    }
                }
                catch (Exception)
                {
                }
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
                catch (Exception)
                {
                }
            }
            return toReturn;
        }

        public bool IsConnected
        {
            get
            {
                bool toReturn = false;
                int secondsSinceLastCheck = (int)DateTime.Now.Subtract(_lastConnectionCheckTime).TotalSeconds;
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
                            _connectionTestingBackgroundWorker.DoWork += new DoWorkEventHandler(_connectionTestingBackgroundWorker_DoWork);
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
        private void EnsureConnected()
        {
            if (_serverEndpoint == null || _serviceName == null) return;
            if (!IsConnected)
            {
                IDictionary prop = new Hashtable();
                prop["port"] = _serverEndpoint.Port;
                prop["machineName"] = _serverEndpoint.Address.ToString();
                prop["priority"] = 100;
                prop["timeout"] = (uint)1;
                prop["retryCount"] = 0;
                prop["useIpAddress"] = 1;
                TcpClientChannel chan = null;
                try
                {
                    chan = new TcpClientChannel();
                }
                catch (Exception)
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
                catch (Exception)
                {
                }
                try
                {
                    // Create an instance of the remote object
                    _server = (Networking.ExtractorServer)Activator.GetObject(
                        typeof(Networking.ExtractorServer),
                        "tcp://"
                            + _serverEndpoint.Address.ToString()
                            + ":"
                            + _serverEndpoint.Port.ToString(CultureInfo.InvariantCulture)
                            + "/"
                            + _serviceName);
                }
                catch (Exception)
                {
                }
            }
        }
        private void _connectionTestingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_server != null)
            {
                try
                {
                    _wasConnected = _server.TestConnection();
                }
                catch {}
            }
        }

    }
}
