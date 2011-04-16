using System.Drawing;
using Common.Imaging;
using MFDExtractor.Properties;

namespace MFDExtractor
{
    internal static class BlankAndTestImages
    {
        #region Blank Images

        /// <summary>
        /// Reference to a bitmap to display when the MFD #4 image data is not available
        /// </summary>
        private static readonly Image _mfd4BlankImage = Util.CloneBitmap(Resources.rightMFDBlankImage);
                                      //TODO: change to MFD4

        /// <summary>
        /// Reference to a bitmap to display when the MFD #3 image data is not available
        /// </summary>
        private static readonly Image _mfd3BlankImage = Util.CloneBitmap(Resources.leftMFDBlankImage);
                                      //TODO: change to MFD3

        /// <summary>
        /// Reference to a bitmap to display when the Left MFD image data is not available
        /// </summary>
        private static readonly Image _leftMfdBlankImage = Util.CloneBitmap(Resources.leftMFDBlankImage);

        /// <summary>
        /// Reference to a bitmap to display when the Right MFD image data is not available
        /// </summary>
        private static readonly Image _rightMfdBlankImage = Util.CloneBitmap(Resources.rightMFDBlankImage);

        /// <summary>
        /// Reference to a bitmap to display when the HUD image data is not available
        /// </summary>
        private static readonly Image _hudBlankImage = Util.CloneBitmap(Resources.hudBlankImage);

        public static Image Mfd4BlankImage
        {
            get { return _mfd4BlankImage; }
        }

        public static Image Mfd3BlankImage
        {
            get { return _mfd3BlankImage; }
        }

        public static Image LeftMfdBlankImage
        {
            get { return _leftMfdBlankImage; }
        }

        public static Image RightMfdBlankImage
        {
            get { return _rightMfdBlankImage; }
        }

        public static Image HudBlankImage
        {
            get { return _hudBlankImage; }
        }

        #endregion

        #region Test/Alignment Images

        /// <summary>
        /// Reference to a bitmap to display when the MFD #4 is in test/alignment mode
        /// </summary>
        private static readonly Image _mfd4TestAlignmentImage = Util.CloneBitmap(Resources.leftMFDTestAlignmentImage);
                                      //TODO: change to MFD4

        /// <summary>
        /// Reference to a bitmap to display when the MFD #3 is in test/alignment mode
        /// </summary>
        private static readonly Image _mfd3TestAlignmentImage = Util.CloneBitmap(Resources.leftMFDTestAlignmentImage);
                                      //TODO: change to MFD3

        /// <summary>
        /// Reference to a bitmap to display when the Left MFD is in test/alignment mode
        /// </summary>
        private static readonly Image _leftMfdTestAlignmentImage = Util.CloneBitmap(Resources.leftMFDTestAlignmentImage);

        /// <summary>
        /// Reference to a bitmap to display when the Right MFD is in test/alignment mode
        /// </summary>
        private static readonly Image _rightMfdTestAlignmentImage =
            Util.CloneBitmap(Resources.rightMFDTestAlignmentImage);

        /// <summary>
        /// Reference to a bitmap to display when the HUD is in test/alignment mode
        /// </summary>
        private static readonly Image _hudTestAlignmentImage = Util.CloneBitmap(Resources.hudTestAlignmentImage);

        public static Image Mfd4TestAlignmentImage
        {
            get { return _mfd4TestAlignmentImage; }
        }

        public static Image Mfd3TestAlignmentImage
        {
            get { return _mfd3TestAlignmentImage; }
        }

        public static Image LeftMfdTestAlignmentImage
        {
            get { return _leftMfdTestAlignmentImage; }
        }

        public static Image RightMfdTestAlignmentImage
        {
            get { return _rightMfdTestAlignmentImage; }
        }

        public static Image HudTestAlignmentImage
        {
            get { return _hudTestAlignmentImage; }
        }

        #endregion
    }
}