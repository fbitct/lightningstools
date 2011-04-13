using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.UI;

namespace MFDExtractor.Runtime.Settings
{
    public class GDIPlusSettings
    {
        private GDIPlusOptions _gdiPlusOptions = new GDIPlusOptions();

        public GDIPlusSettings()
            : base()
        {
            Initialize();
        }
        public  void Initialize()
        {
            _gdiPlusOptions.CompositingQuality = Properties.Settings.Default.CompositingQuality;
            _gdiPlusOptions.InterpolationMode = Properties.Settings.Default.InterpolationMode;
            _gdiPlusOptions.PixelOffsetMode = Properties.Settings.Default.PixelOffsetMode;
            _gdiPlusOptions.SmoothingMode = Properties.Settings.Default.SmoothingMode;
            _gdiPlusOptions.TextRenderingHint = Properties.Settings.Default.TextRenderingHint;
        }

    }
}
