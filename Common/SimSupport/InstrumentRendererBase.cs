using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;

namespace Common.SimSupport
{
    public abstract class InstrumentRendererBase:IInstrumentRenderer
    {

        #region IInstrumentRenderer Members
        public abstract void Render(Graphics g, Rectangle bounds);
        public InstrumentStateBase GetState()
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            InstrumentStateBase state = null;
            foreach (PropertyInfo prop in props)
            {
                if (prop.Name == "InstrumentState")
                {
                    state = (InstrumentStateBase)prop.GetGetMethod().Invoke(this, null);
                }
            }
            return state;
        }
        #endregion
    }
}
