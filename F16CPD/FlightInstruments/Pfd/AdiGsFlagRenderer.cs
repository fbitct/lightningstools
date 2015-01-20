using System.Drawing;
using System.Drawing.Drawing2D;
using Common.Imaging;

namespace F16CPD.FlightInstruments.Pfd
{
    internal class AdiGsFlagRenderer
    {
        internal static void DrawAdiGsFlag(Graphics g, Point location, bool adiGlideslopeInvalidFlag, bool nightMode)
        {
            //draw ADI GS flag
            if (adiGlideslopeInvalidFlag)
            {
                var adiGsFlagFont = new Font("Lucida Console", 25, FontStyle.Bold);
                var path = new GraphicsPath();
                var adiGsFlagStringFormat = new StringFormat(StringFormatFlags.NoWrap)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near
                };
                var adiGsFlagTextLayoutRectangle = new Rectangle(location, new Size(60, 25));
                path.AddString("GS", adiGsFlagFont.FontFamily, (int)adiGsFlagFont.Style, adiGsFlagFont.SizeInPoints,
                    adiGsFlagTextLayoutRectangle, adiGsFlagStringFormat);
                var gsFlagBrush = Brushes.Red;
                var gsFlagTextBrush = Brushes.Black;
                if (nightMode)
                {
                    gsFlagBrush = Brushes.Black;
                    gsFlagTextBrush = Brushes.White;
                }
                g.FillRectangle(gsFlagBrush, adiGsFlagTextLayoutRectangle);
                g.DrawRectangleFast(Pens.Black, adiGsFlagTextLayoutRectangle);
                g.FillPathFast(gsFlagTextBrush, path);
            }
        }
    }
}