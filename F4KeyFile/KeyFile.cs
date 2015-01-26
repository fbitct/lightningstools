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
        private readonly FileInfo _file;

        private IList<ILineInFile> _lines = new List<ILineInFile>();
        private readonly IDictionary<string, IBinding> _callbackBindings = new Dictionary<string, IBinding>();
        public KeyFile() { }

        public KeyFile(FileInfo file)
        {
            _file = file;
        }

        public ILineInFile[] Lines
        {
            get { return _lines.ToArray(); }
            set
            {
                _lines = value;
                UpdateIndexOfCallbacks();
            }
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

        public void Load()
        {
            Load(_file);
        }

        public void Load(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            _lines.Clear();
            _callbackBindings.Clear();
            using (var sr = new StreamReader(file.FullName, 
                
                //Encoding.GetEncoding("iso-8859-1")
                Encoding.Default
                ))
            {
                var lineNum = 0;
                while (!sr.EndOfStream)
                {
                    lineNum++;
                    var currentLine = sr.ReadLine();
                    if (currentLine != null)
                    {
                        var currentLineTrim = currentLine.Trim();
                        if (currentLineTrim.StartsWith("/") || currentLineTrim.StartsWith("#"))
                        {
                            _lines.Add(new CommentLine(currentLine) {LineNum = lineNum});
                            continue;
                        }
                    }

                    var tokenList = Util.Tokenize(currentLine);
                    if (tokenList == null || tokenList.Count == 0)
                    {
                        _lines.Add(new BlankLine() { LineNum = lineNum });
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
                                _lines.Add(new UnparsableLine(currentLine) { LineNum = lineNum });
                                Log.WarnFormat("Line {0} in key file {1} could not be parsed.", lineNum, file.FullName);
                                continue;
                            }
                            directInputBinding.LineNum = lineNum;
                            _lines.Add(directInputBinding);
                        }
                        else
                        {
                            KeyBinding keyBinding;
                            var parsed = KeyBinding.TryParse(currentLine, out keyBinding);
                            if (!parsed)
                            {
                                _lines.Add(new UnparsableLine(currentLine) { LineNum = lineNum });
                                Log.WarnFormat("Line {0} in key file {1} could not be parsed.", lineNum, file.FullName);
                                continue;
                            }
                            keyBinding.LineNum = lineNum;
                            _lines.Add(keyBinding);
                        }
                    }
                    catch (Exception e)
                    {
                        _lines.Add(new UnparsableLine(currentLine) { LineNum = lineNum });
                        Log.Warn(string.Format("Line {0} in key file {1} could not be parsed.", lineNum, file.FullName), e);
                    }
                }
            }
            UpdateIndexOfCallbacks();
        }

        public void Save()
        {
            Save(_file);
        }

        public void Save(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            file.Delete();
            using (var fs = file.OpenWrite())
            using (var sw = new StreamWriter(fs, Encoding.Default))
            {
                foreach (var binding in _lines)
                {
                    sw.WriteLine(binding.ToString());
                }
                sw.Close();
                fs.Close();
            }
        }
    }
}