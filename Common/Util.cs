using System;
using System.Linq;
using log4net;
using System.Collections.Generic;

namespace Common
{
    public static class Util
    {
        private static ILog _log = LogManager.GetLogger(typeof (Util));
        private static Dictionary<int, object> _cachedEnumValues=new Dictionary<int, object>();
        public static bool EnumTryParse<T>(string value, out T result) where T: struct
        {
            var key = typeof(T).GetHashCode() << 32 | value.GetHashCode();
            object parsedValue = null;
            var wasCached= _cachedEnumValues.TryGetValue(key, out parsedValue);
            if (wasCached)
            {
                result = (T)parsedValue;
                return wasCached;
            }
            
            var couldParse= Enum.TryParse(value, out result);
            if (couldParse)
            {
                _cachedEnumValues[key] = result;
            }
            return couldParse;
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