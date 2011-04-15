using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using F4SharedMem;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace MFDExtractor.Networking
{
    public class ExtractorServer : MarshalByRefObject, IExtractorServer, IDisposable
    {
        #region Class variables
        private static ExtractorServer _instance = null;
        #endregion

        #region Instance variables
        private List<Message> _messagesToServerFromClient = new List<Message>();
        private List<Message> _messagesToClientFromServer = new List<Message>();
        private string _serviceName;
        private int _portNumber;
        private bool _serviceEstablished = false;
        private string _compressionType = null;
        private ImageObjectStore _objectStore = null;
        private bool _disposed = false;
        #endregion


        #region Constructors
        private ExtractorServer():base()
        {
        }
        public ExtractorServer(string serviceName, int port, string compressionType, string imageFormat):this()
        {
            if (_instance != null)
            {
                Common.Util.DisposeObject(_instance);
            }
            _serviceName = serviceName;
            _portNumber = port;
            _compressionType = compressionType;
            _objectStore = new ImageObjectStore(compressionType, imageFormat);
            CreateService(serviceName, port);
            _instance = this;
        }
        #endregion

        public bool TestConnection()
        {
            return _serviceEstablished;
        }

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
                    typeof(Networking.ExtractorServer), serviceName,
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
        private void TearDownService(int port)
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
            return (FlightData)_objectStore.GetRawObject("FLIGHTDATA");
        }

        public void SubmitMessageToServerFromClient(Message message)
        {
            if (_messagesToServerFromClient != null)
            {
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
        }
        public void SubmitMessageToClientFromServer(Message message)
        {
            if (_messagesToClientFromServer != null)
            {
                if (_messagesToClientFromServer.Count >= 1000)
                {
                    _messagesToClientFromServer.RemoveRange(999, _messagesToClientFromServer.Count - 1000); //limit the message queue size to 1000 messages
                }
                _messagesToClientFromServer.Add(message);
            }
        }

        public void ClearPendingMessagesToServerFromClientOfType(string messageType)
        {
            List<Message> messagesToRemove = new List<Message>();
            foreach (Message message in _messagesToServerFromClient)
            {
                if (message.MessageType == messageType)
                {
                    messagesToRemove.Add(message);
                }
            }
            foreach (Message message in messagesToRemove)
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
                    Common.Util.DisposeObject(_objectStore);
                    _messagesToClientFromServer.Clear();
                    _messagesToServerFromClient.Clear();
                }
            }
            _disposed = true;
        }


        #endregion
    }

}
