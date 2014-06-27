using F16CPD.Networking;
using F16CPD.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace F16CPD.SimSupport.Falcon4
{
    internal interface IFalconCallbackSender
    {
        void SendCallbackToFalcon(string callback);
        void SendCallbackToFalconLocal(string callback);
    }
    class FalconCallbackSender:IFalconCallbackSender
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(FalconCallbackSender));
        private F16CpdMfdManager _mfdManager;
        public FalconCallbackSender(F16CpdMfdManager mfdManager)
        {
            _mfdManager = mfdManager;
        }
        public void SendCallbackToFalcon(string callback)
        {
            if (!Settings.Default.RunAsClient)
            {
                SendCallbackToFalconLocal(callback);
            }
            else if (Settings.Default.RunAsClient)
            {
                var message = new Message("Falcon4SendCallbackMessage", callback);
                _mfdManager.Client.SendMessageToServer(message);
            }
        }

        private static object SyncLock = new Object();
        public void SendCallbackToFalconLocal(string callback)
        {
            lock (SyncLock)
            {
                IsSendingInput = true;
                F4Utils.Process.KeyFileUtils.SendCallbackToFalcon(callback);
                IsSendingInput = false;
            }
        }
        public static bool IsSendingInput { get; private set; }

    }
}
