using System;
using System.Collections.Generic;
using Common.HardwareSupport;
using Common.InputSupport.DirectInput;
using Common.MacroProgramming;
using log4net;

namespace SimLinkup.HardwareSupport.Powell
{
    public class PowellIP1310ALRHardwareSupportModule : HardwareSupportModuleBase, IDisposable
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof(PowellIP1310ALRHardwareSupportModule));
        private const int MAX_RWR_SYMBOLS_AS_INPUTS = 64;
        private const int MAX_RWR_SYMBOLS_AS_OUTPUTS = 31;
        #endregion

        #region Signals Handling

        #region Signal Creation

        private void CreateInputSignals(string deviceID, out AnalogSignal[] analogSignals,
            out DigitalSignal[] digitalSignals)
        {
            var analogSignalsToReturn = new List<AnalogSignal>();
            var digitalSignalsToReturn = new List<DigitalSignal>();

            {
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = "RWR Symbol Count";
                thisSignal.Id = "IP1310ALR__RWR_Symbol_Count";
                thisSignal.Index = 0;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                analogSignalsToReturn.Add(thisSignal);
            }


            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Symbol ID", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Symbol_ID[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                analogSignalsToReturn.Add(thisSignal);
            }
            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Bearing (degrees)", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Bearing_Degrees[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                analogSignalsToReturn.Add(thisSignal);
            }

            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new AnalogSignal();
                thisSignal.CollectionName = "Analog Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Lethality", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Lethality[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = 0;
                analogSignalsToReturn.Add(thisSignal);
            }

            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Missile Activity Flag", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Missile_Activity_Flag[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = false;
                digitalSignalsToReturn.Add(thisSignal);
            }
            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Missile Launch Flag", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Missile_Launch_Flag[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = false;
                digitalSignalsToReturn.Add(thisSignal);
            }
            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} Selected Flag", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_Selected_Flag[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = false;
                digitalSignalsToReturn.Add(thisSignal);
            }
            for (int i = 0; i < MAX_RWR_SYMBOLS_AS_INPUTS; i++)
            {
                var thisSignal = new DigitalSignal();
                thisSignal.CollectionName = "Digital Inputs";
                thisSignal.FriendlyName = string.Format("RWR Object #{0} New Detection Flag", i);
                thisSignal.Id = string.Format("IP1310ALR__RWR_Object_New_Detection_Flag[{0}]", i);
                thisSignal.Index = i;
                thisSignal.PublisherObject = this;
                thisSignal.Source = this;
                thisSignal.SourceFriendlyName = FriendlyName;
                thisSignal.SourceAddress = _deviceID;
                thisSignal.SubSource = null;
                thisSignal.SubSourceFriendlyName = null;
                thisSignal.SubSourceAddress = null;
                thisSignal.State = false;
                digitalSignalsToReturn.Add(thisSignal);
            }

            analogSignals = analogSignalsToReturn.ToArray();
            digitalSignals = digitalSignalsToReturn.ToArray();
        }

        #endregion

        #endregion

        #region Instance variables

        private readonly AnalogSignal[] _analogInputSignals;
        private readonly DigitalSignal[] _digitalInputSignals;
        private string _deviceID;
        private bool _isDisposed;

        #endregion

        #region Constructors

        private PowellIP1310ALRHardwareSupportModule(string deviceID)
        {
            _deviceID = deviceID;
            CreateInputSignals(deviceID, out _analogInputSignals, out _digitalInputSignals);
        }

        public override string FriendlyName
        {
            get { return string.Format("Powell IP-1310/ALR Azimuth Indicator (RWR) Driver: {0}", _deviceID); }
        }

        public static IHardwareSupportModule[] GetInstances()
        {
            var toReturn = new List<IHardwareSupportModule>();
            try
            {
                foreach (var device in EnumerateDevices())
                {
                    IHardwareSupportModule thisHsm = new PowellIP1310ALRHardwareSupportModule(device);
                    toReturn.Add(thisHsm);
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return toReturn.ToArray();
        }
        private static IEnumerable<string> EnumerateDevices()
        {
            return new[]{"RWR00"}; //TODO: is there a way to enumerate the devices?  or else drive this off configuration...
        }

        #endregion

        #region Virtual Method Implementations

        public override AnalogSignal[] AnalogInputs
        {
            get { return _analogInputSignals; }
        }

        public override DigitalSignal[] DigitalInputs
        {
            get { return _digitalInputSignals; }
        }

        public override AnalogSignal[] AnalogOutputs
        {
            get { return null; }
        }

        public override DigitalSignal[] DigitalOutputs
        {
            get { return null; }
        }

        #endregion

        #region Destructors

        /// <summary>
        ///     Public implementation of IDisposable.Dispose().  Cleans up
        ///     managed and unmanaged resources used by this
        ///     object before allowing garbage collection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Standard finalizer, which will call Dispose() if this object
        ///     is not manually disposed.  Ordinarily called only
        ///     by the garbage collector.
        /// </summary>
        ~PowellIP1310ALRHardwareSupportModule()
        {
            Dispose();
        }

        /// <summary>
        ///     Private implementation of Dispose()
        /// </summary>
        /// <param name="disposing">
        ///     flag to indicate if we should actually
        ///     perform disposal.  Distinguishes the private method signature
        ///     from the public signature.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Common.Util.DisposeObject(_deviceID); //disconnect 
                    _deviceID = null;
                }
            }
            _isDisposed = true;
        }

        #endregion
    }
}