using F16CPD.Networking;
using F16CPD.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.Networking
{
    internal interface IClientSideInboundMessageProcessor
    {
        bool ProcessPendingMessage(Message message);
    }
    class ClientSideInboundMessageProcessor:IClientSideInboundMessageProcessor
    {
        public bool ProcessPendingMessage(Message message)
        {
            if (!Settings.Default.RunAsClient) return false;
            var toReturn = false;
            if (message != null)
            {
                var messageType = message.MessageType;
                if (messageType != null)
                {
                    switch (messageType)
                    {
                        case "Falcon4CallbackOccurredMessage":
                            var callback = (string)message.Payload;
                            ProcessDetectedCallback(callback);
                            toReturn = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            return toReturn;
        }
        private static void ProcessDetectedCallback(string callback)
        {
            if (String.IsNullOrEmpty(callback) || String.IsNullOrEmpty(callback.Trim())) return;
            /*
            if (callback.ToLowerInvariant().Trim() == "SimHSIModeInc".ToLowerInvariant() )
            {
                switch (_currentNavMode)
                {
                    case NavModes.Tcn:
                        _currentNavMode = NavModes.Nav;
                        break;
                    case NavModes.PlsTcn:
                        _currentNavMode = NavModes.Tcn;
                        break;
                    case NavModes.Nav:
                        _currentNavMode = NavModes.PlsNav;
                        break;
                    case NavModes.PlsNav:
                        _currentNavMode = NavModes.PlsNav;
                        break;
                    default:
                        break;
                }
            }
            else if (callback.ToLowerInvariant().Trim() == "SimStepHSIMode".ToLowerInvariant())
            {
                switch (_currentNavMode)
                {
                    case NavModes.Tcn:
                        _currentNavMode = NavModes.Nav;
                        break;
                    case NavModes.PlsTcn:
                        _currentNavMode = NavModes.Tcn;
                        break;
                    case NavModes.Nav:
                        _currentNavMode = NavModes.PlsNav;
                        break;
                    case NavModes.PlsNav:
                        _currentNavMode = NavModes.PlsTcn;
                        break;
                    default:
                        break;
                }
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSIModeDec".ToLowerInvariant())
            {
                switch (_currentNavMode)
                {
                    case NavModes.Tcn:
                        _currentNavMode = NavModes.PlsTcn;
                        break;
                    case NavModes.PlsTcn:
                        _currentNavMode = NavModes.PlsTcn;
                        break;
                    case NavModes.Nav:
                        _currentNavMode = NavModes.Tcn;
                        break;
                    case NavModes.PlsNav:
                        _currentNavMode = NavModes.Nav;
                        break;
                    default:
                        break;
                }
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSIIlsNav".ToLowerInvariant())
            {
                _currentNavMode = NavModes.PlsNav;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSIIlsTcn".ToLowerInvariant())
            {
                _currentNavMode = NavModes.PlsTcn;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSITcn".ToLowerInvariant())
            {
                _currentNavMode = NavModes.Tcn;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimHSINav".ToLowerInvariant())
            {
                _currentNavMode = NavModes.Nav;
            }
            */
            /*
            if (callback.ToLowerInvariant().Trim() == "SimAuxComBackup".ToLowerInvariant())
            {
                _tacanChannelSource = TacanChannelSource.Backup;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimAuxComUFC".ToLowerInvariant())
            {
                _tacanChannelSource = TacanChannelSource.Ufc;
            }
            else if (callback.ToLowerInvariant().Trim() == "SimToggleAuxComMaster".ToLowerInvariant())
            {
                if (_tacanChannelSource == TacanChannelSource.Backup)
                {
                    _tacanChannelSource = TacanChannelSource.Ufc;
                }
                else
                {
                    _tacanChannelSource = TacanChannelSource.Backup;
                }
            }
            else if (callback.ToLowerInvariant().Trim() == "SimCycleBandAuxComDigit".ToLowerInvariant())
            {
                if (_backupTacanBand == TacanBand.X)
                {
                    _backupTacanBand = TacanBand.Y;
                }
                else
                {
                    _backupTacanBand = TacanBand.X;
                }
            }
            */
            if (Settings.Default.RunAsServer)
            {
                var message = new Message("Falcon4CallbackOccurredMessage", callback.Trim());
                F16CPDServer.SubmitMessageToClient(message);
            }
        }
    }
}
