using Common.Math;
using Common.MacroProgramming;
using Common.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.MacroProgramming
{
    public class SignalGraph
    {
        private Signal _signal;
        private TimeSpan _duration;
        private List<TimestampedDecimal> _signalStateHistory = new List<TimestampedDecimal>();
        private DateTime _startTime = DateTime.Now;
        private static Color GridLineColor = Color.FromArgb(217,234,244);
        private static Pen GridLinePen = new Pen(GridLineColor) { Width = 1f };
        private static Pen ZeroLinePen = new Pen(Brushes.DarkBlue) { Width = 1f };
        private static Pen ValueCurvePen = new Pen(Brushes.DarkBlue) { Width = 5f };
        private static Color AreaUnderTheCurveColor=Color.FromArgb(241, 246, 250);
        private static Brush AreaUnderTheCurveBrush = new SolidBrush(AreaUnderTheCurveColor);
        private static Font BigFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        private static Font SmallFont = new Font(FontFamily.GenericSansSerif, 6, FontStyle.Regular);
        private static object _locker = new object();

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
            lock (_locker)
            {
                CaptureNewSample();
            }
        }
        private void DigitalSignalChanged(object sender, DigitalSignalChangedEventArgs args)
        {
            lock (_locker)
            {
                CaptureNewSample();
            }
        }
        private void CaptureNewSample()
        {
            lock (_locker)
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
        }
        private void PurgeOldSamples()
        {
            lock (_locker)
            {
                _signalStateHistory.RemoveAll(x => x.Timestamp < DateTime.Now.Subtract(_duration));
            }
        }
        public void Draw(Graphics graphics, Rectangle targetRectangle)
        {
            lock (_locker)
            {
                var drawTime = DateTime.Now;
                CaptureNewSample();
                string value = string.Empty;
                var topMarginHeight = 40;
                var bottomMarginHeight = 20;
                var width = targetRectangle.Width;
                var height = targetRectangle.Height - topMarginHeight - bottomMarginHeight;
                var originalClip = graphics.Clip;
                var originalTransform = graphics.Transform;
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
                double range = 0;
                float zeroHeight = 0;
                var isFirstSample = true;
                var xOffset = -(float)(((drawTime.Subtract(_startTime).TotalMilliseconds % (_duration.TotalMilliseconds / 20) * (width / _duration.TotalMilliseconds))));

                foreach (var sample in _signalStateHistory)
                {
                    x2 = width - ((float)(drawTime.Subtract(sample.Timestamp).TotalMilliseconds / _duration.TotalMilliseconds) * width);
                    if (_signal is DigitalSignal)
                    {
                        var thisSignal = _signal as DigitalSignal;
                        y2 = thisSignal.State ? 0 : height;
                        value = thisSignal.State ? "1" : "0";
                    }
                    else if (_signal is AnalogSignal)
                    {
                        var thisSignal = _signal as AnalogSignal;
                        range = (thisSignal.MaxValue - thisSignal.MinValue);
                        y2 = height - (int)(((System.Math.Abs(sample.Value - thisSignal.MinValue)) / range) * height);
                        zeroHeight = height - (int)(((System.Math.Abs(-thisSignal.MinValue)) / range) * height);
                        value = (thisSignal.State.FormatDecimal(thisSignal.Precision > -1 ? thisSignal.Precision : 4));
                    }
                    if (isFirstSample)
                    {
                        x1 = x2;
                        y1 = y2;
                        isFirstSample = false;
                    }
                    graphics.DrawLine(ValueCurvePen, new PointF(x1, y1), new PointF(x2, y2));
                    graphics.FillPolygon(AreaUnderTheCurveBrush, new[] { new PointF(x1, y1), new PointF(x2, y2), new PointF(x2, height), new PointF(x1, height) });
                    x1 = x2;
                    y1 = y2;
                }
                for (var x = (float)width + xOffset; x >= 0; x -= (width / 20.0f))
                {
                    graphics.DrawLine(GridLinePen, new PointF(x, 0), new PointF(x, height));
                }
                for (var y = 0.0f; y <= height; y += (height / 10.0f))
                {
                    graphics.DrawLine(GridLinePen, new PointF(0, y), new PointF(width, y));
                }
                graphics.DrawString(value, SmallFont, Brushes.DarkGray, new Rectangle(0, targetRectangle.Height - bottomMarginHeight, width, bottomMarginHeight));
                graphics.DrawLine(ZeroLinePen, new PointF(0, zeroHeight), new PointF(width, zeroHeight));
                graphics.Clip = originalClip;
                graphics.Transform = originalTransform;
                graphics.DrawString(_signal.SubcollectionName, BigFont, Brushes.Black, new Rectangle(0, 0, width, topMarginHeight));
                graphics.DrawString(_signal.FriendlyName, BigFont, Brushes.DarkGray, new Rectangle(0, topMarginHeight / 2, width, topMarginHeight));
            }
        }
    }
}
