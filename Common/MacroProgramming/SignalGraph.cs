using Common.Math;
using Common.MacroProgramming;
using Common.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;

namespace Common.MacroProgramming
{
    public class SignalGraph
    {
        private Signal _signal;
        private TimeSpan _duration;
        private List<TimestampedDecimal> _signalStateHistory = new List<TimestampedDecimal>();
        private DateTime _startTime = DateTime.Now;
        private static Color GridLineColor = Color.FromArgb(217,234,244);
        private static Color ValueCurveColor = Color.FromArgb(54, 145, 198);
        private static Pen GridLinePen = new Pen(GridLineColor) { Width = 1f };
        private static Pen ZeroLinePen = new Pen(Brushes.DarkBlue) { Width = 1f };
        private static Pen ValueCurvePen = new Pen(ValueCurveColor) { Width = 2f };
        private static Pen ValueLinePen = new Pen(Brushes.LightBlue) { Width = 1f };
        private static Pen MinValueLinePen = new Pen(Brushes.DarkRed) { Width = 1f };
        private static Pen MaxValueLinePen = new Pen(Brushes.DarkRed) { Width = 1f };
        private static Color AreaUnderTheCurveColor = Color.FromArgb(241, 246, 250);
        private static Brush AreaUnderTheCurveBrush = new SolidBrush(AreaUnderTheCurveColor);
        private static Brush ValueBrush = Brushes.Black;
        private static Brush MinValueBrush = Brushes.LightPink;
        private static Brush MaxValueBrush = Brushes.LightPink;
        private static Brush ValueInvertedBrush = Brushes.White;
        private static Brush ScaleFontBrush = Brushes.LightGray;
        private static Brush FriendlyNameBrush = Brushes.Black;
        private static Brush SubcollectionNameBrush = Brushes.Black;
        
        private static Font BigFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        private static Font MediumFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
        private static Font SmallFont = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
        private static Font SubcollectionNameFont = BigFont;
        private static Font FriendlyNameFont = MediumFont;
        private static Font ValueFont = MediumFont;
        private static Font MinValueFont = SmallFont;
        private static Font MaxValueFont = SmallFont;
        private static Font ScaleFont = SmallFont;
        public SignalGraph(Signal signal, int durationMs = 5000)
        {
            _signal = signal;
            _duration = TimeSpan.FromMilliseconds(durationMs);
            RegisterForChangedEvent(signal);
        }
        private void RegisterForChangedEvent(Signal signal)
        {
            if (signal is AnalogSignal)
            {
                (signal as AnalogSignal).SignalChanged += AnalogSignalChanged;
            }
            else if (signal is DigitalSignal)
            {
                (signal as DigitalSignal).SignalChanged += DigitalSignalChanged;
            }
        }
        private void AnalogSignalChanged(object sender, AnalogSignalChangedEventArgs args)
        {
            CaptureNewSample();
        }
        private void DigitalSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            CaptureNewSample();
        }
        private void CaptureNewSample()
        {
            PurgeOldSamples();
            if (_signal is AnalogSignal)
            {
                _signalStateHistory.Add(new TimestampedDecimal() { Timestamp = DateTime.Now, Value = ((AnalogSignal)_signal).State });
            }
            else if (_signal is DigitalSignal)
            {
                _signalStateHistory.Add(new TimestampedDecimal() { Timestamp = DateTime.Now, Value = ((DigitalSignal)_signal).State ? 1 : 0 });
            }
        }
        private void PurgeOldSamples()
        {
            _signalStateHistory.RemoveAll(x => x.Timestamp < DateTime.Now.Subtract(_duration));
        }
        public void Draw(Graphics graphics, Rectangle targetRectangle)
        {
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //graphics.CompositingQuality = CompositingQuality.HighQuality;
            //graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //graphics.SmoothingMode = SmoothingMode.HighQuality;
            var originalClip = graphics.Clip;
            var originalTransform = graphics.Transform;

            var drawTime = DateTime.Now;
            CaptureNewSample();
            string value = string.Empty;
            string rangeMinValue = string.Empty;
            string rangeMaxValue = string.Empty;
            var topMarginHeight = 40;
            var bottomMarginHeight = 20;
            var width = (float)targetRectangle.Width;
            var height = (float)targetRectangle.Height - topMarginHeight - bottomMarginHeight;
            graphics.SetClip(targetRectangle);
            graphics.Clear(Color.White);
            graphics.TranslateTransform(-targetRectangle.Location.X, -targetRectangle.Location.Y);
            graphics.DrawRectangle(GridLinePen, 0, 0, targetRectangle.Width, targetRectangle.Height);
            graphics.TranslateTransform(0, topMarginHeight);
            graphics.DrawRectangle(GridLinePen, 0, 0, width, height);

            var x1 = 0.0f;
            var y1 = height / 2.0f;
            var x2 = 0.0f;
            var y2 = height / 2.0f;
            var minY2 = 0.0f;
            var maxY2 = 0.0f;
            var minVal = 0.0;
            var maxVal = 0.0;
            var minValString = string.Empty;
            var maxValString = string.Empty;
            double range = 0;
            float zeroHeight = 0;
            var isFirstSample = true;
            var numYSegments = 20.0f;
            var numXSegments = 20.0f;
            var xOffset = -(float)(((drawTime.Subtract(_startTime).TotalMilliseconds % (_duration.TotalMilliseconds / numXSegments) * (width / _duration.TotalMilliseconds))));

            var pointList = new List<PointF>();
            
            foreach (var sample in _signalStateHistory)
            {
                x2 = width - ((float)(drawTime.Subtract(sample.Timestamp).TotalMilliseconds / _duration.TotalMilliseconds) * width);
                if (x2 < 0) x2 = 0;
                if (x2 > width) x2 = width;
                if (_signal is DigitalSignal)
                {
                    y2 = sample.Value == 1 ? 0 : height;
                    value = sample.Value ==1 ? "1" : "0";
                    rangeMinValue = "0";
                    rangeMaxValue = "1";
                    zeroHeight = height;
                    if (isFirstSample)
                    {
                        minVal = sample.Value;
                        maxVal = sample.Value;
                        minValString = sample.Value == 1 ? "1" : "0";
                        maxValString = sample.Value == 1 ? "1" : "0";
                    }
                    if (sample.Value == 0)
                    {
                        minVal = 0;
                        minValString = "0";
                    }
                    else if (sample.Value ==1)
                    {
                        maxVal = 1;
                        maxValString = "1";
                    }
                }
                else if (_signal is AnalogSignal)
                {
                    var thisSignal = _signal as AnalogSignal;
                    range = (thisSignal.MaxValue - thisSignal.MinValue);
                    if (sample.Value < minVal)
                    {
                        minVal = sample.Value;
                        minValString = minVal.FormatDecimal(thisSignal.Precision > -1 ? thisSignal.Precision : 4);
                    }
                    else if (sample.Value > maxVal)
                    {
                        maxVal = sample.Value;
                        maxValString = maxVal.FormatDecimal(thisSignal.Precision > -1 ? thisSignal.Precision : 4);
                    }

                    if (isFirstSample)
                    {
                        minVal = thisSignal.State;
                        maxVal = thisSignal.State;
                        minValString = minVal.FormatDecimal(thisSignal.Precision > -1 ? thisSignal.Precision : 4);
                        maxValString = maxVal.FormatDecimal(thisSignal.Precision > -1 ? thisSignal.Precision : 4);
                    }
                    y2 = height - (int)(((System.Math.Abs(sample.Value - thisSignal.MinValue)) / range) * height);
                    if (y2 <0) y2=0;
                    if (y2 > height) y2 = height;
                    zeroHeight = height - (int)(((System.Math.Abs(-thisSignal.MinValue)) / range) * height);
                    if (zeroHeight < 0) zeroHeight = 0;
                    if (zeroHeight > height) zeroHeight = height;
                    value = (sample.Value.FormatDecimal(thisSignal.Precision > -1 ? thisSignal.Precision : 4));
                    rangeMinValue = (thisSignal.MinValue.FormatDecimal(thisSignal.Precision > -1 ? thisSignal.Precision : 4));
                    rangeMaxValue = (thisSignal.MaxValue.FormatDecimal(thisSignal.Precision > -1 ? thisSignal.Precision : 4));
                }
                if (isFirstSample)
                {
                    x1 = x2;
                    y1 = y2;
                    minY2 = y2;
                    maxY2 = y2;
                    isFirstSample = false;
                }
                if (y2 < minY2)
                {
                    minY2 = y2;
                }
                else if (y2 > maxY2)
                {
                    maxY2 = y2;
                }
                pointList.Add(new PointF(x1, y1));
                pointList.Add(new PointF(x2, y2));
                graphics.FillPolygon(AreaUnderTheCurveBrush, new[] { new PointF(x1, y1), new PointF(x2, y2), new PointF(x2, height), new PointF(x1, height) });
                x1 = x2;
                y1 = y2;
            }
            for (var x = (float)width + xOffset; x >= 0; x -= (width / numXSegments))
            {
                graphics.DrawLine(GridLinePen, new PointF(x, 0), new PointF(x, height));
            }
            for (var y = 0.0f; y <= height; y += (height / numYSegments))
            {
                graphics.DrawLine(GridLinePen, new PointF(0, y), new PointF(width, y));
            }
            graphics.DrawLine(MinValueLinePen, new PointF(0, minY2), new PointF(width, minY2));
            graphics.DrawLine(MaxValueLinePen, new PointF(0, maxY2), new PointF(width, maxY2));
            if (pointList != null && pointList.Count > 0)
            {
                graphics.DrawLines(ValueCurvePen, pointList.ToArray());
            }
            graphics.DrawLine(ValueLinePen, new PointF(0, y2), new PointF(width, y2));
            graphics.DrawLine(ZeroLinePen, new PointF(0, zeroHeight), new PointF(width, zeroHeight));

            var valueTextSize = graphics.MeasureString(value, ValueFont);
            var valueFormat = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            var valueRectangle = new RectangleF(0, System.Math.Max( y2 - valueTextSize.Height,0), width, valueTextSize.Height);
            graphics.DrawString(value, ValueFont, ValueBrush, valueRectangle, valueFormat);

            var minValueTextSize = graphics.MeasureString(rangeMinValue, MinValueFont);
            var minValueFormat = new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
            var minValueRectangle = new RectangleF(0, System.Math.Min(maxY2+2, height), width, minValueTextSize.Height);
            graphics.DrawString(string.Format("Min:{0}",minValString), MinValueFont, MinValueBrush, minValueRectangle, minValueFormat);

            var maxValueTextSize = graphics.MeasureString(rangeMaxValue, MaxValueFont);
            var maxValueFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var maxValueRectangle = new RectangleF(0, System.Math.Max(minY2- maxValueTextSize.Height,0), width, maxValueTextSize.Height);
            graphics.DrawString(string.Format("Max:{0}",maxValString), MaxValueFont, MaxValueBrush, maxValueRectangle, maxValueFormat);

            graphics.Clip = originalClip;
            graphics.Transform = originalTransform;
            graphics.DrawString(_signal.SubcollectionName, SubcollectionNameFont, SubcollectionNameBrush, new RectangleF(0, 0, width, topMarginHeight));
            graphics.DrawString(_signal.FriendlyName, FriendlyNameFont, FriendlyNameBrush, new RectangleF(0, topMarginHeight / 2, width, topMarginHeight));
            graphics.DrawString(rangeMaxValue, ScaleFont, ScaleFontBrush, new RectangleF(0, 0, width, topMarginHeight), new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Far });
            graphics.DrawString(rangeMinValue, ScaleFont, ScaleFontBrush, new RectangleF(0, targetRectangle.Height - bottomMarginHeight, width, bottomMarginHeight), new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Far });
        }
    }
}
