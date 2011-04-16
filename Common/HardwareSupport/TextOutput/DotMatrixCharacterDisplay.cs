using System;
using Common.MacroProgramming;

namespace Common.HardwareSupport.TextOutput
{
    [Serializable]
    public class DotMatrixCharacterDisplay : TextDisplay
    {
        private DigitalSignal[,] _outputLines;

        private DotMatrixCharacterDisplay()
        {
        }

        public DotMatrixCharacterDisplay(TextSignal displayText, DigitalSignal[,] outputLines) : base(displayText)
        {
            _outputLines = outputLines;
        }

        protected override void UpdateOutputSignals()
        {
        }
    }
}