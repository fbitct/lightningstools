using System;
using System.Collections.Generic;

namespace Common.MacroProgramming
{
    public class SignalList<T> : List<T> where T : Signal
    {
        public SignalList()
        {
        }

        public SignalList(int capacity)
            : base(capacity)
        {
        }

        public SignalList(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public T this[string id]
        {
            get
            {
                foreach (T signal in this)
                {
                    if (signal.Id == id) return signal;
                }
                return null;
            }
            set
            {
                if (this[id] != null)
                {
                    T curVal = this[id];
                    Remove(curVal);
                    curVal = value;
                    Add(curVal);
                }
            }
        }

        public bool Contains(string id)
        {
            foreach (T signal in this)
            {
                if (signal.Id == id) return true;
            }
            return false;
        }

        public List<string> GetDistinctSignalSourceNames()
        {
            var toReturn = new List<string>();
            foreach (T signal in this)
            {
                if (signal.SourceFriendlyName != null && !toReturn.Contains(signal.SourceFriendlyName))
                {
                    toReturn.Add(signal.SourceFriendlyName);
                }
            }
            return toReturn;
        }

        public SignalList<T> GetTopLevelSignals()
        {
            var toReturn = new SignalList<T>();
            foreach (T signal in this)
            {
                if (signal.SubSource == null) toReturn.Add(signal);
            }
            return toReturn;
        }

        public SignalList<T> GetSignalsFromSource(object source)
        {
            var toReturn = new SignalList<T>();
            foreach (T signal in this)
            {
                if (signal.Source == source ||
                    string.Equals(signal.SourceFriendlyName, source.ToString(),
                                  StringComparison.InvariantCultureIgnoreCase))
                {
                    toReturn.Add(signal);
                }
            }
            return toReturn;
        }

        public List<string> GetDistinctSignalCollectionNames()
        {
            var toReturn = new List<string>();
            foreach (T signal in this)
            {
                if (!string.IsNullOrEmpty(signal.CollectionName) && !toReturn.Contains(signal.CollectionName))
                {
                    toReturn.Add(signal.CollectionName);
                }
            }
            return toReturn;
        }

        public SignalList<T> GetSignalsByCollection(string collectionName)
        {
            var toReturn = new SignalList<T>();
            foreach (T signal in this)
            {
                if (!string.IsNullOrEmpty(signal.CollectionName) &&
                    string.Equals(collectionName, signal.CollectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    toReturn.Add(signal);
                }
            }
            return toReturn;
        }

        public List<string> GetUniqueSubSources()
        {
            var toReturn = new List<string>();
            foreach (T signal in this)
            {
                if (!string.IsNullOrEmpty(signal.SubSourceFriendlyName) &&
                    !toReturn.Contains(signal.SubSourceFriendlyName))
                {
                    toReturn.Add(signal.SubSourceFriendlyName);
                }
            }
            return toReturn;
        }

        public SignalList<T> GetSignalsBySubSourceFriendlyName(string subSourceFriendlyName)
        {
            var toReturn = new SignalList<T>();
            foreach (T signal in this)
            {
                if (!string.IsNullOrEmpty(signal.SubSourceFriendlyName) &&
                    string.Equals(subSourceFriendlyName, signal.SubSourceFriendlyName,
                                  StringComparison.InvariantCultureIgnoreCase))
                {
                    toReturn.Add(signal);
                }
            }
            return toReturn;
        }
    }
}