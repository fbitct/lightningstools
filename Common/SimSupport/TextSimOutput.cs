using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;
namespace Common.SimSupport
{
    [Serializable]
    public class TextSimOutput:TextSignal, ISimOutput
    {
        public TextSimOutput()
            : base()
        {
        }

    }
}
