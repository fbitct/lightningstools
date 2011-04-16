using System;
using System.Xml.Serialization;

namespace SimLinkup.Scripting
{
    [Serializable]
    public class ScriptRegistry
    {
        [XmlArray("SetupScripts")]
        [XmlArrayItem("Script")]
        public Script[] SetupScripts { get; set; }

        [XmlArray("LoopScripts")]
        [XmlArrayItem("Script")]
        public Script[] LoopScripts { get; set; }

        [XmlArray("TeardownScripts")]
        [XmlArrayItem("Script")]
        public Script[] TeardownScripts { get; set; }

        public static ScriptRegistry Load(string fileName)
        {
            return Common.Serialization.Util.DeserializeFromXmlFile<ScriptRegistry>(fileName);
        }

        public void Save(string fileName)
        {
            Common.Serialization.Util.SerializeToXmlFile(this, fileName);
        }
    }
}