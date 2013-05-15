using System;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Common.UI
{
    [Serializable]
    public class GdiPlusOptions
    {
        public GdiPlusOptions()
        {
            InterpolationMode = InterpolationMode.Default;
            SmoothingMode = SmoothingMode.Default;
            PixelOffsetMode = PixelOffsetMode.Default;
            TextRenderingHint = TextRenderingHint.SystemDefault;
            CompositingQuality = CompositingQuality.Default;
        }

        public InterpolationMode InterpolationMode { get; set; }
        public SmoothingMode SmoothingMode { get; set; }
        public PixelOffsetMode PixelOffsetMode { get; set; }
        public TextRenderingHint TextRenderingHint { get; set; }
        public CompositingQuality CompositingQuality { get; set; }
    }
}