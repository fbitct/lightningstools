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
        Bitmap GetMfd4Bitmap();
        Bitmap GetMfd3Bitmap();
        Bitmap GetLeftMfdBitmap();
        Bitmap GetRightMfdBitmap();
        Bitmap GetHudBitmap();
        FlightData GetFlightData();
        void SendMessageToServer(Message message);
        void ClearPendingMessagesToClientFromServer();
        Message GetNextMessageToClientFromServer();
        bool IsConnected
        {
            get;
        }
    }
    public interface IExtractorServer
    {
        byte[] GetMfd4BitmapBytes();
        byte[] GetMfd3BitmapBytes();
        byte[] GetLeftMfdBitmapBytes();
        byte[] GetRightMfdBitmapBytes();
        byte[] GetHudBitmapBytes();
        FlightData GetFlightData();
        void SubmitMessageToServerFromClient(Message message);
        void ClearPendingMessagesToClientFromServer();
        Message GetNextPendingMessageToClientFromServer();
        bool TestConnection();
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
        public Bitmap GetMfd4Bitmap()
        {
            if (_server != null)
            {
                byte[] raw = _server.GetMfd4BitmapBytes();
                return Common.Imaging.Util.BitmapFromBytes(raw);
            }
            else
            {
                return null;
            }
        }
        public Bitmap GetMfd3Bitmap()
        {
            if (_server != null)
            {
                byte[] raw = _server.GetMfd3BitmapBytes();
                return Common.Imaging.Util.BitmapFromBytes(raw);
            }
            else
            {
                return null;
            }
        }
        public Bitmap GetLeftMfdBitmap()
        {
            if (_server != null)
            {
                byte[] raw = _server.GetLeftMfdBitmapBytes();
                return Common.Imaging.Util.BitmapFromBytes(raw);
            }
            else
            {
                return null;
            }
        }
        public Bitmap GetRightMfdBitmap()
        {
            if (_server != null)
            {
                byte[] raw = _server.GetRightMfdBitmapBytes();
                return Common.Imaging.Util.BitmapFromBytes(raw);
            }
            else
            {
                return null;
            }
        }
        public Bitmap GetHudBitmap()
        {
            if (_server != null)
            {
                byte[] raw = _server.GetHudBitmapBytes();
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
    public class ExtractorServer : MarshalByRefObject, IExtractorServer
    {
        private ExtractorServer()
        {
        }
        private static object _mfd4BitmapLock = new object();
        private static object _mfd3BitmapLock = new object();
        private static object _leftMfdBitmapLock = new object();
        private static object _rightMfdBitmapLock = new object();
        private static object _hudBitmapLock = new object();
        private static object _flightDataLock = new object();
        private static FlightData _flightData = null;
        private static Bitmap _mfd4Bitmap = null;
        private static Bitmap _mfd3Bitmap = null;
        private static Bitmap _leftMfdBitmap = null;
        private static Bitmap _rightMfdBitmap = null;
        private static Bitmap _hudBitmap = null;
        private static long _mfd4ImageSequenceNum = 0;
        private static long _mfd3ImageSequenceNum = 0;
        private static long _leftMfdImageSequenceNum = 0;
        private static long _rightMfdImageSequenceNum = 0;
        private static long _hudImageSequenceNum = 0;
        private static long _flightDataSequenceNum = 0;
        private static long _lastRetrievedMfd4ImageSequenceNum = 0;
        private static long _lastRetrievedMfd3ImageSequenceNum = 0;
        private static long _lastRetrievedLeftMfdImageSequenceNum = 0;
        private static long _lastRetrievedRightMfdImageSequenceNum = 0;
        private static long _lastRetrievedHudImageSequenceNum = 0;
        private static long _lastRetrievedFlightDataSequenceNum = 0;
        private static byte[] _lastRetrievedMfd4ImageBytes = null;
        private static byte[] _lastRetrievedMfd3ImageBytes = null;
        private static byte[] _lastRetrievedLeftMfdImageBytes = null;
        private static byte[] _lastRetrievedRightMfdImageBytes = null;
        private static byte[] _lastRetrievedHudImageBytes = null;
        private static FlightData _lastRetrievedFlightData = null;
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
        internal static void SetFlightData(FlightData flightData)
        {
            lock (_flightDataLock)
            {
                System.Threading.Interlocked.Exchange(ref _flightData, flightData);
            }
            System.Threading.Interlocked.Increment(ref _flightDataSequenceNum);
        }
        internal static void SetMfd4Bitmap(Bitmap bitmap)
        {
            Bitmap cloned = Common.Imaging.Util.CloneBitmap(bitmap);
            lock (_mfd4BitmapLock)
            {
                if (_mfd4Bitmap != null)
                {
                    Bitmap oldRef = _mfd4Bitmap;
                    System.Threading.Interlocked.Exchange(ref _mfd4Bitmap, cloned);
                    Common.Util.DisposeObject(oldRef);
                }
                else
                {
                    System.Threading.Interlocked.Exchange(ref _mfd4Bitmap, cloned);
                }
            }
            System.Threading.Interlocked.Increment(ref _mfd4ImageSequenceNum);
        }
        internal static void SetMfd3Bitmap(Bitmap bitmap)
        {
            Bitmap cloned = Common.Imaging.Util.CloneBitmap(bitmap);
            lock (_mfd3BitmapLock)
            {
                if (_mfd3Bitmap != null)
                {
                    Bitmap oldRef = _mfd3Bitmap;
                    System.Threading.Interlocked.Exchange(ref _mfd3Bitmap, cloned);
                    Common.Util.DisposeObject(oldRef);
                }
                else
                {
                    System.Threading.Interlocked.Exchange(ref _mfd3Bitmap, cloned);
                }
            }
            System.Threading.Interlocked.Increment(ref _mfd3ImageSequenceNum);
        }
        internal static void SetLeftMfdBitmap(Bitmap bitmap)
        {
            Bitmap cloned = Common.Imaging.Util.CloneBitmap(bitmap);
            lock (_leftMfdBitmapLock)
            {
                if (_leftMfdBitmap != null)
                {
                    Bitmap oldRef = _leftMfdBitmap;
                    System.Threading.Interlocked.Exchange(ref _leftMfdBitmap, cloned);
                    Common.Util.DisposeObject(oldRef);
                }
                else
                {
                    System.Threading.Interlocked.Exchange(ref _leftMfdBitmap, cloned);
                }
            }
            System.Threading.Interlocked.Increment(ref _leftMfdImageSequenceNum);
        }
        internal static void SetRightMfdBitmap(Bitmap bitmap)
        {
            Bitmap cloned = Common.Imaging.Util.CloneBitmap(bitmap);
            lock (_rightMfdBitmapLock)
            {
                if (_rightMfdBitmap != null)
                {
                    Bitmap oldRef = _rightMfdBitmap;
                    System.Threading.Interlocked.Exchange(ref _rightMfdBitmap, cloned);
                    Common.Util.DisposeObject(oldRef);
                }
                else
                {
                    System.Threading.Interlocked.Exchange(ref _rightMfdBitmap, cloned);
                }
            }
            System.Threading.Interlocked.Increment(ref _rightMfdImageSequenceNum);
        }
        internal static void SetHudBitmap(Bitmap bitmap)
        {
            Bitmap cloned = Common.Imaging.Util.CloneBitmap(bitmap);
            lock (_hudBitmapLock)
            {
                if (_hudBitmap != null)
                {
                    Bitmap oldRef = _hudBitmap;
                    System.Threading.Interlocked.Exchange(ref _hudBitmap, cloned);
                    Common.Util.DisposeObject(oldRef);
                }
                else
                {
                    System.Threading.Interlocked.Exchange(ref _hudBitmap, cloned);
                }
            }
            System.Threading.Interlocked.Increment(ref _hudImageSequenceNum);
        }
        public FlightData GetFlightData()
        {
            FlightData toReturn = null;
            if (!Extractor._simRunning)
            {
                return null;
            }
            if (_lastRetrievedFlightDataSequenceNum == _flightDataSequenceNum)
            {
                return _lastRetrievedFlightData;
            }
            if (_flightData == null)
            {
                return null;
            }
            lock (_flightDataLock)
            {
                toReturn = _flightData;
                System.Threading.Interlocked.Exchange(ref _lastRetrievedFlightData, toReturn);
            }
            return toReturn;
        }

        public byte[] GetMfd4BitmapBytes()
        {
            byte[] toReturn = null;
            if (!Extractor._simRunning)
            {
                return null;
            }
            if (_lastRetrievedMfd4ImageSequenceNum == _mfd4ImageSequenceNum)
            {
                return _lastRetrievedMfd4ImageBytes;
            }
            if (_mfd4Bitmap == null)
            {
                return null;
            }
            lock (_mfd4BitmapLock)
            {
                Common.Imaging.Util.ConvertPixelFormat(ref _mfd4Bitmap, PixelFormat.Format16bppRgb565);
                toReturn = Common.Imaging.Util.BytesFromBitmap(_mfd4Bitmap, _compressionType, _imageFormat);
                System.Threading.Interlocked.Exchange(ref _lastRetrievedMfd4ImageBytes, toReturn);
            }
            return toReturn;
        }
        public byte[] GetMfd3BitmapBytes()
        {
            byte[] toReturn = null;
            if (!Extractor._simRunning)
            {
                return null;
            }
            if (_lastRetrievedMfd3ImageSequenceNum == _mfd3ImageSequenceNum)
            {
                return _lastRetrievedMfd3ImageBytes;
            }
            if (_mfd3Bitmap == null)
            {
                return null;
            }
            lock (_mfd3BitmapLock)
            {
                Common.Imaging.Util.ConvertPixelFormat(ref _mfd3Bitmap, PixelFormat.Format16bppRgb565);
                toReturn = Common.Imaging.Util.BytesFromBitmap(_mfd3Bitmap, _compressionType, _imageFormat);
                System.Threading.Interlocked.Exchange(ref _lastRetrievedMfd3ImageBytes, toReturn);
            }
            return toReturn;
        }
        public byte[] GetLeftMfdBitmapBytes()
        {
            byte[] toReturn = null;
            if (!Extractor._simRunning)
            {
                return null;
            }
            if (_lastRetrievedLeftMfdImageSequenceNum == _leftMfdImageSequenceNum)
            {
                return _lastRetrievedLeftMfdImageBytes;
            }
            if (_leftMfdBitmap == null)
            {
                return null;
            }
            lock (_leftMfdBitmapLock)
            {
                Common.Imaging.Util.ConvertPixelFormat(ref _leftMfdBitmap, PixelFormat.Format16bppRgb565);
                toReturn = Common.Imaging.Util.BytesFromBitmap(_leftMfdBitmap, _compressionType, _imageFormat);
                System.Threading.Interlocked.Exchange(ref _lastRetrievedLeftMfdImageBytes, toReturn);
            }
            return toReturn;
        }
        public byte[] GetRightMfdBitmapBytes()
        {
            byte[] toReturn = null;

            if (!Extractor._simRunning)
            {
                return null;
            }
            if (_lastRetrievedRightMfdImageSequenceNum == _rightMfdImageSequenceNum)
            {
                return _lastRetrievedRightMfdImageBytes;
            }
            if (_rightMfdBitmap == null)
            {
                return null;
            }
            lock (_rightMfdBitmapLock)
            {
                Common.Imaging.Util.ConvertPixelFormat(ref _rightMfdBitmap, PixelFormat.Format16bppRgb565);
                toReturn = Common.Imaging.Util.BytesFromBitmap(_rightMfdBitmap, _compressionType, _imageFormat);
                System.Threading.Interlocked.Exchange(ref _lastRetrievedRightMfdImageBytes, toReturn);
            }
            return toReturn;
        }
        public byte[] GetHudBitmapBytes()
        {
            byte[] toReturn = null;
            if (!Extractor._simRunning)
            {
                return null;
            }
            if (_lastRetrievedHudImageSequenceNum == _hudImageSequenceNum)
            {
                return _lastRetrievedHudImageBytes;
            }
            if (_hudBitmap == null)
            {
                return null;
            }
            lock (_hudBitmapLock)
            {
                //TODO: check image format when OpenFalcon is set to 16-bit color, see if it's 565 or 555
                Common.Imaging.Util.ConvertPixelFormat(ref _hudBitmap, PixelFormat.Format16bppRgb565);
                toReturn = Common.Imaging.Util.BytesFromBitmap(_hudBitmap, _compressionType, _imageFormat);
                System.Threading.Interlocked.Exchange(ref _lastRetrievedHudImageBytes, toReturn);
            }
            return toReturn;
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
