using System;
using System.Runtime.InteropServices;

namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class CommentLine : ILineInFile
    {
        private string _text;

        public CommentLine()
        {
        }

        public CommentLine(string text)
        {
            Text = text;
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (value != null)
                {
                    while (!value.Trim().StartsWith("//"))
                    {
                        value = "/" + value;
                    }
                }
                else
                {
                    value = "//";
                }
                _text = value;
            }
        }

        #region IBinding Members

        public int LineNum { get; set; }

        #endregion

        public override string ToString()
        {
            return _text;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!obj.GetType().Equals(GetType()))
            {
                return false;
            }
            if (obj.ToString() != ToString())
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public CommentLine Parse(string input)
        {
            return new CommentLine(input);
        }
    }
}