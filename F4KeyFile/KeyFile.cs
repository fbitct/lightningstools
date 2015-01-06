using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class KeyFile
    {
        private readonly FileInfo _file;
        private List<IBinding> _bindings = new List<IBinding>();

        public KeyFile()
        {
        }

        public KeyFile(FileInfo file)
        {
            _file = file;
        }

        public IBinding[] Bindings
        {
            get
            {
                if (_bindings != null)
                {
                    return _bindings.ToArray();
                }
                return null;
            }
            set { _bindings = new List<IBinding>(value); }
        }

        public IBinding FindBindingForCallback(string callback)
        {
            return
                Bindings.FirstOrDefault(
                    thisBinding =>
                    thisBinding.Callback != null && callback != null &&
                    thisBinding.Callback.ToLowerInvariant().Trim() == callback.ToLowerInvariant().Trim());
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
                        if (currentLineTrim.StartsWith("/"))
                        {
                            _bindings.Add(new CommentLine(currentLine) {LineNum = lineNum});
                            continue;
                        }
                    }

                    var tokenList = Util.Tokenize(currentLine);
                    if (tokenList == null || tokenList.Count == 0 || (tokenList[0] !=null && tokenList[0].StartsWith("#")))
                    {
                        continue;
                    }
                    if (tokenList.Count < 7)
                    {
                        throw new InvalidDataException("Invalid file format detected at line:" + lineNum);
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
                        throw new InvalidDataException("Invalid file format detected at line:" + lineNum, e);
                    }
                    if (directInputBinding != null)
                    {
                        _bindings.Add(directInputBinding);
                    }
                    else
                    {
                        _bindings.Add(keyBinding);
                    }
                }
            }
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
                foreach (var binding in _bindings)
                {
                    sw.WriteLine(binding.ToString());
                }
                sw.Close();
                fs.Close();
            }
        }
    }
}