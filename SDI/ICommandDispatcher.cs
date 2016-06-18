using System;

namespace SDI
{
    internal interface ICommandDispatcher:IDisposable
    {
        void SendCommand(CommandSubAddress subAddress, byte data);
        string SendQuery(CommandSubAddress subAddress, byte data);
    }
}
