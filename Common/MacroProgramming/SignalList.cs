using System;
using System.Collections.Generic;
using System.Text;

namespace Common.MacroProgramming
{
    public class SignalList<T>:List<T> where T:Signal
    {
        public SignalList()
            : base()
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
        public bool Contains(string id)
        {
            foreach(var signal in this) 
            {
                if (signal.Id == id) return true;
            }
            return false;
        }
        public T this[string id]
        {
            get
            {
                foreach(var signal in this) 
                {
                    if (signal.Id == id) return signal;
                }
                return null;
            }
            set
            {
                if (this[id] != null)
                {
                    var curVal = this[id];
                    this.Remove(curVal);
                    curVal = value;
                    this.Add(curVal);

                }
            }
        }
        public List<string> GetDistinctSignalSourceNames()
        {
            List<string> toReturn = new List<string>();
            foreach (var signal in this)
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
            SignalList<T> toReturn = new SignalList<T>();
            foreach (var signal in this)
            {
                if (signal.SubSource == null) toReturn.Add(signal);
            }
            return toReturn;
        }
        public SignalList<T> GetSignalsFromSource(object source)
        {
            SignalList<T> toReturn = new SignalList<T>();
            foreach (var signal in this)
            {
                if (signal.Source == source || string.Equals(signal.SourceFriendlyName, source.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    toReturn.Add(signal);
                }
            }
            return toReturn;
        }
        public List<string> GetDistinctSignalCollectionNames()
        {
            List<string> toReturn = new List<string>();
            foreach (var signal in this)
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
            SignalList<T> toReturn = new SignalList<T>();
            foreach (var signal in this)
            {
                if (!string.IsNullOrEmpty(signal.CollectionName) && string.Equals(collectionName, signal.CollectionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    toReturn.Add(signal);
                }
            }
            return toReturn;
        }
        public List<string> GetUniqueSubSources()
        {
            List<string> toReturn = new List<string>();
            foreach (var signal in this)
            {
                if (!string.IsNullOrEmpty(signal.SubSourceFriendlyName) && !toReturn.Contains(signal.SubSourceFriendlyName))
                {
                    toReturn.Add(signal.SubSourceFriendlyName);
                }
            }
            return toReturn;
        }

        public SignalList<T> GetSignalsBySubSourceFriendlyName(string subSourceFriendlyName)
        {
            SignalList<T> toReturn = new SignalList<T>();
            foreach (var signal in this)
            {
                if (!string.IsNullOrEmpty(signal.SubSourceFriendlyName) && string.Equals(subSourceFriendlyName, signal.SubSourceFriendlyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    toReturn.Add(signal);
                }
            }
            return toReturn;
        }




    }
}
