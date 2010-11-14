using System;
using System.Collections; // required for PhysicalControlInfoNumericComparer : IComparer only
using System.Windows.Forms;
using Common.InputSupport;
using Common.Strings;

namespace Common.UI
{
    /// <summary>
    /// Compares two PhysicalControlInfo objects by their Aliases (useful in editors)
    /// </summary>
    internal sealed class PhysicalControlInfoNumericComparer : IComparer
    {
        #region Constructors
        internal PhysicalControlInfoNumericComparer()
        { }
        #endregion
        #region Public methods
        public int Compare(object x, object y)
        {
            if ((x is PhysicalControlInfo) && (y is PhysicalControlInfo))
            {
                return StringLogicalComparer.Compare(((PhysicalControlInfo)x).Alias, ((PhysicalControlInfo)y).Alias);

            }
            return -1;
        }
        #endregion
    }//EOC
}