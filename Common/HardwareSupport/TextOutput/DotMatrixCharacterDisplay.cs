using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;

namespace Common.HardwareSupport.TextOutput
{
    [Serializable]
    public class DotMatrixCharacterDisplay : TextDisplay
    {
        private DigitalSignal[,] _outputLines = null;
        private DotMatrixCharacterDisplay():base()
        {
        }

        public DotMatrixCharacterDisplay(TextSignal displayText, DigitalSignal[,] outputLines):base(displayText)
        {
            _outputLines = outputLines;
        }
        protected override void UpdateOutputSignals()
        {
            
        }
    }
}
