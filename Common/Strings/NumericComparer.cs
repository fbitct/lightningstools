#region Using statements
using System;
using System.Collections; // required for NumericComparer : IComparer only
using System.Windows.Forms;
using System.Collections.Generic;
#endregion

namespace Common.Strings
{
    /// <summary>
    /// Provides a Comparer that can properly sort numeric-containing strings.  
    /// The default sorting is "1,110, 112, 2, 20,..."; 
    /// this comparer provides for number-line sorting 
    /// (1,2, 10, 20, 110, 120, ...);
    /// </summary>
    public sealed class NumericComparer : IComparer, IComparer<string>
    {
        #region Constructors
        public NumericComparer()
        { }
        #endregion
        #region Public methods
        public int Compare(object x, object y)
        {
            if ((x is string) && (y is string))
            {
                return StringLogicalComparer.Compare((string)x, (string)y);
            }
            return -1;
        }
        
        #endregion

        #region IComparer<string> Members

        public int Compare(string x, string y)
        {
            return Compare((object)x, (object)y);
        }

        #endregion
    }//EOC
}