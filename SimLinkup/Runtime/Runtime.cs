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
            RunSetupScripts();
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
                var startTime = DateTime.UtcNow;
                UpdateSimSignals();
                Synchronize();
                if (_loopScripts != null && _loopScripts.Length > 0)
                {
                    RunLoopScripts();
                }
                Thread.Sleep(0);
                var endTime = DateTime.UtcNow;
                var loopDuration = endTime.Subtract(startTime);
                _loopDurationSignal.State = loopDuration.TotalMilliseconds;
                _loopFrequencySignal.State = 1000 / loopDuration.TotalMilliseconds;

            }
            IsRunning = false;
        }

        private void Synchronize()
        {
            if (ScriptingContext.HardwareSupportModules != null)
            {
                foreach (var hsm in ScriptingContext.HardwareSupportModules)
                {
                    try
                    {
                        hsm.SynchronizeAsync();
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.ToString(), e);
                    }
                }
            }
        }

        private void UpdateSimSignals()
        {
            if (ScriptingContext.SimSupportModules != null)
            {
                foreach (var ssm in ScriptingContext.SimSupportModules)
                {
                    ssm.Update();
                }
            }
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
            RunTeardownScripts();
            IsRunning = false;
        }

        private void RunSetupScripts()
        {
            if (_setupScripts != null && _setupScripts.Length > 0)
            {
                IsRunning = true;
                RunScripts(_setupScripts, false);
            }
        }

        private void RunLoopScripts()
        {
            if (_loopScripts != null && _loopScripts.Length > 0)
            {
                RunScripts(_loopScripts, true);
            }
        }

        private void RunTeardownScripts()
        {
            if (_teardownScripts != null && _teardownScripts.Length > 0)
            {
                RunScripts(_teardownScripts, false);
            }
        }

        private void RunScripts(Script[] scripts, bool checkIsRunning)
        {
            if (scripts == null || scripts.Length == 0) return;
            foreach (var script in scripts)
            {
                if (script != null)
                {
                    var scriptAssembly = script.Assembly;
                    if (scriptAssembly != null)
                    {
                        try
                        {
                            var invoker = scriptAssembly.GetStaticMethod();
                            if (!checkIsRunning || (checkIsRunning && IsRunning && _keepRunning))
                            {
                                invoker.Invoke(ScriptingContext);
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error(e.Message, e);
                        }
                    }
                }
            }
        }

        private void Initialize()
        {
            if (_initialized) return;
            ScriptingContext = new ScriptingContext
            {
                SimSupportModules = GetRegisteredSimSupportModules(),
                HardwareSupportModules = GetRegisteredHardwareSupportModules()
            };

            LoadScripts();
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
            var appDirectory = Util.ApplicationDirectory;
            var mappingFiles = new DirectoryInfo(
                Path.Combine(
                    Path.Combine(appDirectory, "Content"), "Mapping")
                ).GetFiles("*.mapping");
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
            var appDirectory = Util.ApplicationDirectory;

            //get a list of sim support modules that are currently registered
            var ssmRegistry =
                SimSupportModuleRegistry.Load(Path.Combine(appDirectory, "SimSupportModule.registry"));
            var modules = ssmRegistry.GetInstances();
            if (modules != null)
            {
                return modules.ToArray();
            }
            return null;
        }

        public static IHardwareSupportModule[] GetRegisteredHardwareSupportModules()
        {
            var appDirectory = Util.ApplicationDirectory;

            //get a list of hardware support modules that are currently registered
            var hsmRegistry =
                HardwareSupportModuleRegistry.Load(Path.Combine(appDirectory, "HardwareSupportModule.registry"));

            var modules = hsmRegistry.GetInstances();
            return modules != null ? modules.ToArray() : null;
        }

        private void LoadScripts()
        {
            var contentDirectory = Path.Combine(Util.ApplicationDirectory, "Content");
            var scriptsDirectory = Path.Combine(contentDirectory, "Scripts");
            var scriptsRegistryFileName = Path.Combine(scriptsDirectory, "Scripts.xml");

            var scriptRegistry = ScriptRegistry.Load(scriptsRegistryFileName);
            if (scriptRegistry.SetupScripts != null)
            {
                var setupScripts = new List<Script>();
                foreach (var script in scriptRegistry.SetupScripts)
                {
                    if (!string.IsNullOrEmpty(script.Src))
                    {
                        var loadedScript = Script.Load(Util.ApplicationDirectory, script.Src, script.Language);
                        setupScripts.Add(loadedScript);
                    }
                    else
                    {
                        setupScripts.Add(script);
                    }
                }
                _setupScripts = setupScripts.ToArray();
            }

            if (scriptRegistry.LoopScripts != null)
            {
                var loopScripts = new List<Script>();
                foreach (var script in scriptRegistry.LoopScripts)
                {
                    if (!string.IsNullOrEmpty(script.Src))
                    {
                        var loadedScript = Script.Load(Util.ApplicationDirectory, script.Src, script.Language);
                        loopScripts.Add(loadedScript);
                    }
                    else
                    {
                        loopScripts.Add(script);
                    }
                }
                _loopScripts = loopScripts.ToArray();
            }

            if (scriptRegistry.TeardownScripts != null)
            {
                var teardownScripts = new List<Script>();
                foreach (var script in scriptRegistry.TeardownScripts)
                {
                    if (!string.IsNullOrEmpty(script.Src))
                    {
                        var loadedScript = Script.Load(Util.ApplicationDirectory, script.Src, script.Language);
                        teardownScripts.Add(loadedScript);
                    }
                    else
                    {
                        teardownScripts.Add(script);
                    }
                }
                _teardownScripts = teardownScripts.ToArray();
            }
        }

        #region Instance variables

        private readonly List<Chainable> _passthroughs = new List<Chainable>();
        private bool _initialized;
        private bool _keepRunning;
        private Script[] _loopScripts;
        private Script[] _setupScripts;
        private Script[] _teardownScripts;
        private readonly List<SignalMapping> _mappings = new List<SignalMapping>();

        #endregion
    }
}