using System.Drawing;
using System.Windows.Forms;
using Common.SimSupport;
using MFDExtractor.Configuration;
using MFDExtractor.UI;

namespace MFDExtractor
{
    internal interface IInstrumentFormFactory
    {
        InstrumentForm Create
            (
            string instrumentName,
            string formCaption,
            IInstrumentRenderer renderer,
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
        public InstrumentForm Create
        (
            string instrumentName,
            string formCaption,
            IInstrumentRenderer renderer,
            Image initialImage = null
        )
        {
            var currentSettings = _instrumentFormSettingsReader.Read(instrumentName);
            if (!currentSettings.Enabled) return null;
            Point location;
            Size size;
            var screen = Common.Screen.Util.FindScreen(currentSettings.OutputDisplay);
            var instrumentForm = new InstrumentForm { Text = formCaption, ShowInTaskbar = false, ShowIcon = false, Settings = currentSettings};
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
            instrumentForm.DataChanged += new InstrumentFormDataChangedHandler(instrumentName, instrumentForm).HandleDataChangedEvent;

            if (initialImage == null) return instrumentForm;
            using (var graphics = instrumentForm.CreateGraphics())
            {
                graphics.DrawImage(initialImage, instrumentForm.ClientRectangle);
            }
            return instrumentForm;
        }

    }
}
