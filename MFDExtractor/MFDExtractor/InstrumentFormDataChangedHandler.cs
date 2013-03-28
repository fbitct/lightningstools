using System;
using System.Drawing;
using System.Windows.Forms;
using MFDExtractor.UI;

namespace MFDExtractor
{
    public interface IInstrumentFormDataChangedHandler
    {
        void HandleDataChangedEvent(object sender, EventArgs e);
    }

    public class InstrumentFormDataChangedHandler : IInstrumentFormDataChangedHandler
    {
        private readonly  InstrumentForm _instrumentForm;
        private readonly Extractor _extractor;
        private readonly Action<string> _setOutputDisplay;
        private readonly Action<bool> _setStretchToFit;
        private readonly Action<int> _setOutULX;
        private readonly Action<int> _setOutULY;
        private readonly Action<int> _setOutLRX;
        private readonly Action<int> _setOutLRY;
        private readonly Action<bool> _setEnableOutput;
        private readonly Action<RotateFlipType> _setRotateFlipType;
        private readonly Action<bool> _setAlwaysOnTop;
        private readonly Action<bool> _setMonochrome;

        public InstrumentFormDataChangedHandler(
            InstrumentForm instrumentForm, 
            Extractor extractor,
            Action<string> setOutputDisplay,
            Action<bool> setStretchToFit,
            Action<int> setOutULX,
            Action<int> setOutULY,
            Action<int> setOutLRX,
            Action<int> setOutLRY,
            Action<bool> setEnableOutput,
            Action<RotateFlipType> setRotateFlipType,
            Action<bool> setAlwaysOnTop,
            Action<bool> setMonochrome

            )
        {
            _instrumentForm = instrumentForm;
            _extractor = extractor;
            _setOutputDisplay = setOutputDisplay;
            _setStretchToFit = setStretchToFit;
            _setOutULX = setOutULX;
            _setOutULY = setOutULY;
            _setOutLRX = setOutLRX;
            _setOutLRY = setOutLRY;
            _setEnableOutput = setEnableOutput;
            _setRotateFlipType = setRotateFlipType;
            _setAlwaysOnTop = setAlwaysOnTop;
            _setMonochrome = setMonochrome;
        }
        public void HandleDataChangedEvent(object sender, EventArgs e)
        {
            var location = _instrumentForm.DesktopLocation;
            var screen = Screen.FromRectangle(_instrumentForm.DesktopBounds);
            _setOutputDisplay(Common.Screen.Util.CleanDeviceName(screen.DeviceName));
            if (_instrumentForm.StretchToFill)
            {
                _setStretchToFit(true);
            }
            else
            {
                _setStretchToFit(false);
                var size = _instrumentForm.Size;
                _setOutULX(location.X - screen.Bounds.Location.X);
                _setOutULY(location.Y - screen.Bounds.Location.Y);
                _setOutLRX((location.X - screen.Bounds.Location.X) + size.Width);
                _setOutLRY((location.Y - screen.Bounds.Location.Y) + size.Height);
            }
            _setEnableOutput(_instrumentForm.Visible);
            _setRotateFlipType(_instrumentForm.Rotation);
            _setAlwaysOnTop(_instrumentForm.AlwaysOnTop);
            _setMonochrome( _instrumentForm.Monochrome);
            if (!_extractor.TestMode)
            {
                _extractor.SettingsSaveScheduled = true;
            }
            _extractor.SettingsSaveScheduled = true;
        }
    }
}
