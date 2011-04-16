using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using log4net;

namespace Common.HardwareSupport
{
    [Serializable]
    public class HardwareSupportModuleRegistry
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (HardwareSupportModuleRegistry));

        [XmlArray(ElementName = "HardwareSupportModules")]
        [XmlArrayItem("Module")]
        public string[] HardwareSupportModuleTypeNames { get; set; }

        public List<IHardwareSupportModule> GetInstances()
        {
            List<IHardwareSupportModule> toReturn = null;
            foreach (string hsmTypeName in HardwareSupportModuleTypeNames)
            {
                try
                {
                    Type hsmType = Type.GetType(hsmTypeName);
                    MethodInfo method = hsmType.GetMethod
                        (
                            "GetInstances",
                            BindingFlags.Public
                            |
                            BindingFlags.Static
                            |
                            BindingFlags.InvokeMethod,
                            null,
                            new Type[] {},
                            null
                        );
                    if (method != null)
                    {
                        object hsmArray = method.Invoke(null, null);
                        if (hsmArray is IHardwareSupportModule[])
                        {
                            if (toReturn == null)
                            {
                                toReturn = new List<IHardwareSupportModule>();
                            }
                            toReturn.AddRange((IHardwareSupportModule[]) hsmArray);
                        }
                    }
                    else
                    {
                        if (toReturn == null)
                        {
                            toReturn = new List<IHardwareSupportModule>();
                        }
                        toReturn.Add((IHardwareSupportModule) Activator.CreateInstance(hsmType));
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e.Message, e);
                }
            }
            return toReturn;
        }

        public static HardwareSupportModuleRegistry Load(string fileName)
        {
            return Serialization.Util.DeserializeFromXmlFile<HardwareSupportModuleRegistry>(fileName);
        }

        public void Save(string fileName)
        {
            Serialization.Util.SerializeToXmlFile(this, fileName);
        }
    }
}