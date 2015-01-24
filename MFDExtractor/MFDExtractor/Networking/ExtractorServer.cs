using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
    public class ExtractorServer : MarshalByRefObject, IExtractorServer
    {
        private static readonly object FlightDataLock = new object();
        private static FlightData _flightData;
        private static readonly ConcurrentDictionary<InstrumentType, Image> LatestTexSharedmemImages = new ConcurrentDictionary<InstrumentType, Image>(); 
        private static long _flightDataSequenceNum;
        private static long _lastRetrievedFlightDataSequenceNum = 0;
        private static FlightData _lastRetrievedFlightData;
        private static string _compressionType = "LZW";
        private static string _imageFormat = "TIFF";
        private static readonly List<Message> MessagesToServerFromClient = new List<Message>();
        private static readonly List<Message> MessagesToClientFromServer = new List<Message>();
        private static bool _serviceEstablished;

        private ExtractorServer()
        {
        }

        public FlightData GetFlightData()
        {
            FlightData toReturn;
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
            lock (FlightDataLock)
            {
                toReturn = _flightData;
                Interlocked.Exchange(ref _lastRetrievedFlightData, toReturn);
            }
            return toReturn;
        }



        public byte[] GetInstrumentImageBytes(InstrumentType instrumentType)
        {
            if (!Extractor.State.SimRunning)
            {
                return null;
            }
            Image image;
            LatestTexSharedmemImages.TryGetValue(instrumentType, out image);
            Util.ConvertPixelFormat(ref image, PixelFormat.Format16bppRgb565);
            LatestTexSharedmemImages.AddOrUpdate(instrumentType, x => image, (x, y) => image);
            var toReturn = Util.BytesFromBitmap(image, _compressionType, _imageFormat);
            return toReturn;
        }

        public void SubmitMessageToServerFromClient(Message message)
        {
            if (MessagesToServerFromClient == null) return;
            if (MessagesToServerFromClient.Count >= 1000)
            {
                MessagesToServerFromClient.RemoveRange(999, MessagesToServerFromClient.Count - 1000);
            }
            if (message.MessageType == "RequestNewMapImage")
            {
                //only allow one of these in the queue at a time
                ClearPendingMessagesToServerFromClientOfType(message.MessageType);
            }
            MessagesToServerFromClient.Add(message);
        }

        public void ClearPendingMessagesToClientFromServer()
        {
            if (MessagesToClientFromServer != null)
            {
                MessagesToClientFromServer.Clear();
            }
        }

        public Message GetNextPendingMessageToClientFromServer()
        {
            Message toReturn = null;
            if (MessagesToClientFromServer != null)
            {
                if (MessagesToClientFromServer.Count > 0)
                {
                    toReturn = MessagesToClientFromServer[0];
                    MessagesToClientFromServer.RemoveAt(0);
                }
            }
            return toReturn;
        }

        public bool TestConnection()
        {
            return _serviceEstablished;
        }

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
            catch {}
            TcpServerChannel channel = null;
            try
            {
                channel = new TcpServerChannel(prop, null, null);
            }
            catch {}
            try
            {
                if (channel != null)
                {
                    ChannelServices.RegisterChannel(channel, false);
                }
            }
            catch {}
            try
            {
                // Register as an available service with the name HelloWorld     
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof (ExtractorServer), serviceName,
                    WellKnownObjectMode.Singleton);
            }
            catch {}
            if (MessagesToServerFromClient != null)
            {
                MessagesToServerFromClient.Clear();
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
            catch {}

            try
            {
                ChannelServices.UnregisterChannel(channel);
            }
            catch {}
        }

        internal static void SetFlightData(FlightData flightData)
        {
            lock (FlightDataLock)
            {
                Interlocked.Exchange(ref _flightData, flightData);
            }
            Interlocked.Increment(ref _flightDataSequenceNum);
        }

        

        internal static void SetInstrumentImage(Image bitmap, InstrumentType instrumentType)
        {
            var cloned = Util.CloneBitmap(bitmap);
            LatestTexSharedmemImages.AddOrUpdate(instrumentType, x => cloned, (x, y) => cloned);
        }

        public static void ClearPendingMessagesToServerFromClientOfType(string messageType)
        {
            var messagesToRemove = MessagesToServerFromClient.Where(message => message.MessageType == messageType).ToList();
	        foreach (var message in messagesToRemove)
            {
                MessagesToServerFromClient.Remove(message);
            }
        }

        public static void ClearPendingMessagesToServerFromClient()
        {
            if (MessagesToServerFromClient != null)
            {
                MessagesToServerFromClient.Clear();
            }
        }

        public static void SubmitMessageToClientFromServer(Message message)
        {
            if (MessagesToClientFromServer != null)
            {
                if (MessagesToClientFromServer.Count >= 1000)
                {
                    MessagesToClientFromServer.RemoveRange(999, MessagesToClientFromServer.Count - 1000);
                        //limit the message queue size to 1000 messages
                }
                MessagesToClientFromServer.Add(message);
            }
        }

        public static Message GetNextPendingMessageToServerFromClient()
        {
            if (MessagesToServerFromClient == null || MessagesToServerFromClient.Count <= 0) return null;
            var toReturn = MessagesToServerFromClient[0];
            MessagesToServerFromClient.RemoveAt(0);
            return toReturn;
        }
    }
}