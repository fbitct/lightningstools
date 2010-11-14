using System;
using System.Collections.Generic;
using System.Text;

namespace Phcc.DeviceManager.Config
{
    [Serializable]
    public class DoaAnOut1:Peripheral
    {
        public DoaAnOut1()
            : base()
        {
        }
        public byte GainAllChannels
        {
            get;
            set;
        }
    }
}
