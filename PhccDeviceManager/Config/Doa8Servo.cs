using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Phcc.DeviceManager.Config
{
    [Serializable]
    public class Doa8Servo:Peripheral
    {
        public Doa8Servo()
            : base()
        {
        }
        [XmlArray ("Calibrations")]
        [XmlArrayItem("Calibration")]
        public List<ServoCalibration> ServoCalibrations
        {
            get;
            set;
        }
    }
}
