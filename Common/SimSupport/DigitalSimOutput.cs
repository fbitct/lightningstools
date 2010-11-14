using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;
namespace Common.SimSupport
{
    [Serializable]
    public class DigitalSimOutput:DigitalSignal, ISimOutput
    {
        public DigitalSimOutput()
            : base()
        {
        }
    }
}
