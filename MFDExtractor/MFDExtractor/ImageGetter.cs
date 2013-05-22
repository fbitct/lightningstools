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
            Func<Image> remoteImageRetrievalFunc);
    }

    class ImageGetter : IImageGetter
    {
        public Image GetImage(
            ExtractorState extractorState,
            Image testAlignmentImage,
            Func<Image> threeDeeModeLocalExtractFunc,
            Func<Image> remoteImageRetrievalFunc)
        {
            Image toReturn;
            if (extractorState.TestMode)
            {
                toReturn = Util.CloneBitmap(testAlignmentImage);
            }
            else
            {
	            if (!extractorState.SimRunning && extractorState.NetworkMode != NetworkMode.Client) return null;
	            toReturn = extractorState.ThreeDeeMode && (extractorState.NetworkMode == NetworkMode.Server || extractorState.NetworkMode == NetworkMode.Standalone)
		            ? threeDeeModeLocalExtractFunc()
		            : remoteImageRetrievalFunc();
            }
	        return toReturn;

        }
    }
}
