using System;
using System.Collections.Generic;

using System.Text;

namespace F16CPD.Networking
{
    public interface IF16CPDClient
    {
        object GetSimProperty(string propertyName);
        void SendMessageToServer(Message message);
        void ClearPendingClientMessages();
        Message GetNextPendingClientMessage();
        bool IsConnected
        {
            get;
        }
    }
}
