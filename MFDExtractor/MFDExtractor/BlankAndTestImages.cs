using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MFDExtractor
{
    internal static class BlankAndTestImages
    {
        #region Blank Images
        /// <summary>
        /// Reference to a bitmap to display when the MFD #4 image data is not available
        /// </summary>
        private static readonly Image _mfd4BlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.rightMFDBlankImage); //TODO: change to MFD4
        public static Image Mfd4BlankImage
        {
            get { return _mfd4BlankImage; }
        }
        /// <summary>
        /// Reference to a bitmap to display when the MFD #3 image data is not available
        /// </summary>
        private static readonly Image _mfd3BlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDBlankImage); //TODO: change to MFD3
        public static Image Mfd3BlankImage
        {
            get { return _mfd3BlankImage; }
        }
        /// <summary>
        /// Reference to a bitmap to display when the Left MFD image data is not available
        /// </summary>
        private static readonly Image _leftMfdBlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDBlankImage);
        public static Image LeftMfdBlankImage
        {
            get { return _leftMfdBlankImage; }
        }
        /// <summary>
        /// Reference to a bitmap to display when the Right MFD image data is not available
        /// </summary>
        private static readonly Image _rightMfdBlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.rightMFDBlankImage);
        public static Image RightMfdBlankImage
        {
            get { return _rightMfdBlankImage; }
        }
        /// <summary>
        /// Reference to a bitmap to display when the HUD image data is not available
        /// </summary>
        private static readonly Image _hudBlankImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.hudBlankImage);
        public static Image HudBlankImage
        {
            get { return _hudBlankImage; }
        }
        #endregion

        #region Test/Alignment Images
        /// <summary>
        /// Reference to a bitmap to display when the MFD #4 is in test/alignment mode
        /// </summary>
        private static readonly Image _mfd4TestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDTestAlignmentImage);//TODO: change to MFD4
        public static Image Mfd4TestAlignmentImage
        {
            get { return _mfd4TestAlignmentImage; }
        }
        /// <summary>
        /// Reference to a bitmap to display when the MFD #3 is in test/alignment mode
        /// </summary>
        private static readonly Image _mfd3TestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDTestAlignmentImage);//TODO: change to MFD3
        public static Image Mfd3TestAlignmentImage
        {
            get { return _mfd3TestAlignmentImage; }
        }
        /// <summary>
        /// Reference to a bitmap to display when the Left MFD is in test/alignment mode
        /// </summary>
        private static readonly Image _leftMfdTestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.leftMFDTestAlignmentImage);
        public static Image LeftMfdTestAlignmentImage
        {
            get { return _leftMfdTestAlignmentImage; }
        }
        /// <summary>
        /// Reference to a bitmap to display when the Right MFD is in test/alignment mode
        /// </summary>
        private static readonly Image _rightMfdTestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.rightMFDTestAlignmentImage);
        public static Image RightMfdTestAlignmentImage
        {
            get { return _rightMfdTestAlignmentImage; }
        }
        /// <summary>
        /// Reference to a bitmap to display when the HUD is in test/alignment mode
        /// </summary>
        private static readonly Image _hudTestAlignmentImage = Common.Imaging.Util.CloneBitmap(Properties.Resources.hudTestAlignmentImage);
        public static Image HudTestAlignmentImage
        {
            get { return _hudTestAlignmentImage; }
        }
        #endregion

    }
}
