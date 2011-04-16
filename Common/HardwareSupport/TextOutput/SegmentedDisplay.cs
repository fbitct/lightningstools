using System;
using Common.MacroProgramming;

namespace Common.HardwareSupport.TextOutput
{
    [Serializable]
    public abstract class SegmentedDisplay : TextDisplay
    {
        public SegmentedDisplay()
        {
        }

        public SegmentedDisplay(TextSignal displayText) : base(displayText)
        {
        }
    }
}