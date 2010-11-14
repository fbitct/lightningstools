using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Phcc.DeviceManager.Config
{
    [Serializable]
    [XmlInclude (typeof(Motherboard))]
    [XmlInclude(typeof(Peripheral))]
    public abstract class PhccConfigElement
    {
        public PhccConfigElement()
            : base()
        {
        }
    }
}
