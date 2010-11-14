using System;
using System.Collections.Generic;
using System.Text;

namespace F16CPD.Mfd.Controls
{
    public class ToggleSwitchPositionMfdInputControl:MfdInputControl
    {
        public ToggleSwitchPositionMfdInputControl():base()
        {
        }
        public ToggleSwitchPositionMfdInputControl(string positionName, ToggleSwitchMfdInputControl parent)
        {
            this.PositionName = positionName;
            this.Parent = parent;
        }
        public String PositionName
        {
            get;
            set;
        }
        public ToggleSwitchMfdInputControl Parent
        {
            get;
            set;
        }
        public void Activate()
        {
            ToggleSwitchMfdInputControl parent = this.Parent;
            if (parent != null)
            {
                parent.CurrentPosition = this;
            }
        }
    }
}
