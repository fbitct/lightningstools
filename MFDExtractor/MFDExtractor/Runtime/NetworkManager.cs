using System;
using System.Drawing;
using Common;
using Common.Networking;
using F4SharedMem;
using log4net;
using MFDExtractor.Networking;
using MFDExtractor.Runtime.Settings;

namespace MFDExtractor.Runtime
{
    internal class NetworkManager : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (NetworkManager));
        private readonly SettingsManager _settingsManager;
        private IExtractorClient _client;
        private bool _disposed;
        private IExtractorServer _server;
        private NetworkManager()
        {
        }

        public NetworkManager(SettingsManager settingsManager) : this()
        {
            _settingsManager = settingsManager;
            SetupNetworking();
        }

        public void SetupNetworking()
        {
            if (_settingsManager.NetworkMode == NetworkMode.Client)
            {
                SetupClient();
            }
            if (_settingsManager.NetworkMode == NetworkMode.Server)
            {
                SetupServer();
            }
        }
        private void SetupClient()
        {
            try
            {
                _client = new ExtractorClient(_settingsManager.ServerEndpont, _settingsManager.ServiceName);
            }
            catch (Exception)
            {
                //Debug.WriteLine(e);
            }
        }
        private void SetupServer()
        {
            try
            {
                _server = new ExtractorServer(_settingsManager.ServiceName, _settingsManager.ServerEndpont.Port,
                                              _settingsManager.CompressionType, _settingsManager.ImageFormat);
            }
            catch (Exception)
            {
            }
        }
        public void SendFlightDataToClients(FlightData flightData)
        {
            if (_settingsManager.NetworkMode == NetworkMode.Server && _server != null)
            {
                _server.StoreFlightData(flightData);
            }
        }
        public void SendInstrumentImageToClients(string instrumentName, Image image, NetworkMode networkMode)
        {
            if (networkMode == NetworkMode.Server && _server != null)
            {
                _server.StoreInstrumentImage(instrumentName, image);
            }
        }
        public FlightData ReadFlightDataFromServer()
        {
            FlightData retrieved = null;
            try
            {
                retrieved = _client.GetFlightData();
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
            return retrieved;
        }
        public Image ReadInstrumentImageFromServer(string instrumentName)
        {
            Image retrieved = null;
            try
            {
                retrieved = _client.GetInstrumentImage(instrumentName);
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
            return retrieved;
        }
        public void SubmitMessageToServerFromClient(Message message)
        {
            if (_client != null) _client.SendMessageToServer(message);
        }
        public void SubmitMessageToClientFromServer(Message message)
        {
            if (_server != null) _server.SubmitMessageToClientFromServer(message);
        }
        public Message GetNextPendingMessageToServerFromClient()
        {
            return _server != null ? _server.GetNextPendingMessageToServerFromClient() : null;
        }
        public Message GetNextPendingMessageToClientFromServer()
        {
            return _server != null ? _server.GetNextPendingMessageToClientFromServer() : null;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Util.DisposeObject(_server);
                    _server = null;

                    Util.DisposeObject(_client);
                    _client = null;
                }
            }
            _disposed = true;
        }
    }
}