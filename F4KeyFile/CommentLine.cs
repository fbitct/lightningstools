using System;
using System.Runtime.InteropServices;
namespace F4KeyFile
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Serializable]
    public sealed class CommentLine:IBinding
    {
        private string _text;
        public CommentLine()
        {
        }
        public CommentLine(string text)
        {
            this.Text = text;
        }
        public string Callback
        {
            get
            {
                return null;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
        public int LineNum { get; set; }
        public string Text
        {
            get
            {
                return _text;
            }
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
        public override string ToString()
        {
            return _text;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!obj.GetType().Equals(this.GetType()))
            {
                return false;
            }
            if (obj.ToString() != this.ToString())
            {
                return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        public CommentLine Parse(string input)
        {
            return new CommentLine(input);
        }
    }
}
