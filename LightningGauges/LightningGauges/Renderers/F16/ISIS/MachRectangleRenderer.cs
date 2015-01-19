using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace LightningGauges.Renderers.F16.ISIS
{
    internal static class MachRectangleRenderer
    {
        internal static void DrawMachRectangle(Graphics gfx, ref GraphicsState basicState, RectangleF topRectangle, InstrumentState instrumentState, PrivateFontCollection fonts)
        {
            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
            var machRectangle = new RectangleF(0, 12, 57, topRectangle.Height - 12);
            var machRectanglePen = new Pen(Color.White) {Width = 1};
            gfx.DrawRectangle(machRectanglePen, (int) machRectangle.X, (int) machRectangle.Y,
                (int) machRectangle.Width, (int) machRectangle.Height);

            var machNumberStringFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                              StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None
            };
            var machNumber = instrumentState.MachNumber;
            var machNumberString = string.Format("{0:0.00}", machNumber);
            var normalTransform = gfx.Transform;
            var machNumberRectangle = new RectangleF(machRectangle.X, machRectangle.Y - 2, machRectangle.Width,
                machRectangle.Height);
            machNumberRectangle.Offset(0, -7.5f);
            gfx.ScaleTransform(1, 1.50f);
            gfx.DrawString(machNumberString, new Font(fonts.Families[0], 15, FontStyle.Regular, GraphicsUnit.Point),
                Brushes.White, machNumberRectangle, machNumberStringFormat);
            gfx.Transform = normalTransform;

            var machLetterStringFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                              StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None
            };
            machRectangle.Offset(3, -1);
            gfx.DrawString("M", new Font(fonts.Families[0], 22, FontStyle.Regular, GraphicsUnit.Point),
                Brushes.White, machRectangle, machLetterStringFormat);


            GraphicsUtil.RestoreGraphicsState(gfx, ref basicState);
        }
    }
}