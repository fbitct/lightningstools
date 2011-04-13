using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MFDExtractor.Runtime
{
    internal class FormManager
    {
        /// <summary>
        /// Reference to the application's main form (for supplying to DirectInput)
        /// </summary>
        private Form _applicationForm = null;
        private volatile bool _windowSizingOrMoving = false;
        /// <summary>
        /// Gets/sets a reference to the application's main form (if there is one) -- required for DirectInput event notifications
        /// </summary>
        public Form ApplicationForm
        {
            get
            {
                return _applicationForm;
            }
            set
            {
                _applicationForm = value;
            }
        }
        

    }
}
