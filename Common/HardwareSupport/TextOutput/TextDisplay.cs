using System;
using System.Collections.Generic;
using System.Text;
using Common.MacroProgramming;

namespace Common.HardwareSupport.TextOutput
{
    [Serializable]
    public abstract class TextDisplay : CompositeControl
    {
        private TextSignal _displayText = null;
        public TextDisplay():base()
        {
        }
        public TextDisplay(TextSignal displayText)
        {
            this.DisplayText = displayText;
        }
        public virtual TextSignal DisplayText
        {
            get
            {
                return _displayText;
            }
            set
            {
                StoreDisplayTextSignal(value, ref _displayText);
            }
        }
        protected virtual void StoreDisplayTextSignal(TextSignal displayTextSignal, ref TextSignal storageVariable)
        {
            displayTextSignal.SignalChanged += new TextSignal.TextSignalChangedEventHandler(displayTextSignal_SignalChanged);
            storageVariable = displayTextSignal;
        }

        private void displayTextSignal_SignalChanged(object sender, TextSignalChangedEventArgs args)
        {
            UpdateOutputSignals();
        }
        protected abstract void UpdateOutputSignals();
    }
}
