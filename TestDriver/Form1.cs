using NAudio.Wave;
using NCorder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDriver
{
    public partial class Form1 : Form
    {
        NCorder.Recorder Recorder { get; set; }
        ChromeDriver Browser { get; set; }

        public Form1()
        {
            InitializeComponent();
            this.Browser = new ChromeDriver(9222);
            this.Recorder = new NCorder.Recorder();
        }

        protected override void OnClosed(EventArgs e)
        {
            Recorder.Stop();
            Browser.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = saveFileDialog1.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Recorder.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Recorder.Stop();
            NCorder.MP3Writer writer = new NCorder.MP3Writer();
            writer.WriteOutBuffer(Recorder.Buffer, textBox1.Text);
            // save out file
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Browser.Start();
            this.Browser.NavigateToUrl("http://www.youtube.com");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Browser.Stop();
        }
    }
}
