using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;
using MFDExtractor.Configuration;
using MFDExtractor.Properties;
using MFDExtractor.UI;

namespace MFDExtractor
{
    public interface IInstrumentFormDataChangedHandler
    {
        void HandleDataChangedEvent(object sender, EventArgs e);
    }

    internal class InstrumentFormDataChangedHandler : IInstrumentFormDataChangedHandler
    {
	    private readonly string _instrumentName;
        private readonly  InstrumentForm _instrumentForm;
        private readonly Extractor _extractor;
	    private readonly IInstrumentFormSettingsWriter _instrumentFormSettingsWriter;
        public InstrumentFormDataChangedHandler(
			string instrumentName,
            InstrumentForm instrumentForm, 
            Extractor extractor,
			IInstrumentFormSettingsWriter instrumentFormSettingsWriter = null
            )
        {
	        _instrumentName = instrumentName;
            _instrumentForm = instrumentForm;
            _extractor = extractor;
	        _instrumentFormSettingsWriter = instrumentFormSettingsWriter ?? new InstrumentFormSettingsWriter();
        }
        public void HandleDataChangedEvent(object sender, EventArgs e)
        {
            var location = _instrumentForm.DesktopLocation;
            var screen = Screen.FromRectangle(_instrumentForm.DesktopBounds);
	        var settings = new InstrumentFormSettings();
	        settings.OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (_instrumentForm.StretchToFill)
            {
	            settings.StretchToFit = true;
            }
            else
            {
				settings.StretchToFit = false;
                var size = _instrumentForm.Size;
	            settings.ULX = location.X - screen.Bounds.Location.X;
	            settings.ULY = location.Y - screen.Bounds.Location.Y;
	            settings.LRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.LRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
			settings.Enabled = _instrumentForm.Visible;
            settings.RotateFlipType = _instrumentForm.Rotation;
            settings.AlwaysOnTop = _instrumentForm.AlwaysOnTop;
            settings.Monochrome = _instrumentForm.Monochrome;
			_instrumentFormSettingsWriter.Write(_instrumentName, settings);
            
            SaveSettings();

        }

        private static void SaveSettings()
        {
            new TaskFactory().StartNew(()=> Settings.Default.Save());
        }
    }
}
