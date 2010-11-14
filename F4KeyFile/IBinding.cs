using System;
using System.Runtime.InteropServices;
namespace F4KeyFile
{
    [ComVisible (true)]
    public interface IBinding
    {
        String Callback
        {
            get;
            set;
        }
        int LineNum
        {
            get;
            set;
        }
    }
}
