using System;
using System.Drawing;
using Common.Imaging;
using Common.Networking;

namespace MFDExtractor
{
    internal interface IImageGetter
    {
        Image GetImage(
            ExtractorState extractorState,
            Image testAlignmentImage,
            Func<Image> threeDeeModeLocalExtractFunc,
            Func<Image> remoteImageRetrievalFunc,
            CaptureCoordinates captureCoordinates);
    }

    class ImageGetter : IImageGetter
    {
        public Image GetImage(
            ExtractorState extractorState,
            Image testAlignmentImage,
            Func<Image> threeDeeModeLocalExtractFunc,
            Func<Image> remoteImageRetrievalFunc,
            CaptureCoordinates captureCoordinates)
        {
            Image toReturn = null;
            if (extractorState.TestMode)
            {
                toReturn = Util.CloneBitmap(testAlignmentImage);
            }
            else
            {
                if (!extractorState.SimRunning && extractorState.NetworkMode != NetworkMode.Client) return null;
                if (extractorState.ThreeDeeMode && (extractorState.NetworkMode == NetworkMode.Server || extractorState.NetworkMode == NetworkMode.Standalone))
                {
                    toReturn = threeDeeModeLocalExtractFunc();
                }
                else
                {
                    switch (extractorState.NetworkMode)
                    {
                        case NetworkMode.Server://falls through to the standalone case 
                        case NetworkMode.Standalone: 
                            toReturn = Common.Screen.Util.CaptureScreenRectangle(
                                extractorState.TwoDeePrimaryView
                                    ? captureCoordinates.Primary2D
                                    : captureCoordinates.Secondary2D);
                            break;
                        case NetworkMode.Client:
                            toReturn = remoteImageRetrievalFunc();
                            break;
                    }
                }
            }
            return toReturn;

        }
    }
}
