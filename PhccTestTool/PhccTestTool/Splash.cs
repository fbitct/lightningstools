using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PhccTestTool
{
    public partial class Splash : Form
    {
        public Splash()
        {
            InitializeComponent();
        }

        private void Splash_Load(object sender, EventArgs e)
        {
            Bitmap bmp = (Bitmap)Properties.Resources.testTool;
            //bmp.MakeTransparent(Color.White);
            this.Size = bmp.Size;
            this.BackgroundImage = bmp;

        }

    }
}
