using System;
using Microsoft.DirectX.DirectInput;

namespace Common.InputSupport.DirectInput
{
    public class DIStateChangedEventArgs : EventArgs
    {
        private readonly JoystickState? _newState;
        private readonly JoystickState? _previousState;

        public DIStateChangedEventArgs()
        {
        }

        public DIStateChangedEventArgs(JoystickState? newState, JoystickState? previousState)
        {
            _newState = newState;
            _previousState = previousState;
        }

        public JoystickState? NewState
        {
            get { return _newState; }
        }

        public JoystickState? PreviousState
        {
            get { return _previousState; }
        }
    }
}