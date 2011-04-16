using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using log4net;

namespace Common.SimSupport
{
    [Serializable]
    public class SimSupportModuleRegistry
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (SimSupportModuleRegistry));

        [XmlArray(ElementName = "SimSupportModules")]
        [XmlArrayItem("Module")]
        public string[] SimSupportModuleTypeNames { get; set; }

        public List<SimSupportModule> GetInstances()
        {
            List<SimSupportModule> toReturn = null;
            foreach (string ssmTypeName in SimSupportModuleTypeNames)
            {
                try
                {
                    Type ssmType = Type.GetType(ssmTypeName);
                    object ssm = Activator.CreateInstance(ssmType);
                    if (ssm is SimSupportModule)
                    {
                        if (toReturn == null) toReturn = new List<SimSupportModule>();
                        toReturn.Add((SimSupportModule) ssm);
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
            }
            return toReturn;
        }

        public static SimSupportModuleRegistry Load(string fileName)
        {
            return Serialization.Util.DeserializeFromXmlFile<SimSupportModuleRegistry>(fileName);
        }

        public void Save(string fileName)
        {
            Serialization.Util.SerializeToXmlFile(this, fileName);
        }
    }
}