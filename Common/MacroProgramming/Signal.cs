﻿using System;
using System.Drawing;
using System.Xml.Serialization;

namespace Common.MacroProgramming
{
    [Serializable]
    [XmlInclude(typeof (AnalogSignal))]
    [XmlInclude(typeof (DigitalSignal))]
    [XmlInclude(typeof (TextSignal))]
    public abstract class Signal
    {
        [NonSerialized] private object _publisherObject;
        [NonSerialized] private object _source;
        [NonSerialized] private object _subSource;
        [NonSerialized] private SignalGraph _signalGraph;

        /// <summary>
        ///   The source object which originally published the Signal
        /// </summary>
        [XmlIgnore]
        public object PublisherObject
        {
            get { return _publisherObject; }
            set { _publisherObject = value; }
        }

        /// <summary>
        ///   The source or device that this signal emanates from
        /// </summary>
        [XmlIgnore]
        public object Source
        {
            get { return _source; }
            set { _source = value; }
        }
        public void DrawGraph(Graphics graphics, Rectangle targetRectangle)
        {
            if (_signalGraph == null)
            {
                _signalGraph = new SignalGraph(this);
            }
            _signalGraph.Draw(graphics, targetRectangle);
        }
        /// <summary>
        ///   Descriptive, human-readable name of the source of the signal
        /// </summary>
        public string SourceFriendlyName { get; set; }

        /// <summary>
        ///   The address of the source of the signal (could be a device name, COM port, IP address, etc..)
        /// </summary>
        public string SourceAddress { get; set; }

        /// <summary>
        ///   The secondary source or device that this signal emanates from
        /// </summary>
        [XmlIgnore]
        public object SubSource
        {
            get { return _subSource; }
            set { _subSource = value; }
        }

        public string SubSourceFriendlyName { get; set; }
        public string SubSourceAddress { get; set; }
        public string FriendlyName { get; set; }
        public string Id { get; set; }
        public abstract string SignalType { get; }
        public string Category { get; set; }
        public string CollectionName { get; set; }
        public string SubcollectionName { get; set; }

        public int? Index { get; set; }
        public virtual bool HasListeners { get; }
        #region Object Overrides (ToString, GetHashCode, Equals)

        /// <summary>
        ///   Gets a string representation of this object.
        /// </summary>
        /// <returns>a String containing a textual representation of this object.</returns>
        public override string ToString()
        {
            return Serialization.Util.SerializeToXml(this, typeof (Signal));
        }

        /// <summary>
        ///   Gets an integer "hash" representation of this object, for use in hashtables.
        /// </summary>
        /// <returns>an integer containing a numeric hash of this object's variables.  When two objects are Equal, their hashes should be equal as well.</returns>
        public override int GetHashCode()
        {
            return Serialization.Util.ToRawBytes(this).GetHashCode();
        }

        /// <summary>
        ///   Compares this object to another one to determine if they are equal.  Equality for this type of object simply means that the other object must be of the same type and must be monitoring the same DirectIn device.
        /// </summary>
        /// <param name = "obj">An object to compare this object to</param>
        /// <returns>a boolean, set to true, if the this object is equal to the specified object, and set to false, if they are not equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            return GetHashCode() == obj.GetHashCode() && Serialization.Util.DeepEquals(this, obj);
        }

        #endregion
    }
}