using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Math
{
    public static class ExtensionMethods
    {
        public static string FormatDecimal(this float toConvert, int decimalPlaces)
        {
            var rounded = System.Math.Round(toConvert, decimalPlaces, MidpointRounding.AwayFromZero);
            var formatString = "{0:0." + new String('0', decimalPlaces) + "}";
            return String.Format(formatString, rounded); 
        }

    }
}
