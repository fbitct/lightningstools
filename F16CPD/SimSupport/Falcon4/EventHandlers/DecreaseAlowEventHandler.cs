using F16CPD.Networking;
using F16CPD.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface IDecreaseAlowEventHandler
    {
        void DecreaseAlow();
    }
    class DecreaseAlowEventHandler:IDecreaseAlowEventHandler
    {
        private F16CpdMfdManager _mfdManager;
        private IFalconCallbackSender _falconCallbackSender;
        public DecreaseAlowEventHandler(F16CpdMfdManager mfdManager, IFalconCallbackSender _falconCallbackSender = null)
        {
            _mfdManager = mfdManager;
            _falconCallbackSender = _falconCallbackSender ?? new FalconCallbackSender(_mfdManager);
        }
        public void DecreaseAlow()
        {
            if (Settings.Default.RunAsClient)
            {
                var message = new Message("Falcon4DecreaseALOW", _mfdManager.FlightData.AutomaticLowAltitudeWarningInFeet);
                _mfdManager.Client.SendMessageToServer(message);
            }
            else
            {
                if (_mfdManager.FlightData.AutomaticLowAltitudeWarningInFeet > 1000)
                {
                    _mfdManager.FlightData.AutomaticLowAltitudeWarningInFeet -= 1000;
                }
                else
                {
                    _mfdManager.FlightData.AutomaticLowAltitudeWarningInFeet -= 100;
                }
                _falconCallbackSender.SendCallbackToFalcon("DecreaseAlow");
            }
        }
    }
}
