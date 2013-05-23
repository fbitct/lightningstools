using System;
using System.Collections.Concurrent;
using Common.SimSupport;
using MFDExtractor.UI;

namespace MFDExtractor
{
	internal interface IInstrumentStateSnapshotCache
	{
		bool CaptureInstrumentStateSnapshotAndCheckIfStale(IInstrumentRenderer renderer, InstrumentForm instrumentForm);
	}

	class InstrumentStateSnapshotCache : IInstrumentStateSnapshotCache
	{
		private readonly ConcurrentDictionary<IInstrumentRenderer, InstrumentStateSnapshot> _instrumentStates = new ConcurrentDictionary<IInstrumentRenderer, InstrumentStateSnapshot>();

		public bool CaptureInstrumentStateSnapshotAndCheckIfStale(IInstrumentRenderer renderer, InstrumentForm instrumentForm)
		{
		    if (renderer == null) return false;
			const int staleDataTimeout = 500;
			var oldStateSnapshot = _instrumentStates.ContainsKey(renderer) ? _instrumentStates[renderer] : InstrumentStateSnapshot.Default;
			var newStateSnapshot = CaptureStateSnapshot(renderer);
			var hashesAreDifferent = (oldStateSnapshot.HashCode != newStateSnapshot.HashCode);
			var timeSinceHashChanged = (int)Math.Floor(DateTime.Now.Subtract(oldStateSnapshot.DateTime).TotalMilliseconds);
			var isStale= (hashesAreDifferent ||timeSinceHashChanged > staleDataTimeout);
            if (isStale)
            {
                 _instrumentStates.AddOrUpdate(renderer, newStateSnapshot, (x, y) => newStateSnapshot);
            }
		    return isStale;
		}

		private InstrumentStateSnapshot CaptureStateSnapshot(IInstrumentRenderer renderer)
		{
			var newState = renderer.GetState();
			return new InstrumentStateSnapshot
			{
				HashCode = newState != null ? newState.GetHashCode() : 0,
				DateTime = DateTime.Now
			};
		}

		private class InstrumentStateSnapshot
		{
			public DateTime DateTime { get; set; }
			public int HashCode { get; set; }
			public static InstrumentStateSnapshot Default { get { return new InstrumentStateSnapshot { DateTime = DateTime.Now.Subtract(TimeSpan.FromDays(1)), HashCode = 0 }; } }
		}
	}
}
