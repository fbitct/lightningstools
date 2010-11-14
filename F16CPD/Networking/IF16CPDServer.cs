using System;
using System.Collections.Generic;

using System.Text;

namespace F16CPD.Networking
{
    public interface IF16CPDServer
    {
        object GetSimProperty(string propertyName);
        void SubmitMessageToServer(Message message);
        void ClearPendingClientMessages();
        Message GetNextPendingClientMessage();
        bool TestConnection();
    }
}
