using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
namespace Phcc.DeviceManager.Config
{
    [XmlRoot("PhccDeviceManagerConfiguration")]
    public class ConfigurationManager
    {
        private List<Motherboard> _motherboards = new List<Motherboard>();
        public ConfigurationManager()
            : base()
        {
            
        }
        [XmlArray("Devices")]
        [XmlArrayItem("Motherboard")]
        public List<Motherboard> Motherboards
        {
            get
            {
                return _motherboards;
            }
            set
            {
                _motherboards = value;
            }
        }
        public static ConfigurationManager Load(string fileName)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<ConfigurationManager>(fileName);
        }
        public void Save(string fileName)
        {
            Common.Serialization.Util.SerializeToXmlFile<ConfigurationManager>(this, fileName);
        }
    }
}
