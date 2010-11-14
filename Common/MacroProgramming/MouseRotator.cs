using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Contexts;
namespace Common.MacroProgramming
{
    [Serializable]
    public sealed class MouseRotator:Chainable
    {
        private int _xCenter=0;
        private int _yCenter=0;
        private int _radius=0;
        private int _startAngle=0;
        private int _degreesRotation=0;
        private int _numSteps=0;
        private TimeSpan _stepDelay=TimeSpan.Zero;
        private DigitalSignal _in = null;
        private DigitalSignal _out = null;
        private Macro _macro = new Macro();

        public MouseRotator():base()
        {
            this.In = new DigitalSignal();
            this.Out= new DigitalSignal();
        }

        private void _in_SignalChanged(object sender, DigitalSignalChangedEventArgs e)
        {
            if (e.CurrentState)
            {
                if (_out != null)
                {
                    _out.State = false;
                }
                double degStartAngle = _startAngle;
                while (degStartAngle >= 360)
                {
                    degStartAngle -= 360;

                }
                double radiansPerDegree = (2 * global::System.Math.PI / 360);
                double radStartAngle = (degStartAngle * radiansPerDegree);
                double radDesiredRotation = (_degreesRotation * radiansPerDegree);
                double radiansPerStep = ((2 * global::System.Math.PI) / _numSteps);
                if (_degreesRotation < 0)
                {
                    radiansPerStep = 0 - radiansPerStep;
                }
                bool firstStep = true;
                for (double radAngle = radStartAngle;
                    global::System.Math.Abs(radAngle - radStartAngle) <
                        global::System.Math.Abs(radDesiredRotation);
                    radAngle += radiansPerStep)
                {
                    int x = _xCenter + (int)(_radius * global::System.Math.Sin(global::System.Math.PI - radAngle));
                    int y = _yCenter + (int)(_radius * global::System.Math.Cos(global::System.Math.PI - radAngle));
                    if (_macro != null && !firstStep)
                    {
                        _macro.In.State = true;
                    }
                    if (firstStep)
                    {
                        firstStep = false;
                    }
                    KeyAndMouseFunctions.MouseMoveAbsolute(x, y);
                    if (_macro != null)
                    {
                        _macro.In.State = false;
                    }
                    Thread.Sleep(_stepDelay);
                }
                if (_out != null)
                {
                    _out.State = true;
                }
            }
            else
            {
                if (_macro != null)
                {
                    _macro.In.State = false;
                }
                if (_out != null)
                {
                    _out.State = false;
                }
            }
        }
        public Macro Macro
        {
            get
            {
                return _macro;
            }
            set
            {
                if (value == null)
                {
                    value = new Macro();
                }
                _macro = value;
            }
        }
        public DigitalSignal Out
        {
            get
            {
                return _out;
            }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                _out = value;
            }
        }
        public DigitalSignal In
        {
            get
            {
                return _in;
            }
            set
            {
                if (value == null)
                {
                    value = new DigitalSignal();
                }
                value.SignalChanged += _in_SignalChanged;
                _in = value;
            }
        }
        public int XCenter
        {
            get
            {
                return _xCenter;
            }
            set
            {
                _xCenter = value;
            }
        }
        public int YCenter
        {
            get
            {
                return _yCenter;
            }
            set
            {
                _yCenter = value;
            }
        }
        public int Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
            }
        }
        public int StartAngle
        {
            get
            {
                return _startAngle;
            }
            set
            {
                _startAngle = value;
            }
        }
        public int DegreesRotation
        {
            get
            {
                return _degreesRotation;
            }
            set
            {
                _degreesRotation = value;
            }
        }
        public int NumSteps
        {
            get
            {
                return _numSteps;
            }
            set
            {
                _numSteps = value;
            }
        }
        public TimeSpan StepDelay
        {
            get
            {
                return _stepDelay;
            }
            set
            {
                _stepDelay = value;
            }
        }
        

    }
}
