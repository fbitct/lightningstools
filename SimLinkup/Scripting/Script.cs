using System;
using System.IO;
using System.Reflection;
using System.Xml.Schema;
using System.Xml.Serialization;
using CSScriptLibrary;

namespace SimLinkup.Scripting
{
    [Serializable]
    public class Script
    {
        private readonly string _code;

        [XmlAttribute(AttributeName = "language", DataType = "string", Form = XmlSchemaForm.None)]
        public string Language { get; set; }

        [XmlAttribute(AttributeName = "src", DataType = "string", Form = XmlSchemaForm.None)]
        public string Src { get; set; }

        [XmlAttribute(AttributeName = "requiredSignals", DataType = "string", Form = XmlSchemaForm.None)]
        public string RequiredSignals { get; set; }

        [XmlAttribute(AttributeName = "code", DataType = "string", Form = XmlSchemaForm.None)]
        public string Code
        {
            get { return _code; }
            set { Assembly = Compile(Language, value); }
        }

        [XmlIgnore]
        public Assembly Assembly { get; set; }

        public static Script Load(string basePath, string fileName, string language)
        {
            FileInfo[] foundScripts = new DirectoryInfo(basePath).GetFiles(fileName, SearchOption.AllDirectories);
            if (foundScripts == null || foundScripts.Length == 0)
            {
                throw new FileNotFoundException(fileName);
            }

            using (StreamReader sr = foundScripts[0].OpenText())
            {
                //read the contents of the script file
                string scriptFileContents = sr.ReadToEnd();
                Assembly assembly = Compile(language, scriptFileContents);
                return new Script {Assembly = assembly, Language = language, Src = fileName};
            }
        }

        private static Assembly Compile(string language, string code)
        {
            bool useAlternateCompiler = false;

            if (!string.IsNullOrEmpty(language))
            {
                if (
                    string.Compare(language, "C#", StringComparison.InvariantCultureIgnoreCase) != 0
                    &&
                    string.Compare(language, "CSS", StringComparison.InvariantCultureIgnoreCase) != 0
                    )
                {
                    useAlternateCompiler = true;
                }
            }

            string previousAlternateCompilerSetting = null;

            if (useAlternateCompiler)
            {
                //setup CS-Script so it uses the correct alternate compiler
                previousAlternateCompilerSetting = CSScript.GlobalSettings.UseAlternativeCompiler;
                string alternateCompilerLocation = Util.ApplicationPath;
                CSScript.GlobalSettings.UseAlternativeCompiler = alternateCompilerLocation;
            }

            //compile the code in the script file
            Assembly assembly = CSScript.LoadCode(code);

            if (useAlternateCompiler)
            {
                //reset CS-Script to use the builtin compiler
                CSScript.GlobalSettings.UseAlternativeCompiler = previousAlternateCompilerSetting;
            }

            return assembly;
        }
    }
}