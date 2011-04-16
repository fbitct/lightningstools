using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Common.HardwareSupport;
using Common.MacroProgramming;
using Common.SimSupport;
using CSScriptLibrary;
using log4net;
using SimLinkup.Scripting;
using SimLinkup.Signals;

namespace SimLinkup.Runtime
{
    public class Runtime
    {
        #region Class variables

        private static readonly ILog _log = LogManager.GetLogger(typeof (Runtime));

        #endregion

        #region Instance variables

        private readonly List<Chainable> _passthroughs = new List<Chainable>();
        private bool _initialized;
        private bool _isRunning;
        private bool _keepRunning;
        private Script[] _loopScripts;
        private ScriptingContext _scriptingContext;
        private Script[] _setupScripts;
        private Script[] _teardownScripts;

        #endregion

        public Runtime()
        {
            Initialize();
        }

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public ScriptingContext ScriptingContext
        {
            get { return _scriptingContext; }
        }

        public void Start()
        {
            RunSetupScripts();
            MainLoop();
        }

        private void MainLoop()
        {
            const int MIN_LOOP_TIME = 1; //milliseconds
            _keepRunning = true;
            _isRunning = true;
            var rnd = new Random();
            while (_keepRunning)
            {
                DateTime startTime = DateTime.Now;
                UpdateSimSignals();
                Application.DoEvents();
                Synchronize();
                Application.DoEvents();
                if (_loopScripts != null && _loopScripts.Length > 0)
                {
                    RunLoopScripts();
                }
                Application.DoEvents();
                DateTime endTime = DateTime.Now;
                TimeSpan timedelta = endTime.Subtract(startTime);
                if (timedelta.Milliseconds < MIN_LOOP_TIME)
                {
                    Thread.Sleep(MIN_LOOP_TIME);
                }
            }
            _isRunning = false;
        }

        private void Synchronize()
        {
            if (_scriptingContext.HardwareSupportModules != null)
            {
                foreach (IHardwareSupportModule hsm in _scriptingContext.HardwareSupportModules)
                {
                    try
                    {
                        hsm.Synchronize();
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
            if (_scriptingContext.SimSupportModules != null)
            {
                foreach (SimSupportModule ssm in _scriptingContext.SimSupportModules)
                {
                    if (ssm.IsSimRunning)
                    {
                        ssm.Update();
                    }
                }
            }
        }

        public void Stop()
        {
            _keepRunning = false;
            DateTime startWaitingTime = DateTime.Now;
            while (_isRunning)
            {
                Thread.Sleep(5);
                DateTime currentTime = DateTime.Now;
                TimeSpan elapsed = currentTime.Subtract(startWaitingTime);
                if (elapsed.TotalSeconds > 5)
                {
                    _isRunning = false;
                }
            }
            RunTeardownScripts();
            _isRunning = false;
        }

        private void RunSetupScripts()
        {
            if (_setupScripts != null && _setupScripts.Length > 0)
            {
                _isRunning = true;
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
            foreach (Script script in scripts)
            {
                if (script != null)
                {
                    Assembly scriptAssembly = script.Assembly;
                    if (scriptAssembly != null)
                    {
                        try
                        {
                            MethodDelegate invoker = scriptAssembly.GetStaticMethod();
                            if (!checkIsRunning || (checkIsRunning && _isRunning && _keepRunning))
                            {
                                invoker.Invoke(_scriptingContext);
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
                Application.DoEvents();
            }
        }

        private void Initialize()
        {
            if (_initialized) return;
            _scriptingContext = new ScriptingContext();

            //GetRegisteredHardwareSupportModules();

            _scriptingContext.SimSupportModules = GetRegisteredSimSupportModules();
            _scriptingContext.HardwareSupportModules = GetRegisteredHardwareSupportModules();

            LoadScripts();
            InitializeMappings();
            _initialized = true;
        }

        private Signal ResolveSignal(Signal signalToResolve)
        {
            if (signalToResolve == null) return null;
            foreach (Signal signal in ScriptingContext.AllSignals)
            {
                if (
                    signal.Id != null
                    &&
                    signalToResolve.Id != null
                    &&
                    signal.Id.Trim().Equals
                        (
                            signalToResolve.Id.Trim(),
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    )
                {
                    return signal;
                }
            }
            return null;
        }

        private void InitializeMappings()
        {
            string appDirectory = Util.ApplicationDirectory;
            FileInfo[] mappingFiles = new DirectoryInfo(
                Path.Combine(
                    Path.Combine(appDirectory, "Content"), "Mapping")
                ).GetFiles("*.mapping");
            foreach (FileInfo mappingFile in mappingFiles)
            {
                string profileToLoad = mappingFile.FullName;
                if (!string.IsNullOrEmpty(profileToLoad) && !string.IsNullOrEmpty(profileToLoad.Trim()))
                {
                    MappingProfile profile = MappingProfile.Load(profileToLoad);
                    foreach (SignalMapping mapping in profile.SignalMappings)
                    {
                        mapping.Source = ResolveSignal(mapping.Source);
                        mapping.Destination = ResolveSignal(mapping.Destination);
                        if (mapping.Source == null || mapping.Destination == null)
                        {
                            _log.Warn(
                                string.Format(
                                    "A mapping defined in file {0} had an unresolvable source or destination signal.",
                                    profileToLoad));
                            continue;
                        }
                        if (mapping.Source is AnalogSignal)
                        {
                            var mappingSource = mapping.Source as AnalogSignal;
                            var mappingDestination = mapping.Destination as AnalogSignal;

                            var passthru = new AnalogPassthrough();
                            passthru.In = mappingSource;
                            passthru.Out = mappingDestination;
                            passthru.Refresh();
                            _passthroughs.Add(passthru);
                        }
                        else if (mapping.Source is DigitalSignal)
                        {
                            var mappingSource = mapping.Source as DigitalSignal;
                            var mappingDestination = (DigitalSignal) mapping.Destination;
                            var passthru = new DigitalPassthrough();
                            passthru.In = mappingSource;
                            passthru.Out = mappingDestination;
                            passthru.Refresh();
                            _passthroughs.Add(passthru);
                        }
                        else if (mapping.Source is TextSignal)
                        {
                            var mappingSource = mapping.Source as TextSignal;
                            var mappingDestination = (TextSignal) mapping.Destination;
                            var passthru = new TextPassthrough();
                            passthru.In = mappingSource;
                            passthru.Out = mappingDestination;
                            passthru.Refresh();
                            _passthroughs.Add(passthru);
                        }
                    }
                }
            }
        }

        public static SimSupportModule[] GetRegisteredSimSupportModules()
        {
            string appDirectory = Util.ApplicationDirectory;

            //get a list of sim support modules that are currently registered
            SimSupportModuleRegistry ssmRegistry =
                SimSupportModuleRegistry.Load(Path.Combine(appDirectory, "SimSupportModule.registry"));
            List<SimSupportModule> modules = ssmRegistry.GetInstances();
            if (modules != null)
            {
#if TESTMODE
                foreach (var module in modules)
                {
                    ((Common.SimSupport.SimSupportModule)module).TestMode = true;
                }
#else
#endif
                return modules.ToArray();
            }
            else
            {
                return null;
            }
        }

        public static IHardwareSupportModule[] GetRegisteredHardwareSupportModules()
        {
            string appDirectory = Util.ApplicationDirectory;

            //get a list of hardware support modules that are currently registered
            HardwareSupportModuleRegistry hsmRegistry =
                HardwareSupportModuleRegistry.Load(Path.Combine(appDirectory, "HardwareSupportModule.registry"));

            List<IHardwareSupportModule> modules = hsmRegistry.GetInstances();
            if (modules != null)
            {
                return modules.ToArray();
            }
            else
            {
                return null;
            }
        }

        private void LoadScripts()
        {
            string contentDirectory = Path.Combine(Util.ApplicationDirectory, "Content");
            string scriptsDirectory = Path.Combine(contentDirectory, "Scripts");
            string scriptsRegistryFileName = Path.Combine(scriptsDirectory, "Scripts.xml");

            ScriptRegistry scriptRegistry = ScriptRegistry.Load(scriptsRegistryFileName);
            if (scriptRegistry.SetupScripts != null)
            {
                var setupScripts = new List<Script>();
                foreach (Script script in scriptRegistry.SetupScripts)
                {
                    if (!string.IsNullOrEmpty(script.Src))
                    {
                        Script loadedScript = Script.Load(Util.ApplicationDirectory, script.Src, script.Language);
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
                foreach (Script script in scriptRegistry.LoopScripts)
                {
                    if (!string.IsNullOrEmpty(script.Src))
                    {
                        Script loadedScript = Script.Load(Util.ApplicationDirectory, script.Src, script.Language);
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
                foreach (Script script in scriptRegistry.TeardownScripts)
                {
                    if (!string.IsNullOrEmpty(script.Src))
                    {
                        Script loadedScript = Script.Load(Util.ApplicationDirectory, script.Src, script.Language);
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
    }
}