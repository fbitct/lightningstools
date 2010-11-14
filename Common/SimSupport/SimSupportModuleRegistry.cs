using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using log4net;
using System.Windows.Forms;

namespace Common.SimSupport
{
    [Serializable]
    public class SimSupportModuleRegistry
    {
        private static ILog _log = LogManager.GetLogger(typeof(SimSupportModuleRegistry));
        public SimSupportModuleRegistry()
            : base()
        {
        }
        [XmlArray(ElementName = "SimSupportModules")]
        [XmlArrayItem("Module")]
        public string[] SimSupportModuleTypeNames
        {
            get;
            set;
        }

        public List<SimSupportModule> GetInstances()
        {
            List<SimSupportModule> toReturn = null;
            foreach (string ssmTypeName in this.SimSupportModuleTypeNames)
            {
                try
                {
                    Type ssmType = Type.GetType(ssmTypeName);
                    object ssm = Activator.CreateInstance(ssmType);
                    if (ssm is Common.SimSupport.SimSupportModule)
                    {
                        if (toReturn == null) toReturn = new List<Common.SimSupport.SimSupportModule>();
                        toReturn.Add((Common.SimSupport.SimSupportModule)ssm);
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
            return Common.Serialization.Util.DeserializeFromXmlFile<SimSupportModuleRegistry>(fileName);
        }
        public void Save(string fileName)
        {
            Common.Serialization.Util.SerializeToXmlFile<SimSupportModuleRegistry>(this, fileName);
        }

    }
}
