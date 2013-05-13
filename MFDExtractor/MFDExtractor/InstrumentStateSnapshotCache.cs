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
			const int staleDataTimeout = 500;
			var oldStateSnapshot = _instrumentStates.ContainsKey(renderer) ? _instrumentStates[renderer] : InstrumentStateSnapshot.Default;
			var newStateSnapshot = CaptureStateSnapshot(renderer);
			var hashesAreDifferent = (oldStateSnapshot.HashCode != newStateSnapshot.HashCode);
			var timeSinceHashChanged = (int)Math.Floor(DateTime.Now.Subtract(oldStateSnapshot.DateTime).TotalMilliseconds);
			return (hashesAreDifferent ||timeSinceHashChanged > staleDataTimeout);
		}

		private InstrumentStateSnapshot CaptureStateSnapshot(IInstrumentRenderer renderer)
		{
			var newState = renderer.GetState();
			var newStateSnapshot = new InstrumentStateSnapshot
			{
				HashCode = newState != null ? newState.GetHashCode() : 0,
				DateTime = DateTime.Now
			};
			return _instrumentStates.AddOrUpdate(renderer, newStateSnapshot, (x, y) => newStateSnapshot);
		}

		private class InstrumentStateSnapshot
		{
			public DateTime DateTime { get; set; }
			public int HashCode { get; set; }
			public static InstrumentStateSnapshot Default { get { return new InstrumentStateSnapshot { DateTime = DateTime.MinValue, HashCode = 0 }; } }
		}
	}
}
