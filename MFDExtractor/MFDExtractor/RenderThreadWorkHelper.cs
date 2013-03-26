using System;
using System.Drawing;
using System.Threading;
using Common.SimSupport;
using MFDExtractor.UI;

namespace MFDExtractor
{
    public interface IRenderThreadWorkHelper
    {
        void DoWork();
    }

    public sealed class RenderThreadWorkHelper : IRenderThreadWorkHelper
    {
        private readonly Func<bool> _keepRunningFunc;
        private readonly WaitHandle _startSignal;
        private readonly AutoResetEvent _endSignal;
        private readonly IInstrumentRenderer _renderer;
        private readonly InstrumentForm _targetForm;
        private readonly Func<RotateFlipType> _rotateFlipTypeFunc;
        private readonly Func<bool> _monochromeFunc;
        private readonly Action<IInstrumentRenderer, InstrumentForm, RotateFlipType, bool> _renderFunc;

        public RenderThreadWorkHelper(
            Func<bool> keepRunningFunc, 
            WaitHandle startSignal, 
            AutoResetEvent endSignal, 
            IInstrumentRenderer renderer, 
            InstrumentForm targetForm, 
            Func<RotateFlipType> rotateFlipTypeFunc,
            Func<bool> monochromeFunc,
            Action<IInstrumentRenderer, InstrumentForm, RotateFlipType, bool> renderFunc)
        {
            _keepRunningFunc = keepRunningFunc;
            _startSignal = startSignal;
            _endSignal = endSignal;
            _renderer = renderer;
            _targetForm = targetForm;
            _rotateFlipTypeFunc = rotateFlipTypeFunc;
            _monochromeFunc = monochromeFunc;
            _renderFunc = renderFunc;

        }
        public void DoWork()
        {
            try
            {
                while (_keepRunningFunc())
                {
                    _startSignal.WaitOne();
                    _renderFunc(_renderer, _targetForm, _rotateFlipTypeFunc(),_monochromeFunc());
                    _endSignal.Set();
                }
            }
            catch (ThreadAbortException) {}
            catch (ThreadInterruptedException){}
        }
    }
}