using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using log4net;


namespace Common
{
    public static class Util
    {
        private static ILog _log = LogManager.GetLogger(typeof(Common.Util));
        public static bool EnumTryParse<T>(string strType, out T result)
        {
            string strTypeFixed = strType.Replace(' ', '_');
            if (Enum.IsDefined(typeof(T), strTypeFixed))
            {
                result = (T)Enum.Parse(typeof(T), strTypeFixed, true);
                return true;
            }
            else
            {
                foreach (string value in Enum.GetNames(typeof(T)))
                {
                    if (value.Equals(strTypeFixed, StringComparison.OrdinalIgnoreCase))
                    {
                        result = (T)Enum.Parse(typeof(T), value);
                        return true;
                    }
                }
                result = default(T);
                return false;
            }
        }
        public static byte SetBit(byte bits, int index, bool newVal)
        {
            byte toReturn = bits;
            if (newVal)
            {
                toReturn |= (byte)((int)global::System.Math.Pow(2, index));
            }
            else
            {
                toReturn &= (byte)~(int)global::System.Math.Pow(2, index);
            }
            return toReturn;
        }

        public static void DisposeObject(object obj)
        {
            if (obj != null)
            {

                try
                {
                    IDisposable disposable = obj as IDisposable;
                    if (disposable != null) disposable.Dispose();
                }
                catch (Exception e)
                {
                    //_log.Debug(e.Message, e);
                }

                obj = null;
            }
        }
    }
}
