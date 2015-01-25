using System;
using System.Linq;
using log4net;
using System.Collections.Generic;

namespace Common
{
    public static class Util
    {
        private static ILog _log = LogManager.GetLogger(typeof (Util));
        private static Dictionary<Type, Dictionary<string,object>> _cachedTypes = new Dictionary<Type,Dictionary<string,object>>();
        public static bool EnumTryParse<T>(string strType, out T result)
        {
            

            if (_cachedTypes.ContainsKey(typeof(T)))
            {
                if (_cachedTypes[typeof(T)].ContainsKey(strType))
                {
                    result = (T)_cachedTypes[typeof(T)][strType];
                    return true;
                }
            }
            else
            {
                _cachedTypes[typeof(T)] = new Dictionary<string, object>();
            }

            var strTypeFixed = strType.Replace(' ', '_');
            if (Enum.IsDefined(typeof (T), strTypeFixed))
            {
                result = (T) Enum.Parse(typeof (T), strTypeFixed, true);
                _cachedTypes[typeof(T)][strType] = result;
                return true;
            }
            foreach (var value in
                Enum.GetNames(typeof (T)).Where(value => value.Equals(strTypeFixed, StringComparison.OrdinalIgnoreCase)))
            {
                result = (T) Enum.Parse(typeof (T), value);
                _cachedTypes[typeof(T)][strType] = result;
                return true;
            }
            result = default(T);
            return false;
        }

        public static byte SetBit(byte bits, int index, bool newVal)
        {
            var toReturn = bits;
            if (newVal)
            {
                toReturn |= (byte) ((int) System.Math.Pow(2, index));
            }
            else
            {
                toReturn &= (byte) ~(int) System.Math.Pow(2, index);
            }
            return toReturn;
        }

        public static void DisposeObject(object obj)
        {
            if (obj == null) return;
            try
            {
                var disposable = obj as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
            catch
            {
            }
        }
    }
}