using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Common.SimSupport;
using MFDExtractor.Configuration;
using MFDExtractor.UI;

namespace MFDExtractor
{
    internal interface IInstrumentFormFactory
    {
        InstrumentForm SetupInstrumentForm
            (
            Extractor extractor,
            Dictionary<IInstrumentRenderer, InstrumentForm> outputForms,
            string instrumentName,
            string formCaption,
            IInstrumentRenderer renderer,
            EventHandler disposeHandler,
            Image initialImage = null
            );
    }

    class InstrumentFormFactory : IInstrumentFormFactory
    {
        private readonly IInstrumentFormSettingsReader _instrumentFormSettingsReader;

        public InstrumentFormFactory(IInstrumentFormSettingsReader instrumentFormSettingsReader = null)
        {
            _instrumentFormSettingsReader = instrumentFormSettingsReader ?? new InstrumentFormSettingsReader();
        }
        public InstrumentForm SetupInstrumentForm
        (
            Extractor extractor,
            Dictionary<IInstrumentRenderer, InstrumentForm> outputForms,
            string instrumentName,
            string formCaption,
            IInstrumentRenderer renderer,
            EventHandler disposeHandler,
            Image initialImage = null
        )
        {
            var currentSettings = _instrumentFormSettingsReader.Read(instrumentName);
            if (!currentSettings.Enabled) return null;
            Point location;
            Size size;
            var screen = Common.Screen.Util.FindScreen(currentSettings.OutputDisplay);
            var instrumentForm = new InstrumentForm { Text = formCaption, ShowInTaskbar = false, ShowIcon = false };
            if (currentSettings.StretchToFit)
            {
                location = new Point(0, 0);
                size = screen.Bounds.Size;
                instrumentForm.StretchToFill = true;
            }
            else
            {
                location = new Point(currentSettings.ULX, currentSettings.ULY);
                size = new Size(currentSettings.LRX - currentSettings.ULX, currentSettings.LRY - currentSettings.ULY);
                instrumentForm.StretchToFill = false;
            }
            instrumentForm.AlwaysOnTop = currentSettings.AlwaysOnTop;
            instrumentForm.Monochrome = currentSettings.Monochrome;
            instrumentForm.Rotation = currentSettings.RotateFlipType;
            instrumentForm.WindowState = FormWindowState.Normal;
            Common.Screen.Util.OpenFormOnSpecificMonitor(instrumentForm, screen, location, size, true, true);
            instrumentForm.DataChanged += new InstrumentFormDataChangedHandler(instrumentName, instrumentForm, extractor).HandleDataChangedEvent;

            instrumentForm.Disposed += disposeHandler;
            if (renderer != null)
            {
                outputForms.Add(renderer, instrumentForm);
            }
            if (initialImage == null) return instrumentForm;
            using (var graphics = instrumentForm.CreateGraphics())
            {
                graphics.DrawImage(initialImage, instrumentForm.ClientRectangle);
            }
            return instrumentForm;
        }

    }
}
