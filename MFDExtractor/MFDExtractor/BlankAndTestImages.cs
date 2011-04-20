using System.Drawing;
using Common.Imaging;
using MFDExtractor.Properties;

namespace MFDExtractor
{
    internal static class BlankAndTestImages
    {
        public static readonly Image Mfd4BlankImage = Util.CloneBitmap(Resources.rightMFDBlankImage);
        public static readonly Image Mfd3BlankImage = Util.CloneBitmap(Resources.leftMFDBlankImage);
        public static readonly Image LeftMfdBlankImage = Util.CloneBitmap(Resources.leftMFDBlankImage);
        public static readonly Image RightMfdBlankImage = Util.CloneBitmap(Resources.rightMFDBlankImage);
        public static readonly Image HudBlankImage = Util.CloneBitmap(Resources.hudBlankImage);
        public static readonly Image Mfd4TestAlignmentImage = Util.CloneBitmap(Resources.leftMFDTestAlignmentImage);
        public static readonly Image Mfd3TestAlignmentImage = Util.CloneBitmap(Resources.leftMFDTestAlignmentImage);
        public static readonly Image LeftMfdTestAlignmentImage = Util.CloneBitmap(Resources.leftMFDTestAlignmentImage);
        public static readonly Image RightMfdTestAlignmentImage = Util.CloneBitmap(Resources.rightMFDTestAlignmentImage);
        public static readonly Image HudTestAlignmentImage = Util.CloneBitmap(Resources.hudTestAlignmentImage);
    }
}