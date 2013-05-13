using System;
using System.Drawing;
using System.Linq;

namespace Common.SimSupport
{
    public interface IInstrumentRendererBase
    {
        void Render(Graphics g, Rectangle bounds);
        InstrumentStateBase GetState();
    }

    public abstract class InstrumentRendererBase : IInstrumentRenderer, IInstrumentRendererBase
    {
        public abstract void Render(Graphics g, Rectangle bounds);
        public InstrumentStateBase GetState()
        {
            var props = GetType().GetProperties();
            InstrumentStateBase state = null;
            foreach (var prop in props.Where(prop => prop.Name == "InstrumentState"))
            {
                state = (InstrumentStateBase) prop.GetGetMethod().Invoke(this, null);
            }
            return state;
        }

        protected virtual void Dispose(bool disposing) {}
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}