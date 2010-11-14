using System;
using System.Collections.Generic;

using System.Text;

namespace F16CPD.Networking
{
    [Serializable]
    public class Message 
    {
        public Message():base()
        {
        }
        public Message(string messageType, object payload):this()
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
    }}
