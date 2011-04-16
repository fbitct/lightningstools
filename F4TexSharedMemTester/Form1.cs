using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using F4TexSharedMem;

namespace F4TexSharedMemTester
{
    public partial class Form1 : Form
    {
        private readonly Reader _reader = new Reader();
        private bool _closing;

        public Form1()
        {
            InitializeComponent();
        }

        private static void Form1_Load(object sender, EventArgs e)
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
            ClientSize = new Size(pictureBox1.Image.Width, pictureBox1.Image.Height);
            while (!_closing)
            {
                pictureBox1.Refresh();
                Thread.Sleep(20);
                Application.DoEvents();
            }
        }
    }
}