using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using F16CPD.UI.Util;

namespace F16CPD.UI.Forms
{
    public class MfdForm:DraggableForm
    {
        internal ResizeHelper _resizeHelper;
        public MfdForm()
            : base()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ContainerControl, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            
            if (!base.DesignMode)
            {
                _resizeHelper = new ResizeHelper(this);
            }
        }
    }
}
