using F4SharedMem;

namespace MFDExtractor.BMSSupport
{
    internal interface ISharedmemReaderFactory
    {
        Reader Create();
    }

    class SharedmemReaderFactory : ISharedmemReaderFactory
    {
        private Reader _cachedReader;
        public SharedmemReaderFactory(Reader falconSmReader =null)
        {
            _cachedReader = falconSmReader  ?? new Reader();
        }

        public Reader Create()
        {
            EnsureCachedReaderDataFormatMatchesDetectedFormat();
            return _cachedReader;
        }

        private void EnsureCachedReaderDataFormatMatchesDetectedFormat()
        {
            var falconDataFormat = F4Utils.Process.Util.DetectFalconFormat();
            if (falconDataFormat == null) DisposeReader();
            if (_cachedReader == null)
            {
                CreateReader(falconDataFormat);
            }
            else if (falconDataFormat.HasValue && falconDataFormat.Value != _cachedReader.DataFormat)
            {
                DisposeReader();
                _cachedReader = new Reader(falconDataFormat.Value);
            }
        }

        private void CreateReader(FalconDataFormats? falconDataFormat)
        {
            _cachedReader = falconDataFormat.HasValue
                ? new Reader(falconDataFormat.Value)
                : new Reader();
        }
        private void DisposeReader()
        {
            Common.Util.DisposeObject(_cachedReader);
            _cachedReader = null;
        }

    }
}
