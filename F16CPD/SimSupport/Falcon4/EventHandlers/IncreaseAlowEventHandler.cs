using F16CPD.Networking;
using F16CPD.Properties;
using F16CPD.SimSupport.Falcon4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4.EventHandlers
{
    internal interface IIncreaseAlowEventHandler
    {
        void IncreaseAlow();
    }
    class IncreaseAlowEventHandler : IIncreaseAlowEventHandler
    {
        private F16CpdMfdManager _mfdManager;
        private IFalconCallbackSender _falconCallbackSender;
        public IncreaseAlowEventHandler(F16CpdMfdManager mfdManager, IFalconCallbackSender falconCallbackSender = null)
        {
            _mfdManager = mfdManager;
            _falconCallbackSender = falconCallbackSender ?? new FalconCallbackSender(_mfdManager);
        }
        public void IncreaseAlow()
        {

            if (Settings.Default.RunAsClient)
            {
                var message = new Message("Falcon4IncreaseALOW", _mfdManager.FlightData.AutomaticLowAltitudeWarningInFeet);
                _mfdManager.Client.SendMessageToServer(message);
            }
            else
            {
                if (_mfdManager.FlightData.AutomaticLowAltitudeWarningInFeet < 1000)
                {
                    _mfdManager.FlightData.AutomaticLowAltitudeWarningInFeet += 100;
                }
                else
                {
                    _mfdManager.FlightData.AutomaticLowAltitudeWarningInFeet += 1000;
                }
                _falconCallbackSender.SendCallbackToFalcon("IncreaseAlow");
            }
        }
    }
}
