using System.Threading;

namespace MFDExtractor
{
    internal interface IInstrumentFactory
    {
        IInstrument Create(InstrumentType instrumentType);
    }

    class InstrumentFactory : IInstrumentFactory
    {
        private readonly IInstrumentStateSnapshotCache _instrumentStateSnapshotCache;
        private readonly IInstrumentRendererSet _instrumentRendererSet;
        private readonly IInstrumentFormFactory _instrumentFormFactory;
        public InstrumentFactory(
            IInstrumentRendererSet instrumentRendererSet = null,
            InstrumentStateSnapshotCache instrumentStateSnapshotCache = null, 
            IInstrumentFormFactory instrumentFormFactory=null)
        {
            _instrumentRendererSet = instrumentRendererSet ?? new InstrumentRendererSet();
            _instrumentStateSnapshotCache = instrumentStateSnapshotCache ?? new InstrumentStateSnapshotCache();
            _instrumentFormFactory = instrumentFormFactory ?? new InstrumentFormFactory();
        }

        public IInstrument Create(InstrumentType instrumentType)
        {
            var instrument = new Instrument(_instrumentStateSnapshotCache)
            {
                Type = instrumentType,
                Renderer = _instrumentRendererSet[instrumentType],
                Form = _instrumentFormFactory.Create(
                    instrumentType.ToString(),
                    instrumentType.ToString(),
                    _instrumentRendererSet[instrumentType]
                    ),
            };
            return instrument;
        }
    }
}
