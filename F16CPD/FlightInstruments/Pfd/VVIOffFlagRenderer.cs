using System.Drawing;
using System.Drawing.Drawing2D;

namespace F16CPD.FlightInstruments.Pfd
{
    internal class VVIOffFlagRenderer
    {

        internal static void DrawVVIOffFlag(Graphics g, Point location, bool vviOffFlag, bool nightMode)
        {
            //draw VVI OFF flag
            if (vviOffFlag)
            {
                var vviOffFlagFont = new Font("Lucida Console", 25, FontStyle.Bold);
                var path = new GraphicsPath();
                var vviOffFlagStringFormat = new StringFormat(StringFormatFlags.NoWrap)
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near
                };
                var vviOffFlagTextLayoutRectangle = new Rectangle(location, new Size(60, 25));
                path.AddString("OFF", vviOffFlagFont.FontFamily, (int) vviOffFlagFont.Style, vviOffFlagFont.SizeInPoints,
                    vviOffFlagTextLayoutRectangle, vviOffFlagStringFormat);
                var vviOffFlagBrush = Brushes.Red;
                var vviOffFlagTextBrush = Brushes.Black;
                if (nightMode)
                {
                    vviOffFlagBrush = Brushes.Black;
                    vviOffFlagTextBrush = Brushes.White;
                }
                g.FillRectangle(vviOffFlagBrush, vviOffFlagTextLayoutRectangle);
                g.DrawRectangle(Pens.Black, vviOffFlagTextLayoutRectangle);
                g.FillPath(vviOffFlagTextBrush, path);
            }
        }
    }
}