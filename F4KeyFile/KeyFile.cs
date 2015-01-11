using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using log4net;
namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class KeyFile
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(KeyFile));
        private readonly FileInfo _file;

        private IList<ILineInFile> _lines = new List<ILineInFile>();
        private readonly IDictionary<string, IBinding> _callbackBindings = new Dictionary<string, IBinding>();
        public KeyFile()
        {
        }

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
            _lines.OfType<KeyBinding>().Cast<IBinding>()
                .Union(_lines.OfType<DirectInputBinding>())
                .Select<IBinding, object>(x => _callbackBindings[x.Callback] = x);
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
            using (var sr = file.OpenText())
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
                        continue;
                    }
                    if (tokenList.Count < 7)
                    {
                        _lines.Add(new CommentLine(currentLine) { LineNum = lineNum });
                        _log.Warn(string.Format("Line {0} in key file {1} could not be parsed.", lineNum, file.FullName));
                        continue;
                    }
                    KeyBinding keyBinding = null;
                    DirectInputBinding directInputBinding = null;
                    try
                    {
                        var token2 = Int32.Parse(tokenList[1]);
                        if (token2 >= 0 && token2 < 1000)
                        {
                            directInputBinding = new DirectInputBinding().Parse(currentLine);
                            directInputBinding.LineNum = lineNum;
                        }
                        else
                        {
                            keyBinding = new KeyBinding().Parse(currentLine);
                            keyBinding.LineNum = lineNum;
                        }
                    }
                    catch (Exception e)
                    {
                        _lines.Add(new CommentLine(currentLine) { LineNum = lineNum });
                        _log.Warn(string.Format("Line {0} in key file {1} could not be parsed.", lineNum, file.FullName),e);
                        continue;
                    }
                    if (directInputBinding != null)
                    {
                        _lines.Add(directInputBinding);
                    }
                    else
                    {
                        _lines.Add(keyBinding);
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
            using (var sw = new StreamWriter(fs))
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