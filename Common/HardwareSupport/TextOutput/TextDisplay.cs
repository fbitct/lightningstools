using System;
using Common.MacroProgramming;

namespace Common.HardwareSupport.TextOutput
{
    [Serializable]
    public abstract class TextDisplay : CompositeControl
    {
        private TextSignal _displayText;

        public TextDisplay()
        {
        }

        public TextDisplay(TextSignal displayText)
        {
            DisplayText = displayText;
        }

        public virtual TextSignal DisplayText
        {
            get { return _displayText; }
            set { StoreDisplayTextSignal(value, ref _displayText); }
        }

        protected virtual void StoreDisplayTextSignal(TextSignal displayTextSignal, ref TextSignal storageVariable)
        {
            displayTextSignal.SignalChanged += displayTextSignal_SignalChanged;
            storageVariable = displayTextSignal;
        }

        private void displayTextSignal_SignalChanged(object sender, TextSignalChangedEventArgs args)
        {
            UpdateOutputSignals();
        }

        protected abstract void UpdateOutputSignals();
    }
}