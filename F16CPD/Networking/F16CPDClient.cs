using System;
using System.Collections.Generic;

using System.Text;
using System.Net;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using log4net;

namespace F16CPD.Networking
{
    public class F16CPDClient : IF16CPDClient
    {
        private static ILog _log = LogManager.GetLogger(typeof(F16CPDClient));
        private IF16CPDServer _server = null;
        private IPEndPoint _serverEndpoint = null;
        private string _serviceName = null;
        private DateTime _lastConnectionCheckTime = DateTime.MinValue;
        private bool _wasConnected = false;
        private BackgroundWorker _connectionTestingBackgroundWorker = null;
        public F16CPDClient(IPEndPoint serverEndpoint, string serviceName)
        {
            _serverEndpoint = serverEndpoint;
            _serviceName = serviceName;
            EnsureConnected();
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
                prop["timeout"] = (uint)1;
                prop["retryCount"] = 0;
                prop["useIpAddress"] = 1;
                TcpClientChannel chan = null;
                try
                {
                    chan = new TcpClientChannel();
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
                try
                {
                    if (chan != null)
                    {
                        ChannelServices.RegisterChannel(chan, false);
                    }
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
                try
                {
                    RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
                try
                {
                    // Create an instance of the remote object
                    _server = (F16CPDServer)Activator.GetObject(
                        typeof(F16CPDServer),
                        "tcp://"
                            + _serverEndpoint.Address.ToString()
                            + ":"
                            + _serverEndpoint.Port.ToString(CultureInfo.InvariantCulture)
                            + "/"
                            + _serviceName);
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
            }
        }
        [DebuggerHidden]
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
                    catch (Exception e)
                    {
                        _log.Debug(e.Message, e);
                    }
                }
                return toReturn;
            }
        }

        [DebuggerHidden]
        void _connectionTestingBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_server != null)
            {
                try
                {
                    _wasConnected = _server.TestConnection();
                }
                catch (Exception ex)
                {
                    _log.Debug(ex.Message, ex);
                }
            }
        }
        

        public object GetSimProperty(string propertyName)
        {
            EnsureConnected();
            object toReturn = null;
            if (IsConnected)
            {
                try
                {
                    toReturn = _server.GetSimProperty(propertyName);
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
            }
            return toReturn;
        }

        public void SendMessageToServer(Message message)
        {
            EnsureConnected();
            if (IsConnected)
            {
                try
                {
                    _server.SubmitMessageToServer(message);
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
            }
        }
        public void ClearPendingClientMessages()
        {
            EnsureConnected();
            if (IsConnected)
            {
                try
                {
                    _server.ClearPendingClientMessages();
                }
                catch (Exception e)
                {
                    _log.Debug(e.Message, e);
                }
            }
        }
        public Message GetNextPendingClientMessage()
        {
            EnsureConnected();
            Message toReturn = null;
            if (IsConnected)
            {
                try
                {
                    toReturn = _server.GetNextPendingClientMessage();
                }
                catch (Exception e )
                {
                    _log.Debug(e.Message, e);
                }
            }
            return toReturn;
        }
    }
}
