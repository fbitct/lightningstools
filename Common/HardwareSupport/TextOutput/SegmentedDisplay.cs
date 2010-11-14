using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;

namespace Common.HardwareSupport.TextOutput
{
    [Serializable]
    public abstract class SegmentedDisplay : TextDisplay
    {
        public SegmentedDisplay()
            : base()
        {
        }
        public SegmentedDisplay(TextSignal displayText):base(displayText)
        {
        }
    }
}
