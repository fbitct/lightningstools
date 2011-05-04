using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Common;
using F4SharedMem;

namespace MFDExtractor.Networking
{
    public class ExtractorServer : MarshalByRefObject, IExtractorServer, IDisposable
    {
        #region Class variables

        private static ExtractorServer _instance;

        #endregion

        #region Instance variables

        private readonly List<Message> _messagesToClientFromServer = new List<Message>();
        private readonly List<Message> _messagesToServerFromClient = new List<Message>();
        private readonly ImageObjectStore _objectStore;
        private readonly int _portNumber;
        private string _compressionType;
        private bool _disposed;
        private bool _serviceEstablished;
        private string _serviceName;

        #endregion

        #region Constructors

        private ExtractorServer()
        {
        }

        public ExtractorServer(string serviceName, int port, string compressionType, string imageFormat) : this()
        {
            if (_instance != null)
            {
                Util.DisposeObject(_instance);
            }
            _serviceName = serviceName;
            _portNumber = port;
            _compressionType = compressionType;
            _objectStore = new ImageObjectStore(compressionType, imageFormat);
            CreateService(serviceName, port);
            _instance = this;
        }

        #endregion

        #region IExtractorServer Members

        public bool TestConnection()
        {
            return _serviceEstablished;
        }

        public void StoreInstrumentImage(string instrumentName, Image image)
        {
            _objectStore.StoreImage(instrumentName, image);
        }

        public byte[] GetInstrumentImageBytes(string instrumentName)
        {
            return _objectStore.GetSerializedObject(instrumentName);
        }

        public void StoreFlightData(FlightData flightData)
        {
            _objectStore.StoreRawObject("FLIGHTDATA", flightData);
        }

        public FlightData GetFlightData()
        {
            return (FlightData) _objectStore.GetRawObject("FLIGHTDATA");
        }

        public void SubmitMessageToServerFromClient(Message message)
        {
            if (_messagesToServerFromClient == null) return;
            if (_messagesToServerFromClient.Count >= 1000)
            {
                _messagesToServerFromClient.RemoveRange(999, _messagesToServerFromClient.Count - 1000);
            }
            if (message.MessageType == "RequestNewMapImage")
            {
                //only allow one of these in the queue at a time
                ClearPendingMessagesToServerFromClientOfType(message.MessageType);
            }
            if (message != null)
            {
                _messagesToServerFromClient.Add(message);
            }
        }

        public void SubmitMessageToClientFromServer(Message message)
        {
            if (_messagesToClientFromServer == null) return;
            if (_messagesToClientFromServer.Count >= 1000)
            {
                _messagesToClientFromServer.RemoveRange(999, _messagesToClientFromServer.Count - 1000);
                //limit the message queue size to 1000 messages
            }
            _messagesToClientFromServer.Add(message);
        }


        public Message GetNextPendingMessageToServerFromClient()
        {
            Message toReturn = null;
            if (_messagesToServerFromClient != null)
            {
                if (_messagesToServerFromClient.Count > 0)
                {
                    toReturn = _messagesToServerFromClient[0];
                    _messagesToServerFromClient.RemoveAt(0);
                }
            }
            return toReturn;
        }

        public void ClearPendingMessagesToClientFromServer()
        {
            if (_messagesToClientFromServer != null)
            {
                _messagesToClientFromServer.Clear();
            }
        }

        public Message GetNextPendingMessageToClientFromServer()
        {
            Message toReturn = null;
            if (_messagesToClientFromServer != null)
            {
                if (_messagesToClientFromServer.Count > 0)
                {
                    toReturn = _messagesToClientFromServer[0];
                    _messagesToClientFromServer.RemoveAt(0);
                }
            }
            return toReturn;
        }

        #endregion

        #region Object Disposal & Destructors

        /// <summary>
        /// Public implementation of the IDisposable pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Private implementation of the IDisposable pattern
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    TearDownService(_portNumber);
                    Util.DisposeObject(_objectStore);
                    _messagesToClientFromServer.Clear();
                    _messagesToServerFromClient.Clear();
                }
            }
            _disposed = true;
        }

        #endregion

        private void CreateService(string serviceName, int port)
        {
            IDictionary prop = new Hashtable();
            prop["port"] = port;
            prop["priority"] = 100;
            try
            {
                RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
            }
            catch (Exception)
            {
            }
            TcpServerChannel channel = null;
            try
            {
                channel = new TcpServerChannel(prop, null, null);
            }
            catch (Exception)
            {
            }
            try
            {
                if (channel != null)
                {
                    ChannelServices.RegisterChannel(channel, false);
                }
            }
            catch (Exception)
            {
            }
            try
            {
                // Register as an available service      
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof (ExtractorServer), serviceName,
                    WellKnownObjectMode.Singleton);
            }
            catch (Exception)
            {
            }
            if (_messagesToServerFromClient != null)
            {
                _messagesToServerFromClient.Clear();
            }
            _serviceEstablished = true;
        }

        private static void TearDownService(int port)
        {
            IDictionary prop = new Hashtable();
            prop["port"] = port;
            TcpServerChannel channel = null;
            try
            {
                channel = new TcpServerChannel(prop, null, null);
            }
            catch (Exception)
            {
            }

            try
            {
                ChannelServices.UnregisterChannel(channel);
            }
            catch (Exception)
            {
            }
        }

        public void ClearPendingMessagesToServerFromClientOfType(string messageType)
        {
            var messagesToRemove = _messagesToServerFromClient.Where(message => message.MessageType == messageType).ToList();
            foreach (var message in messagesToRemove)
            {
                _messagesToServerFromClient.Remove(message);
            }
        }

        public void ClearPendingMessagesToServerFromClient()
        {
            if (_messagesToServerFromClient != null)
            {
                _messagesToServerFromClient.Clear();
            }
        }
    }
}