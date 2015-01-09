﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MFDExtractor.Configuration;
using MFDExtractor.Properties;
using MFDExtractor.UI;
using System.Threading;
using System.ComponentModel;

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
	    private readonly IInstrumentFormSettingsWriter _instrumentFormSettingsWriter;
        public InstrumentFormDataChangedHandler(
			string instrumentName,
            InstrumentForm instrumentForm, 
			IInstrumentFormSettingsWriter instrumentFormSettingsWriter = null
            )
        {
	        _instrumentName = instrumentName;
            _instrumentForm = instrumentForm;
	        _instrumentFormSettingsWriter = instrumentFormSettingsWriter ?? new InstrumentFormSettingsWriter();
        }
        public void HandleDataChangedEvent(object sender, EventArgs e)
        {
            var location = _instrumentForm.DesktopLocation;
            var screen = Screen.FromRectangle(_instrumentForm.DesktopBounds);
	        var settings = _instrumentForm.Settings;
	        settings.OutputDisplay = Common.Screen.Util.CleanDeviceName(screen.DeviceName);
            if (!_instrumentForm.StretchToFill)
            {
                var size = _instrumentForm.Size;
	            settings.ULX = location.X - screen.Bounds.Location.X;
	            settings.ULY = location.Y - screen.Bounds.Location.Y;
	            settings.LRX = (location.X - screen.Bounds.Location.X) + size.Width;
                settings.LRY = (location.Y - screen.Bounds.Location.Y) + size.Height;
            }
			settings.Enabled = _instrumentForm.Visible;
			_instrumentFormSettingsWriter.Write(_instrumentName, settings);
        }
    }
}
