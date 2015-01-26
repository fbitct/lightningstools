using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using log4net;
namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class KeyFile
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(KeyFile));
        
        private IList<ILineInFile> _lines = new List<ILineInFile>();
        private readonly IDictionary<string, IBinding> _callbackBindings = new Dictionary<string, IBinding>();
        private Encoding _encoding = Encoding.Default;
        public KeyFile() { }

        public string FileName { get; internal set; }
        public ILineInFile[] Lines
        {
            get { return _lines.ToArray(); }
            set
            {
                _lines = value;
                UpdateIndexOfCallbacks();
            }
        }

        public Encoding Encoding
        {
            get { return _encoding;}
            set { _encoding = value ?? Encoding.Default; }
        }

        private void UpdateIndexOfCallbacks()
        {
            _callbackBindings.Clear();
            foreach (var binding in 
                            _lines.OfType<KeyBinding>().Cast<IBinding>()
                    .Union(_lines.OfType<DirectInputBinding>()))
            {
                _callbackBindings[binding.Callback] = binding;
            }
                
        }
        public IBinding GetBindingForCallback(string callback)
        {
            return _callbackBindings.ContainsKey(callback) ? _callbackBindings[callback] : null;
        }

        public static KeyFile Load(string fileName)
        {
            var encoding = Util.GetEncoding(fileName) ?? Encoding.Default;
            return Load(fileName, encoding);
        }
        public static KeyFile Load(string fileName, Encoding encoding)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            var file = new FileInfo(fileName);
            var keyFile = new KeyFile {FileName = file.FullName, _encoding = encoding};
            using (var sr = new StreamReader(file.FullName, encoding))
            {
                var lineNum = 0;
                while (!sr.EndOfStream)
                {
                    lineNum++;
                    var currentLine = sr.ReadLine();
                    if (currentLine != null)
                    {
                        var currentLineTrim = currentLine.Trim();
                        if (currentLineTrim.TrimStart().StartsWith("/") || currentLineTrim.TrimStart().StartsWith("#"))
                        {
                            keyFile._lines.Add(new CommentLine(currentLine) { LineNum = lineNum });
                            continue;
                        }
                    }

                    var tokenList = Util.Tokenize(currentLine);
                    if (tokenList == null || tokenList.Count == 0)
                    {
                        keyFile._lines.Add(new BlankLine() { LineNum = lineNum });
                        continue;
                    }

                    try
                    {
                        int token4;
                        Int32.TryParse(tokenList[3],out token4);
                        if (token4 ==-1 || token4 == -2 || token4 ==-4 || token4 == 8)
                        {
                            DirectInputBinding directInputBinding;
                            var parsed = DirectInputBinding.TryParse(currentLine, out directInputBinding);
                            if (!parsed)
                            {
                                keyFile._lines.Add(new UnparsableLine(currentLine) { LineNum = lineNum });
                                Log.WarnFormat("Line {0} in key file {1} could not be parsed.", lineNum, file.FullName);
                                continue;
                            }
                            directInputBinding.LineNum = lineNum;
                            keyFile._lines.Add(directInputBinding);
                        }
                        else
                        {
                            KeyBinding keyBinding;
                            var parsed = KeyBinding.TryParse(currentLine, out keyBinding);
                            if (!parsed)
                            {
                                keyFile._lines.Add(new UnparsableLine(currentLine) { LineNum = lineNum });
                                Log.WarnFormat("Line {0} in key file {1} could not be parsed.", lineNum, file.FullName);
                                continue;
                            }
                            keyBinding.LineNum = lineNum;
                            keyFile._lines.Add(keyBinding);
                        }
                    }
                    catch (Exception e)
                    {
                        keyFile._lines.Add(new UnparsableLine(currentLine) { LineNum = lineNum });
                        Log.Warn(string.Format("Line {0} in key file {1} could not be parsed.", lineNum, file.FullName), e);
                    }
                }
            }
            keyFile.UpdateIndexOfCallbacks();
            return keyFile;
        }

        public void Save(string fileName)
        {
            Save(fileName, _encoding ?? Encoding.Default);
        }
        public void Save(string fileName, Encoding encoding)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            var file = new FileInfo(fileName);
            if (file.Exists)
            {
                file.Delete();
            }
            using (var fs = file.OpenWrite())
            using (var sw = new StreamWriter(fs, encoding ?? Encoding.Default))
            {
                foreach (var binding in _lines)
                {
                    sw.WriteLine(binding.ToString());
                }
                sw.Close();
                fs.Close();
            }
            FileName = fileName;
        }
    }
}