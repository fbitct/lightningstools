using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using Common.Imaging;
using F4SharedMem;

namespace MFDExtractor.Networking
{
    public class ExtractorServer : MarshalByRefObject, IExtractorServer
    {
        private static readonly object _instrumentImagesSpriteLock = new object();
        private static readonly object _flightDataLock = new object();
        private static FlightData _flightData;
        private static Image _instrumentImagesSprite;
        private static long _instrumentImagesSpriteSequenceNum;
        private static long _flightDataSequenceNum;
        private static long _lastRetrievedInstrumentImagesSpriteSequenceNum = 0;
        private static long _lastRetrievedFlightDataSequenceNum = 0;
        private static byte[] _lastRetrievedInstrumentImagesSpriteBytes;
        private static FlightData _lastRetrievedFlightData;
        private static string _compressionType = "LZW";
        private static string _imageFormat = "TIFF";
        private static readonly List<Message> _messagesToServerFromClient = new List<Message>();
        private static readonly List<Message> _messagesToClientFromServer = new List<Message>();
        private static bool _serviceEstablished;

        private ExtractorServer()
        {
        }

        public FlightData GetFlightData()
        {
            FlightData toReturn = null;
            if (!Extractor.State.SimRunning)
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
                Interlocked.Exchange(ref _lastRetrievedFlightData, toReturn);
            }
            return toReturn;
        }



        public byte[] GetInstrumentImagesSpriteBytes()
        {
            byte[] toReturn;
            if (!Extractor.State.SimRunning)
            {
                return null;
            }
            if (_lastRetrievedInstrumentImagesSpriteSequenceNum == _instrumentImagesSpriteSequenceNum)
            {
                return _lastRetrievedInstrumentImagesSpriteBytes;
            }
            if (_instrumentImagesSprite == null)
            {
                return null;
            }
            lock (_instrumentImagesSpriteLock)
            {
                //TODO: check image format when BMS is set to 16-bit color, see if it's 565 or 555
                Util.ConvertPixelFormat(ref _instrumentImagesSprite, PixelFormat.Format16bppRgb565);
                toReturn = Util.BytesFromBitmap(_instrumentImagesSprite, _compressionType, _imageFormat);
                Interlocked.Exchange(ref _lastRetrievedInstrumentImagesSpriteBytes, toReturn);
            }
            return toReturn;
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
                Interlocked.Exchange(ref _flightData, flightData);
            }
            Interlocked.Increment(ref _flightDataSequenceNum);
        }

        

        internal static void SetInstrumentImagesSprite(Image bitmap)
        {
            var cloned = Util.CloneBitmap(bitmap);
            lock (_instrumentImagesSpriteLock)
            {
                if (_instrumentImagesSprite != null)
                {
                    var oldRef = _instrumentImagesSprite;
                    Interlocked.Exchange(ref _instrumentImagesSprite, cloned);
                    Common.Util.DisposeObject(oldRef);
                }
                else
                {
                    Interlocked.Exchange(ref _instrumentImagesSprite, cloned);
                }
            }
            Interlocked.Increment(ref _instrumentImagesSpriteSequenceNum);
        }

        public static void ClearPendingMessagesToServerFromClientOfType(string messageType)
        {
            var messagesToRemove = _messagesToServerFromClient.Where(message => message.MessageType == messageType).ToList();
	        foreach (var message in messagesToRemove)
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

        public static void SubmitMessageToClientFromServer(Message message)
        {
            if (_messagesToClientFromServer != null)
            {
                if (_messagesToClientFromServer.Count >= 1000)
                {
                    _messagesToClientFromServer.RemoveRange(999, _messagesToClientFromServer.Count - 1000);
                        //limit the message queue size to 1000 messages
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
    }
}