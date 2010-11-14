using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;
namespace Common.SimSupport
{
    [Serializable]
    public class AnalogSimOutput:AnalogSignal, ISimOutput
    {
        public AnalogSimOutput():base() 
        {
        }
    }
}
