#region Using statements

using System.Collections;
using System.Windows.Forms;
using Common.Strings;
// required for TreeNodeNumericComparer : IComparer only

#endregion

namespace Common.UI
{
    public sealed class TreeNodeNumericComparer : IComparer
    {
        #region Constructors

        #endregion

        #region Public methods

        public int Compare(object x, object y)
        {
            if ((x is TreeNode) && (y is TreeNode))
            {
                return StringLogicalComparer.Compare(((TreeNode) x).Text, ((TreeNode) y).Text);
            }
            return -1;
        }

        #endregion
    }

//EOC
}