using System;
using System.Drawing;
using System.Drawing.Text;
using Common.Imaging;

namespace LightningGauges.Renderers.F16.EHSI
{
    internal static class InstrumentModeRenderer
    {
        internal static void DrawInstrumentMode(Graphics g, RectangleF outerBounds, PrivateFontCollection fonts, InstrumentState instrumentState)
        {
            var fontFamily = fonts.Families[0];
            var labelFont = new Font(fontFamily, 25, FontStyle.Bold, GraphicsUnit.Point);

            var labelStringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip |
                              StringFormatFlags.NoWrap,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.None
            };

            const float letterHeight = 20;
            const float margin = 8;
            const float labelWidth = 50;

            var howLongSinceInstrumentModeChanged =
                DateTime.Now.Subtract(instrumentState.WhenInstrumentModeLastChanged);
            if (howLongSinceInstrumentModeChanged.TotalMilliseconds <= 2000)
            {
                var toDisplay = string.Empty;
                switch (instrumentState.InstrumentMode)
                {
                    case InstrumentModes.Unknown:
                        break;
                    case InstrumentModes.PlsTacan:
                        toDisplay = "PLS/TACAN";
                        break;
                    case InstrumentModes.Tacan:
                        toDisplay = "TACAN";
                        break;
                    case InstrumentModes.Nav:
                        toDisplay = "NAV";
                        break;
                    case InstrumentModes.PlsNav:
                        toDisplay = "PLS/NAV";
                        break;
                }
                if (!instrumentState.ShowBrightnessLabel)
                {
                    CenterLabelRenderer.DrawCenterLabel(g, outerBounds, toDisplay, fonts);
                }
            }

            //draw PLS label
            if (
                instrumentState.InstrumentMode == InstrumentModes.PlsNav
                ||
                instrumentState.InstrumentMode == InstrumentModes.PlsTacan
                )
            {
                var plsLabelRect = new RectangleF(outerBounds.Width*0.25f, outerBounds.Height - letterHeight - margin,
                    labelWidth, letterHeight);
                g.DrawStringFast("PLS", labelFont, Brushes.White, plsLabelRect, labelStringFormat);
            }

            if (
                instrumentState.InstrumentMode == InstrumentModes.PlsNav
                ||
                instrumentState.InstrumentMode == InstrumentModes.Nav
                )
            {
                var navLabelRect = new RectangleF(outerBounds.Width*0.7f, outerBounds.Height - letterHeight - margin,
                    labelWidth, letterHeight);
                g.DrawStringFast("NAV", labelFont, Brushes.White, navLabelRect, labelStringFormat);
            }

            if (
                instrumentState.InstrumentMode == InstrumentModes.PlsTacan
                ||
                instrumentState.InstrumentMode == InstrumentModes.Tacan
                )
            {
                var tacanLabelRect = new RectangleF(outerBounds.Width*0.7f, outerBounds.Height - letterHeight - margin,
                    labelWidth, letterHeight);
                g.DrawStringFast("TCN", labelFont, Brushes.White, tacanLabelRect, labelStringFormat);
            }
        }
    }
}