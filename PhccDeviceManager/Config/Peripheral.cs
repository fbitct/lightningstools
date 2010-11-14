using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Phcc.DeviceManager.Config
{
    [Serializable]
    [XmlInclude(typeof(Doa40Do))]
    [XmlInclude(typeof(Doa7Seg))]
    [XmlInclude(typeof(Doa8Servo))]
    [XmlInclude(typeof(DoaAirCore))]
    [XmlInclude(typeof(DoaAnOut1))]
    [XmlInclude(typeof(DoaStepper))]
    public abstract class Peripheral : PhccConfigElement
    {
        public Peripheral():base() 
        {
        }
        public byte Address
        {
            get;
            set;
        }
    }
}
