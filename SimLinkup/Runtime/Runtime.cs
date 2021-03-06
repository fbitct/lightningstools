﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Common.HardwareSupport;
using Common.MacroProgramming;
using Common.SimSupport;
using log4net;
using SimLinkup.Scripting;
using SimLinkup.Signals;
using System.Threading.Tasks;

namespace SimLinkup.Runtime
{
    public class Runtime
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (Runtime));
        private AnalogSignal _loopDurationSignal;
        private AnalogSignal _loopFrequencySignal;
        public Runtime()
        {
            Initialize();
        }
        public bool IsRunning { get; private set; }
        public ScriptingContext ScriptingContext { get; private set; }

        public IEnumerable<SignalMapping> Mappings
        {
            get { return _mappings; }
        }

        public void Start()
        {
            Task.Run(() => MainLoop());
        }
        private void AddPerformanceMonitoringSignals()
        {
            _loopDurationSignal =
                new AnalogSignal
                {
                    Category = "Outputs",
                    CollectionName = "Analog Outputs",
                    SubcollectionName = "Performance Metrics",
                    FriendlyName = "Loop Duration (ms)",
                    Id = "SIMLINKUP__PERFORMANCE__LOOP_DURATION",
                    Index = 0,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = "SimLinkup",
                    IsAngle = false,
                    IsPercentage = false,
                    MinValue = 0,
                    MaxValue = 250
                };
            ScriptingContext[_loopDurationSignal.Id]=_loopDurationSignal;


            _loopFrequencySignal=
                new AnalogSignal
                {
                    Category = "Outputs",
                    CollectionName = "Analog Outputs",
                    SubcollectionName = "Performance Metrics",
                    FriendlyName = "Loop Frequency (Hz)",
                    Id = "SIMLINKUP__PERFORMANCE__LOOP_FREQUENCY",
                    Index = 0,
                    PublisherObject = this,
                    Source = this,
                    SourceFriendlyName = "SimLinkup",
                    IsAngle = false,
                    IsPercentage = false,
                    MinValue = 0,
                    MaxValue = 300
                };
            ScriptingContext[_loopFrequencySignal.Id] = _loopFrequencySignal;
        }
        private void MainLoop()
        {
            _keepRunning = true;
            IsRunning = true;
            while (_keepRunning)
            {
                ExecuteOneLoopIteration();
            }
            IsRunning = false;
        }

        private void ExecuteOneLoopIteration()
        {
            var startTime = DateTime.UtcNow;
            UpdateSimSignals();
            Synchronize();
            var elapsed = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;
            var toSleep = 2 - (int)elapsed;
            if (toSleep < 0) toSleep = 1;
            Thread.Sleep(toSleep);
            var endTime = DateTime.UtcNow;
            var loopDuration = endTime.Subtract(startTime).TotalMilliseconds;
            if (loopDuration <= 0) loopDuration = 1; 
            _loopDurationSignal.State = loopDuration;
            _loopFrequencySignal.State = 1000 / loopDuration;
        }

        private void Synchronize()
        {
            if (ScriptingContext.HardwareSupportModules == null) return;
            ScriptingContext.HardwareSupportModules.ToList().ForEach(hsm => hsm.Synchronize());
        }

        private void UpdateSimSignals()
        {
            if (ScriptingContext.SimSupportModules == null) return;
            ScriptingContext.SimSupportModules.ToList().ForEach(ssm => ssm.Update());
        }

        public void Stop()
        {
            _keepRunning = false;
            var startWaitingTime = DateTime.UtcNow;
            while (IsRunning)
            {
                Thread.Sleep(5);
                var currentTime = DateTime.UtcNow;
                var elapsed = currentTime.Subtract(startWaitingTime);
                if (elapsed.TotalSeconds > 5)
                {
                    IsRunning = false;
                }
            }
            IsRunning = false;
        }
        

        private void Initialize()
        {
            if (_initialized) return;
            ScriptingContext = new ScriptingContext
            {
                SimSupportModules = GetRegisteredSimSupportModules(),
                HardwareSupportModules = GetRegisteredHardwareSupportModules()
            };

            AddPerformanceMonitoringSignals();
            InitializeMappings();
            _initialized = true;
        }
        private Signal ResolveSignal(Signal signalToResolve)
        {
            if (signalToResolve == null) return null;
            return
                ScriptingContext.AllSignals.FirstOrDefault(
                    signal =>
                        signal.Id != null && signalToResolve.Id != null &&
                        signal.Id.Trim().Equals(signalToResolve.Id.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private void InitializeMappings()
        {
            var mappingFiles = new DirectoryInfo(Util.CurrentMappingProfileDirectory).GetFiles("*.mapping");
            foreach (var mappingFile in mappingFiles)
            {
                var profileToLoad = mappingFile.FullName;
                if (!string.IsNullOrEmpty(profileToLoad) && !string.IsNullOrEmpty(profileToLoad.Trim()))
                {
                    var profile = MappingProfile.Load(profileToLoad);
                    foreach (var mapping in profile.SignalMappings)
                    {
                        var origSource = mapping.Source;
                        var origDestination = mapping.Destination;
                        mapping.Source = ResolveSignal(origSource);
                        mapping.Destination = ResolveSignal(origDestination);
                        if (mapping.Source == null || mapping.Destination == null)
                        {
                            _log.Warn(
                                string.Format(
                                    "A mapping defined in file {0} had an unresolvable source or destination signal.",
                                    profileToLoad));
                            continue;
                        }
                        _mappings.Add(mapping);

                        if (mapping.Source is AnalogSignal)
                        {
                            var mappingSource = mapping.Source as AnalogSignal;
                            var mappingDestination = mapping.Destination as AnalogSignal;

                            var passthru = new AnalogPassthrough {In = mappingSource, Out = mappingDestination};
                            passthru.Refresh();
                            _passthroughs.Add(passthru);
                        }
                        else if (mapping.Source is DigitalSignal)
                        {
                            var mappingSource = mapping.Source as DigitalSignal;
                            var mappingDestination = (DigitalSignal) mapping.Destination;
                            var passthru = new DigitalPassthrough {In = mappingSource, Out = mappingDestination};
                            passthru.Refresh();
                            _passthroughs.Add(passthru);
                        }
                        else if (mapping.Source is TextSignal)
                        {
                            var mappingSource = mapping.Source as TextSignal;
                            var mappingDestination = (TextSignal) mapping.Destination;
                            var passthru = new TextPassthrough {In = mappingSource, Out = mappingDestination};
                            passthru.Refresh();
                            _passthroughs.Add(passthru);
                        }
                    }
                }
            }
        }

        public static SimSupportModule[] GetRegisteredSimSupportModules()
        {
            //get a list of sim support modules that are currently registered
            var ssmRegistry =
                SimSupportModuleRegistry.Load(Path.Combine(Util.CurrentMappingProfileDirectory, "SimSupportModule.registry"));
            var modules = ssmRegistry.GetInstances();
            if (modules != null)
            {
                return modules.ToArray();
            }
            return null;
        }

        public static IHardwareSupportModule[] GetRegisteredHardwareSupportModules()
        {
            //get a list of hardware support modules that are currently registered
            var hsmRegistry =
                HardwareSupportModuleRegistry.Load(Path.Combine(Util.CurrentMappingProfileDirectory, "HardwareSupportModule.registry"));

            var modules = hsmRegistry.GetInstances();
            return modules != null ? modules.ToArray() : null;
        }

        #region Instance variables

        private readonly List<Chainable> _passthroughs = new List<Chainable>();
        private bool _initialized;
        private bool _keepRunning;
        private readonly List<SignalMapping> _mappings = new List<SignalMapping>();

        #endregion
    }
}