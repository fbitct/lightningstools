using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFDExtractor.Networking
{
    [Serializable]
    public class Message
    {
        public Message()
            : base()
        {
        }
        public Message(string messageType, object payload)
            : this()
        {
            this.MessageType = messageType;
            this.Payload = payload;
        }
        public string MessageType
        {
            get;
            set;
        }
        public object Payload
        {
            get;
            set;
        }
    }
}
