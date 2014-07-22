using F16CPD.Networking;
using F16CPD.Properties;
using F4SharedMem;
using System;

namespace F16CPD.SimSupport.Falcon4
{
    internal interface IFalconDataFormatDetector
    {
        FalconDataFormats? DetectFalconDataFormat();
    }
    class FalconDataFormatDetector:IFalconDataFormatDetector
    {
        private F16CpdMfdManager _mfdManager;
        public FalconDataFormatDetector(F16CpdMfdManager mfdManager)
        {
            _mfdManager = mfdManager;
        }
        public FalconDataFormats? DetectFalconDataFormat()
        {
            FalconDataFormats? toReturn = null;
            //if we're running as the server or we're running in standalone mode
            if (Settings.Default.RunAsServer || (!Settings.Default.RunAsServer && !Settings.Default.RunAsClient))
            {
                toReturn = F4Utils.Process.Util.DetectFalconFormat();
                if (Settings.Default.RunAsServer)
                {
                    F16CPDServer.SetSimProperty("SimName", "Falcon4");
                    F16CPDServer.SetSimProperty("SimVersion",
                                                toReturn.HasValue
                                                    ? Enum.GetName(typeof(FalconDataFormats), toReturn)
                                                    : null);
                }
            }
            else if (Settings.Default.RunAsClient)
            {
                var simName = (string)_mfdManager.Client.GetSimProperty("SimName");
                var simVersion = (string)_mfdManager.Client.GetSimProperty("SimVersion");
                if (simName != null && simName.ToLowerInvariant() == "falcon4")
                {
                    toReturn = (FalconDataFormats)Enum.Parse(typeof(FalconDataFormats), simVersion);
                }
            }

            return toReturn;
        }
    }
}
