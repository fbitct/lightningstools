using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
namespace F4TexSharedMemTester
{
    public partial class Form1 : Form
    {
        private F4TexSharedMem.Reader _reader = new F4TexSharedMem.Reader();
        private bool _closing = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _closing = true;
            if (_reader != null)
            {
                try
                {
                    _reader.Dispose();
                }
                catch (Exception)
                {
                }
            }

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            while (!_reader.IsDataAvailable)
            {
                Thread.Sleep(100);
                Application.DoEvents();
            }
            pictureBox1.Image = _reader.FullImage;
            pictureBox1.Width = pictureBox1.Image.Width;
            pictureBox1.Height = pictureBox1.Image.Height;
            this.ClientSize = new Size(pictureBox1.Image.Width,pictureBox1.Image.Height);
            while (!_closing)
            {
                pictureBox1.Refresh();
                Thread.Sleep(20);
                Application.DoEvents();
            }

        }

    }
}