using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Globalization;
using F4SharedMem;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace MFDExtractor.Networking
{
    [Serializable]
    public class Message
    {
        public Message()
            : base()
        {
        }
        public Message(string messageType, object payload)
            : this()
        {
            this.MessageType = messageType;
            this.Payload = payload;
        }
        public string MessageType
        {
            get;
            set;
        }
        public object Payload
        {
            get;
            set;
        }
    }
    public enum MessageTypes
    {
        AirspeedIndexIncrease,
        AirspeedIndexDecrease,
        ToggleNightMode,
        EHSILeftKnobIncrease,
        EHSILeftKnobDecrease,
        EHSIRightKnobIncrease,
        EHSIRightKnobDecrease,
        EHSIRightKnobDepressed,
        EHSIRightKnobReleased,
        EHSIMenuButtonDepressed,
        ISISBrightButtonDepressed,
        ISISStandardButtonDepressed,
        AzimuthIndicatorBrightnessIncrease,
        AzimuthIndicatorBrightnessDecrease,
        AccelerometerIsReset,
        EnableBMSAdvancedSharedmemValues,
        DisableBMSAdvancedSharedmemValues
    }
    public interface IExtractorClient
    {
        Image GetInstrumentImage(string instrumentName);
        FlightData GetFlightData();
        void SendMessageToServer(Message message);
        void ClearPendingMessagesToClientFromServer();
        Message GetNextMessageToClientFromServer();
        bool IsConnected
        {
            get;
        }
    }
    public class ExtractorClient : IExtractorClient
    {
        private IExtractorServer _server = null;
        private IPEndPoint _serverEndpoint = null;
        private string _serviceName = null;
        private DateTime _lastConnectionCheckTime = DateTime.Now.Subtract(new TimeSpan(0,5,0));
        private bool _wasConnected = false;
        private BackgroundWorker _connectionTestingBackgroundWorker = null;

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
                    catch (Exception)
                    {
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
                }
            }
        }
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


    }
    public interface IExtractorServer
    {
        byte[] GetInstrumentImageBytes(string instrumentName);
        FlightData GetFlightData();
        void SubmitMessageToServerFromClient(Message message);
        void ClearPendingMessagesToClientFromServer();
        Message GetNextPendingMessageToClientFromServer();
        bool TestConnection();
    }
    public class ExtractorServer : MarshalByRefObject, IExtractorServer
    {
        private ExtractorServer()
        {
        }
        private static Dictionary<string, object> _rawObjects = new Dictionary<string, object>(); //raw objects that can be transported across the network
        private static Dictionary<string, object> _objectLocks = new Dictionary<string, object>(); //lock per object prevents modifying object while being serialized
        private static Dictionary<string, int> _preSerializationHashcodes = new Dictionary<string, int>(); //stores pre-serialization hashcodes for serialized objects so that we can tell if a new version exists
        private static Dictionary<string, byte[]> _serializedObjects; //caches serialized objects so we don't have to perform serialization over and over again
        private static string _compressionType = "LZW";
        private static string _imageFormat = "TIFF";
        private static List<Message> _messagesToServerFromClient = new List<Message>();
        private static List<Message> _messagesToClientFromServer = new List<Message>();
        private static bool _serviceEstablished = false;

        [DebuggerHidden]
        internal static void CreateService(string serviceName, int port, string compressionType, string imageFormat)
        {
            _compressionType = compressionType;
            _imageFormat = imageFormat;

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
                // Register as an available service with the name HelloWorld     
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
        [DebuggerHidden]
        internal static void TearDownService(int port)
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
        internal static object GetLockForObject(string objectName)
        {
            object lockObject = _objectLocks.ContainsKey(objectName) ? _objectLocks[objectName] : null;
            if (lockObject == null)
            {

                lockObject = new object();
                _objectLocks.Add(objectName, lockObject);
            }
            return lockObject;
            
        }
        internal static void StoreRawObject(string objectName, object obj)
        {
            object objectLock = GetLockForObject(objectName);
            lock (objectLock)
            {
                if (_rawObjects.ContainsKey(objectName))
                {
                    _rawObjects[objectName] = objectName;
                }
                else
                {
                    _rawObjects.Add(objectName, obj);
                }

            }
        }
        internal static void StoreInstrumentImage(string instrumentName, Image image)
        {
            object imageLock = GetLockForObject(instrumentName);
            lock (imageLock)
            {
                Image existingImage = GetRawObject(instrumentName) as Image;
                StoreRawObject(instrumentName, image);
                Common.Util.DisposeObject(existingImage);
            }
        }
        internal static void StoreFlightData(FlightData flightData)
        {
            StoreRawObject("FLIGHTDATA", flightData);
        }
        internal static object GetRawObject(string objectName)
        {
            if (_rawObjects.ContainsKey(objectName))
            {
                return _rawObjects[objectName];
            }
            else
            {
                return null;
            }
        }
        internal static byte[] GetSerializedObject(string objectName)
        {
            if (!Extractor._simRunning)
            {
                return null;
            }

            object lockObject = GetLockForObject(objectName);
            lock (lockObject)
            {
                //retrieve the current unserialized (raw) object
                object rawObject = GetRawObject(objectName);
                if (rawObject == null) return null; //if it's NULL, then we don't have a value for this object now

                //see if we've ever serialized this object before, and if so, retrieve the latest cached value of the serialized object
                byte[] serializedObject = _serializedObjects.ContainsKey(objectName) ? _serializedObjects[objectName] : null;
                int preSerializationHashcode = _preSerializationHashcodes.ContainsKey(objectName) ? _preSerializationHashcodes[objectName] : 0;

                if (preSerializationHashcode == rawObject.GetHashCode())
                {
                    return serializedObject; //if the latest serialization was of an object having the same hashcode as the current raw object instance, then we don't bother serializing it again
                }

                //here, we either don't have a cached serialized version or the hashcode doesn't match 
                //so we need to re-serialize and  cache the serialized version
                serializedObject = SerializeObject(rawObject); 
                if (_serializedObjects.ContainsKey(objectName))
                {
                    _serializedObjects[objectName] = serializedObject; 
                }
                else
                {
                    _serializedObjects.Add(objectName, serializedObject);
                }

                //store the pre-serialized object's hashcode for later comparisons.  
                if (_preSerializationHashcodes.ContainsKey(objectName))
                {
                    _preSerializationHashcodes[objectName] = serializedObject.GetHashCode();
                }
                else
                {
                    _preSerializationHashcodes.Add(objectName, serializedObject.GetHashCode());
                }

                return serializedObject;
            }
        }
        internal static byte[] SerializeObject(object toSerialize)
        {
            if (toSerialize == null)
            {
                return null;
            }
            else if (toSerialize is Image)
            {
                Image asImage = toSerialize as Image;
                Common.Imaging.Util.ConvertPixelFormat(ref asImage, PixelFormat.Format16bppRgb565);
                return Common.Imaging.Util.BytesFromBitmap(asImage as Image, _compressionType, _imageFormat);
            }
            else
            {
                byte[] bytes = null;
                using (MemoryStream ms = new MemoryStream(1024))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, toSerialize);
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    bytes = new byte[ms.Length];
                    ms.Read(bytes, 0, (int)bytes.Length);
                    ms.Close();
                }
                return bytes;
            }
        }

        public byte[] GetInstrumentImageBytes(string instrumentName)
        {
            return GetSerializedObject(instrumentName);
        }
        public FlightData GetFlightData()
        {
            return (FlightData)GetRawObject("FLIGHTDATA");
        }
        public static void ClearPendingMessagesToServerFromClientOfType(string messageType)
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
        public static void ClearPendingMessagesToServerFromClient()
        {
            if (_messagesToServerFromClient != null)
            {
                _messagesToServerFromClient.Clear();
            }
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
        public static void SubmitMessageToClientFromServer(Message message)
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
        public static Message GetNextPendingMessageToServerFromClient()
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
        public bool TestConnection()
        {
            return _serviceEstablished;
        }
    }
}
