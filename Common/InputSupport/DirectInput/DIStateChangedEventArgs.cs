using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectInput;

namespace Common.InputSupport.DirectInput
{
    public class DIStateChangedEventArgs : EventArgs
    {
        private JoystickState? _previousState;
        private JoystickState? _newState;
        public DIStateChangedEventArgs()
            : base()
        {
        }
        public DIStateChangedEventArgs(JoystickState? newState, JoystickState? previousState)
        {
            _newState = newState;
            _previousState = previousState;
        }
        public JoystickState? NewState
        {
            get
            {
                return _newState;
            }
        }
        public JoystickState? PreviousState
        {
            get
            {
                return _previousState;
            }
        }
    }
}
