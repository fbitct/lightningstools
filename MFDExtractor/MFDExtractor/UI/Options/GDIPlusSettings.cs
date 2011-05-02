using System;
using System.Collections.Generic;
using MFDExtractor.Properties;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace MFDExtractor.UI.Options
{
    public partial class OptionsForm
    {
        private void PopulateGDIPlusOptionsCombos()
        {
            cbInterpolationMode.Items.Clear();
            var interpolationModes = new List<InterpolationMode>();
            foreach (var val in Enum.GetValues(typeof(InterpolationMode)))
            {
                if ((InterpolationMode)val != InterpolationMode.Invalid)
                {
                    interpolationModes.Add((InterpolationMode)val);
                }
            }
            cbInterpolationMode.DataSource = interpolationModes;

            cbSmoothingMode.Items.Clear();
            var smoothingModes = new List<SmoothingMode>();
            var vals = Enum.GetValues(typeof(SmoothingMode));
            Array.Sort(vals);
            foreach (object val in vals)
            {
                if ((SmoothingMode)val != SmoothingMode.Invalid)
                {
                    smoothingModes.Add((SmoothingMode)val);
                }
            }
            cbSmoothingMode.DataSource = smoothingModes;


            cbPixelOffsetMode.Items.Clear();
            var pixelOffsetModes = new List<PixelOffsetMode>();
            vals = Enum.GetValues(typeof(PixelOffsetMode));
            Array.Sort(vals);
            foreach (object val in vals)
            {
                if ((PixelOffsetMode)val != PixelOffsetMode.Invalid)
                {
                    pixelOffsetModes.Add((PixelOffsetMode)val);
                }
            }
            cbPixelOffsetMode.DataSource = pixelOffsetModes;

            cbTextRenderingHint.Items.Clear();
            var vals2 = Enum.GetValues(typeof(TextRenderingHint));
            Array.Sort(vals2);
            cbTextRenderingHint.DataSource = vals2;

            cbCompositingQuality.Items.Clear();
            var compositingQualities = new List<CompositingQuality>();
            vals = Enum.GetValues(typeof(CompositingQuality));
            Array.Sort(vals);
            foreach (object val in vals)
            {
                if ((CompositingQuality)val != CompositingQuality.Invalid)
                {
                    compositingQualities.Add((CompositingQuality)val);
                }
            }
            cbCompositingQuality.DataSource = compositingQualities;
        }
        private void LoadGDIPlusSettings()
        {
            cbInterpolationMode.SelectedItem = Settings.Default.InterpolationMode;
            cbSmoothingMode.SelectedItem = Settings.Default.SmoothingMode;
            cbPixelOffsetMode.SelectedItem = Settings.Default.PixelOffsetMode;
            cbTextRenderingHint.SelectedItem = Settings.Default.TextRenderingHint;
            cbCompositingQuality.SelectedItem = Settings.Default.CompositingQuality;
        }
        private void SaveGDIPlusOptionsToSettings()
        {
            Settings.Default.InterpolationMode = (InterpolationMode)cbInterpolationMode.SelectedItem;
            Settings.Default.SmoothingMode = (SmoothingMode)cbSmoothingMode.SelectedItem;
            Settings.Default.PixelOffsetMode = (PixelOffsetMode)cbPixelOffsetMode.SelectedItem;
            Settings.Default.TextRenderingHint = (TextRenderingHint)cbTextRenderingHint.SelectedItem;
            Settings.Default.CompositingQuality = (CompositingQuality)cbCompositingQuality.SelectedItem;
        }

    }
}
