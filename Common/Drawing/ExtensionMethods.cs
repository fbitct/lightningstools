using System;
using System.Collections;
using System.Collections.Generic;

namespace Common.Drawing
{
    public static class ExtensionMethods
    {
        public static IEnumerable<T> Convert<T>(this IEnumerable source)
        {
            foreach (dynamic current in source)
            {
                yield return (T)(current);
            }
        }
    }
}
