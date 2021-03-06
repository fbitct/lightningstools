﻿using Common.Reflection;
using Common.SimSupport;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SimLinkup
{
    internal class SimSupportModuleLoader
    {
        private ILog _log = LogManager.GetLogger(typeof(SimSupportModuleLoader));
        public IEnumerable<SimSupportModule> LoadSimSupportModules(string pathToScan, bool recursiveScan)
        {
            var toReturn = new List<SimSupportModule>();
            try
            {
                var moduleTypes = new AssemblyTypeScanner<SimSupportModule>().FindMatchingTypesInAssemblies(pathToScan, recursiveScan);

                foreach (var moduleType in moduleTypes.Distinct())
                {
                    try
                    {
                        var module = Activator.CreateInstance(moduleType) as SimSupportModule;
                        toReturn.Add(module);
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
