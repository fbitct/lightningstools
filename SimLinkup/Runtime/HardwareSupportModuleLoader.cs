using Common.HardwareSupport;
using Common.Reflection;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SimLinkup.Runtime
{
    internal class HardwareSupportModuleLoader 
    {
        private ILog _log = LogManager.GetLogger(typeof(HardwareSupportModuleLoader));
        public IEnumerable<IHardwareSupportModule> LoadHardwareSupportModules(string pathToScan, bool recursiveScan)
        {
            var toReturn = new List<IHardwareSupportModule>();
            try
            {
                var moduleTypes = new AssemblyTypeScanner<IHardwareSupportModule>().FindMatchingTypesInAssemblies(pathToScan, recursiveScan);

                foreach (var moduleType in moduleTypes.Distinct())
                {
                    try
                    {
                        var instances = moduleType.GetMethod("GetInstances").Invoke(obj: null, parameters: null) as IHardwareSupportModule[];
                        toReturn.AddRange(instances);
                    }
                    catch (Exception e)
                    {
                        _log.Error(e.Message, e);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message, e);
            }
            return toReturn;
            
        }

    }
}
