using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Common.HardwareSupport;
using Common.SimSupport;
using SimLinkup.Scripting;
using System.Threading;
using log4net;
using SimLinkup.Signals;
using Common.MacroProgramming;

namespace SimLinkup.Runtime
{
    public class Runtime
    {
        #region Class variables
        private static ILog _log = LogManager.GetLogger(typeof(Runtime));
        #endregion
        #region Instance variables
        private bool _initialized = false;
        private Script[] _setupScripts= null;
        private Script[] _loopScripts = null;
        private Script[] _teardownScripts = null;
        private ScriptingContext _scriptingContext = null;
        private bool _isRunning = false;
        private bool _keepRunning = false;
        private List<Chainable> _passthroughs = new List<Chainable>();
        #endregion

        public Runtime()
            : base()
        {
            Initialize();
        }
        public void Start()
        {
            RunSetupScripts();
            MainLoop();

        }
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }
        public ScriptingContext ScriptingContext
        {
            get
            {
                return _scriptingContext;
            }
        }
        private void MainLoop()
        {
            _keepRunning = true;
            _isRunning = true;
                while (_keepRunning)
                {
                    UpdateSimSignals();
                    Application.DoEvents();
                    if (_loopScripts != null && _loopScripts.Length > 0)
                    {
                        RunLoopScripts();
                    }
                }
                Thread.Sleep(50);
            _isRunning = false;
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
                RunScripts(_teardownScripts,false);
            }
        }
        private void RunScripts(Script[] scripts, bool checkIsRunning)
        {
            if (scripts == null || scripts.Length ==0) return;
            foreach (Script script in scripts)
            {
                if (script != null)
                {
                    Assembly scriptAssembly = script.Assembly;
                    if (scriptAssembly != null)
                    {
                        try
                        {
                            var invoker = scriptAssembly.GetStaticMethod();
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

            GetRegisteredHardwareSupportModules();

            _scriptingContext.SimSupportModules = GetRegisteredSimSupportModules();
            _scriptingContext.HardwareSupportModules = GetRegisteredHardwareSupportModules(); ;

            LoadScripts();
            InitializeMappings();
            _initialized = true;            
        }
        private Signal ResolveSignal(Signal signalToResolve)
        {
            if (signalToResolve == null) return null;
            foreach (var signal in this.ScriptingContext.AllSignals)
            {
                if (signal.Id == signalToResolve.Id)
                {
                    return signal;
                }
            }
            return null;
        }
        private void InitializeMappings()
        {
            string appDirectory = Util.ApplicationDirectory;
            FileInfo[] mappingFiles= new DirectoryInfo(
                        Path.Combine(
                            Path.Combine(appDirectory, "Content"), "Mapping")
                    ).GetFiles("*.mapping");
            foreach (var mappingFile in mappingFiles)
            {
                string profileToLoad = mappingFile.FullName;
                if (!string.IsNullOrEmpty(profileToLoad) && !string.IsNullOrEmpty(profileToLoad.Trim()))
                {
                    MappingProfile profile = MappingProfile.Load(profileToLoad);
                    foreach (var mapping in profile.SignalMappings)
                    {
                        mapping.Source = ResolveSignal(mapping.Source);
                        mapping.Destination = ResolveSignal(mapping.Destination);
                        if (mapping.Source == null || mapping.Destination == null)
                        {
                            continue;
                        }
                        if (mapping.Source is AnalogSignal)
                        {
                            AnalogSignal mappingSource = mapping.Source as AnalogSignal;
                            AnalogSignal mappingDestination = mapping.Destination as AnalogSignal;

                            Common.MacroProgramming.AnalogPassthrough passthru = new AnalogPassthrough();
                            passthru.In = mappingSource;
                            passthru.Out = (AnalogSignal)mappingDestination;
                            _passthroughs.Add(passthru);
                        }
                        else if (mapping.Source is DigitalSignal)
                        {
                            DigitalSignal mappingSource = mapping.Source as DigitalSignal;
                            Common.MacroProgramming.DigitalPassthrough passthru = new DigitalPassthrough();
                            passthru.In = mappingSource;
                            passthru.Out = (DigitalSignal)mapping.Destination;
                            _passthroughs.Add(passthru);
                        }
                        else if (mapping.Source is TextSignal)
                        {
                            TextSignal mappingSource = mapping.Source as TextSignal;
                            Common.MacroProgramming.TextPassthrough passthru = new TextPassthrough();
                            passthru.In = mappingSource;
                            passthru.Out = (TextSignal)mapping.Destination;
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
            SimSupportModuleRegistry ssmRegistry = SimSupportModuleRegistry.Load(Path.Combine(appDirectory, "SimSupportModule.registry"));
            var modules = ssmRegistry.GetInstances();
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
            HardwareSupportModuleRegistry hsmRegistry = HardwareSupportModuleRegistry.Load(Path.Combine(appDirectory, "HardwareSupportModule.registry"));

            var modules = hsmRegistry.GetInstances();
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
                List<Script> setupScripts = new List<Script>();
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
                List<Script> loopScripts = new List<Script>();
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
                List<Script> teardownScripts = new List<Script>();
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
