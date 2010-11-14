using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using F16CPD.Mfd.Menus;
namespace F16CPD.Mfd
{
    public abstract class MfdManager
    {
        public Size ScreenBoundsPixels
        {
            get;
            set;
        }
        public MfdMenuPage[] MenuPages
        {
            get;
            set;
        }
        public MfdMenuPage ActiveMenuPage
        {
            get;
            set;
        }
        public MfdManager()
            : base()
        {
        }
        public MfdManager(Size screenBoundsPixels):this()
        {
            this.ScreenBoundsPixels = screenBoundsPixels;
        }
        public virtual void Render(Graphics g)
        {
            throw new NotImplementedException();
        }


    }
}
