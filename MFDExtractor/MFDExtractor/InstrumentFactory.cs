namespace MFDExtractor
{
    internal interface IInstrumentFactory
    {
        IInstrument Create(InstrumentType instrumentType);
    }

    class InstrumentFactory : IInstrumentFactory
    {
        private readonly IInstrumentStateSnapshotCache _instrumentStateSnapshotCache;
        private readonly IRendererFactory _rendererFactory;
        public InstrumentFactory(
            InstrumentStateSnapshotCache instrumentStateSnapshotCache = null, 
            IRendererFactory rendererFactory=null)
        {
            _instrumentStateSnapshotCache = instrumentStateSnapshotCache ?? new InstrumentStateSnapshotCache();
            _rendererFactory = rendererFactory ?? new RendererFactory();
        }

        public IInstrument Create(InstrumentType instrumentType)
        {
            var renderer = _rendererFactory.CreateRenderer(instrumentType);
            var instrument = new Instrument(_instrumentStateSnapshotCache)
            {
                Type = instrumentType,
                Renderer =renderer,
            };
            return instrument;
        }
    }
}
