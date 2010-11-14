using System;
using System.Collections.Generic;
using System.Text;

namespace F16CPD.Mfd.Controls
{
    public class MomentaryButtonPressedEventArgs : EventArgs
    {
        private DateTime _whenPressed;
        public DateTime WhenPressed
        {
            get
            {
                return _whenPressed;
            }
        }
        public MomentaryButtonPressedEventArgs()
            : this(DateTime.Now)
        {
        }
        public MomentaryButtonPressedEventArgs(DateTime whenPressed)
            : base()
        {
            _whenPressed = whenPressed;
        }

    }
    public class MomentaryButtonMfdInputControl:MfdInputControl
    {
        public MomentaryButtonMfdInputControl():base()
        {
        }
        public event EventHandler<MomentaryButtonPressedEventArgs> Pressed;
        public void Press(DateTime whenPressed)
        {
            OnPressed(whenPressed);
        }
        protected virtual void OnPressed(DateTime whenPressed)
        {
            if (Pressed != null)
            {
                Pressed(this, new MomentaryButtonPressedEventArgs(whenPressed));
            }
        }

    }
}
