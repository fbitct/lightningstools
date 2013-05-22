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
            Func<Image> localExtractFunc,
            Func<Image> remoteImageRetrievalFunc);
    }

    class ImageGetter : IImageGetter
    {
        public Image GetImage(
            ExtractorState extractorState,
            Image testAlignmentImage,
            Func<Image> localExtractFunc,
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
	            toReturn = (extractorState.NetworkMode == NetworkMode.Server || extractorState.NetworkMode == NetworkMode.Standalone)
		            ? localExtractFunc()
		            : remoteImageRetrievalFunc();
            }
	        return toReturn;

        }
    }
}
