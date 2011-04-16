using System;
using System.Collections.Generic;

namespace Common.MacroProgramming
{
    [Serializable]
    public enum MathOperator
    {
        AbsoluteValue,
        Multiply,
        Divide,
        Add,
        Subtract,
        DivRem,
        Min,
        Max,
        Power,
        Sign,
        LeftShift,
        RightShift,
        Negate,
    }

    [Serializable]
    public sealed class MathOperation : Chainable
    {
        private List<AnalogSignal> _ins = new List<AnalogSignal>();
        private MathOperator _op = MathOperator.Max;
        private AnalogSignal _out;

        public MathOperation()
        {
            Out = new AnalogSignal();
        }

        public MathOperator Op
        {
            get { return _op; }
            set
            {
                _op = value;
                CalculateResult();
            }
        }

        public List<AnalogSignal> Ins
        {
            get { return _ins; }
            set
            {
                if (value == null)
                {
                    value = new List<AnalogSignal>();
                }
                for (int i = 0; i < value.Count; i++)
                {
                    if (value[i] != null)
                    {
                        value[i].SignalChanged += _in_SignalChanged;
                    }
                }
                _ins = value;
                CalculateResult();
            }
        }

        public AnalogSignal Out
        {
            get { return _out; }
            set
            {
                if (value == null)
                {
                    value = new AnalogSignal();
                }
                _out = value;
                CalculateResult();
            }
        }

        private void _in_SignalChanged(object sender, AnalogSignalChangedEventArgs e)
        {
            CalculateResult();
        }

        private void CalculateResult()
        {
            double result = 0;
            switch (_op)
            {
                case MathOperator.AbsoluteValue:
                    result = System.Math.Abs(_ins[0].State);
                    break;
                case MathOperator.Add:
                    for (int i = 0; i < _ins.Count; i++)
                    {
                        try
                        {
                            result += _ins[i].State;
                        }
                        catch (OverflowException)
                        {
                        }
                    }
                    break;
                case MathOperator.Divide:
                    result = _ins[0].State;
                    for (int i = 1; i < _ins.Count; i++)
                    {
                        try
                        {
                            result = result/_ins[i].State;
                        }
                        catch (OverflowException)
                        {
                        }
                        catch (DivideByZeroException)
                        {
                        }
                    }
                    break;
                case MathOperator.DivRem:
                    result = _ins[0].State;
                    for (int i = 1; i < _ins.Count; i++)
                    {
                        try
                        {
                            var curResult = (int) result;
                            int outResult = curResult;
                            System.Math.DivRem(curResult, (int) _ins[i].State, out outResult);
                            result = outResult;
                        }
                        catch (OverflowException)
                        {
                        }
                        catch (DivideByZeroException)
                        {
                        }
                    }
                    break;
                case MathOperator.LeftShift:
                    result = _ins[0].State;
                    for (int i = 0; i < _ins.Count; i++)
                    {
                        int numPlaces = 1;
                        try
                        {
                            numPlaces = (int) _ins[i].State;
                        }
                        catch (InvalidCastException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        result = (long) result << numPlaces;
                    }
                    break;
                case MathOperator.Max:
                    result = _ins[0].State;
                    for (int i = 0; i < _ins.Count; i++)
                    {
                        result = System.Math.Max(result, _ins[i].State);
                    }
                    break;
                case MathOperator.Min:
                    result = _ins[0].State;
                    for (int i = 0; i < _ins.Count; i++)
                    {
                        result = System.Math.Max(result, _ins[i].State);
                    }
                    break;
                case MathOperator.Multiply:
                    result = _ins[0].State;
                    for (int i = 1; i < _ins.Count; i++)
                    {
                        try
                        {
                            result *= _ins[i].State;
                        }
                        catch (OverflowException)
                        {
                        }
                    }
                    break;
                case MathOperator.Negate:
                    result = 0;
                    for (int i = 0; i < _ins.Count; i++)
                    {
                        try
                        {
                            result -= _ins[i].State;
                        }
                        catch (OverflowException)
                        {
                        }
                    }
                    break;
                case MathOperator.Power:
                    result = _ins[0].State;
                    for (int i = 1; i < _ins.Count; i++)
                    {
                        try
                        {
                            result *= _ins[i].State;
                        }
                        catch (OverflowException)
                        {
                        }
                    }
                    break;
                case MathOperator.RightShift:
                    result = _ins[0].State;
                    for (int i = 0; i < _ins.Count; i++)
                    {
                        int numPlaces = 1;
                        try
                        {
                            numPlaces = (int) _ins[i].State;
                        }
                        catch (InvalidCastException)
                        {
                        }
                        catch (OverflowException)
                        {
                        }
                        result = (long) result >> numPlaces;
                    }
                    break;
                case MathOperator.Sign:
                    for (int i = 0; i < _ins.Count; i++)
                    {
                        try
                        {
                            result += System.Math.Sign(_ins[i].State);
                        }
                        catch (OverflowException)
                        {
                        }
                    }

                    break;
                case MathOperator.Subtract:
                    result = _ins[0].State;
                    for (int i = 1; i < _ins.Count; i++)
                    {
                        try
                        {
                            result -= _ins[i].State;
                        }
                        catch (OverflowException)
                        {
                        }
                    }
                    break;
                default:
                    break;
            }
            _out.State = result;
        }
    }
}