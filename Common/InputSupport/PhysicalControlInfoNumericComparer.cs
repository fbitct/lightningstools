using System.Collections;
using Common.Strings;

// required for PhysicalControlInfoNumericComparer : IComparer only

namespace Common.InputSupport
{
    /// <summary>
    ///   Compares two PhysicalControlInfo objects by their Aliases (useful in editors)
    /// </summary>
    internal sealed class PhysicalControlInfoNumericComparer : IComparer
    {
        #region Constructors

        #endregion

        #region Public methods

        public int Compare(object x, object y)
        {
            if ((x is PhysicalControlInfo) && (y is PhysicalControlInfo))
            {
                return StringLogicalComparer.Compare(((PhysicalControlInfo) x).Alias, ((PhysicalControlInfo) y).Alias);
            }
            return -1;
        }

        #endregion
    }

//EOC
}