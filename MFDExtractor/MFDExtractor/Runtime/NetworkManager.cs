using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;
using F4SharedMem;
using System.Drawing;
using System.Net;
using Common.Networking;
using MFDExtractor.Runtime.Settings;
using MFDExtractor.Networking;

namespace MFDExtractor.Runtime
{
    internal partial class NetworkManager:IDisposable
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(NetworkManager));
        #endregion

        #region Instance variables
        private bool _disposed = false;
        /// <summary>
        /// Reference to a Client object that can read data from a networked Extractor engine running
        /// in Server mode
        /// </summary>
        private IExtractorClient _client = null;
        private IExtractorServer _server = null;
        private SettingsManager _settingsManager;
        #endregion

        private NetworkManager() { }

        public NetworkManager(SettingsManager settingsManager):this()
        {
            _settingsManager = settingsManager;
            SetupNetworking();
        }
        #region Networking Support
        #region Basic Network Client/Server Setup Code
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
        /// <summary>
        /// Establishes a .NET Remoting-based connection to a remote MFD Extractor server
        /// </summary>
        private void SetupClient()
        {
            try
            {
                _client = new Networking.ExtractorClient(_settingsManager.ServerEndpont, _settingsManager.ServiceName);
            }
            catch (Exception)
            {
                //Debug.WriteLine(e);
            }
        }
        /// <summary>
        /// Opens a .NET Remoting-based network server channel that remote clients can connect to
        /// </summary>
        private void SetupServer()
        {
            try
            {
                _server = new ExtractorServer(_settingsManager.ServiceName, _settingsManager.ServerEndpont.Port, _settingsManager.CompressionType, _settingsManager.ImageFormat);
            }
            catch (Exception)
            {
            }
        }
        #endregion
        #region MFD Network Image Transfer Code
        #region Outbound Transfer
        public void SendFlightDataToClients(FlightData flightData)
        {
            if (_settingsManager.NetworkMode == NetworkMode.Server && _server !=null)
            {
                _server.StoreFlightData(flightData);
            }
        }

        /// <summary>
        /// Makes an image of a specified instrument available to remote (networked) clients
        /// </summary>
        /// <param name="image">a Bitmap representing the specified instrument</param>
        public void SendInstrumentImageToClients(string instrumentName, Image image, NetworkMode networkMode)
        {
            if (networkMode == NetworkMode.Server && _server !=null)
            {
                _server.StoreInstrumentImage(instrumentName, image);
            }
        }
        #endregion
        #region Inbound Transfer
        public FlightData ReadFlightDataFromServer()
        {
            FlightData retrieved = null;
            try
            {
                retrieved = _client.GetFlightData();
            }
            catch (Exception e)
            {
                _log.Error(e.Message.ToString(), e);
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
                _log.Error(e.Message.ToString(), e);
            }
            return retrieved;
        }
        public void SubmitMessageToServerFromClient(Message message)
        {
            if (_client !=null) _client.SendMessageToServer(message);
        }
        public void SubmitMessageToClientFromServer(Message message)
        {
            if (_server !=null) _server.SubmitMessageToClientFromServer(message);
        }
        public Message GetNextPendingMessageToServerFromClient()
        {
            if (_server != null)
            {
                return _server.GetNextPendingMessageToServerFromClient();
            }
            else
            {
                return null;
            }
        }
        public Message GetNextPendingMessageToClientFromServer()
        {
            if (_server != null)
            {
                return _server.GetNextPendingMessageToClientFromServer();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #endregion
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
                    Common.Util.DisposeObject(_server);
                    _server = null;

                    Common.Util.DisposeObject(_client);
                    _client = null;
                }
            }
            _disposed = true;
        }
        #endregion

    }
}
