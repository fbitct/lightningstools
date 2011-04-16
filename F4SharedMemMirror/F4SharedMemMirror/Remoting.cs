using System;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace F4SharedMemMirror
{
    public interface ISharedMemoryMirrorClient
    {
        byte[] GetPrimaryFlightData();
        byte[] GetFlightData2();
        byte[] GetOSBData();
    }

    public interface ISharedMemoryMirrorServer
    {
        byte[] GetPrimaryFlightData();
        byte[] GetFlightData2();
        byte[] GetOSBData();
    }

    public class SharedMemoryMirrorClient : ISharedMemoryMirrorClient
    {
        private readonly ISharedMemoryMirrorServer _server;

        public SharedMemoryMirrorClient(IPEndPoint serverEndpoint, string serviceName)
        {
            IDictionary prop = new Hashtable();
            prop["port"] = serverEndpoint.Port;
            prop["machineName"] = serverEndpoint.Address.ToString();
            prop["priority"] = 100;
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
                _server = (SharedMemoryMirrorServer) Activator.GetObject(
                    typeof (SharedMemoryMirrorServer),
                    "tcp://"
                    + serverEndpoint.Address
                    + ":"
                    + serverEndpoint.Port.ToString(CultureInfo.InvariantCulture)
                    + "/"
                    + serviceName);
            }
            catch (Exception)
            {
            }
        }

        #region ISharedMemoryMirrorClient Members

        public byte[] GetPrimaryFlightData()
        {
            return _server.GetPrimaryFlightData();
        }

        public byte[] GetFlightData2()
        {
            return _server.GetFlightData2();
        }

        public byte[] GetOSBData()
        {
            return _server.GetOSBData();
        }

        #endregion
    }

    public class SharedMemoryMirrorServer : MarshalByRefObject, ISharedMemoryMirrorServer
    {
        private static byte[] _primaryFlightData;
        private static byte[] _flightData2;
        private static byte[] _osbData;

        private SharedMemoryMirrorServer()
        {
        }

        #region ISharedMemoryMirrorServer Members

        public byte[] GetPrimaryFlightData()
        {
            return _primaryFlightData;
        }

        public byte[] GetFlightData2()
        {
            return _flightData2;
        }

        public byte[] GetOSBData()
        {
            return _osbData;
        }

        #endregion

        internal static void CreateService(string serviceName, int port)
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
                // Register as an available service with the name HelloWorld     
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof (SharedMemoryMirrorServer), serviceName,
                    WellKnownObjectMode.Singleton);
            }
            catch (Exception)
            {
            }
        }

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

        public static void SetPrimaryFlightData(byte[] primaryFlightData)
        {
            _primaryFlightData = primaryFlightData;
        }

        public static void SetFlightData2(byte[] flightData2)
        {
            _flightData2 = flightData2;
        }

        public static void SetOSBData(byte[] osbData)
        {
            _osbData = osbData;
        }
    }
}