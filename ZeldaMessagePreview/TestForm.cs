using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZeldaMessage
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();

            byte[] a = { 0x13, 0x12, 0x17, 0x20, 0x20, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x77, 0x6F, 0x72, 0x6C, 0x64, 0x21, 0x01, 0x50, 0x72, 0x65, 0x76, 0x69, 0x05, 0x42, 0x65, 0x77, 0x20, 0x74, 0x65, 0x73, 0x74, 0x69, 0x6E, 0x67, 0x21, 0x01, 0x4E, 0x6F, 0x74, 0x20, 0x66, 0x75, 0x6E, 0x2E, 0x2E, 0x2E, 0x2};

            MessagePreview msgP1 = new MessagePreview(Data.BoxType.Black, test);
            MessagePreview msgP2 = new MessagePreview(Data.BoxType.Wooden, a);
            MessagePreview msgP3 = new MessagePreview(Data.BoxType.Blue, a);
            MessagePreview msgP4 = new MessagePreview(Data.BoxType.Ocarina, a);
            MessagePreview msgP5 = new MessagePreview(Data.BoxType.None_Black, a);
            MessagePreview msgP6 = new MessagePreview(Data.BoxType.None_White, a);

            pictureBox1.BackgroundImage = msgP1.GetPreview();
            pictureBox2.BackgroundImage = msgP2.GetPreview();
            pictureBox3.BackgroundImage = msgP3.GetPreview();
            pictureBox4.BackgroundImage = msgP4.GetPreview();
            pictureBox5.BackgroundImage = msgP5.GetPreview();
            pictureBox6.BackgroundImage = msgP6.GetPreview();
        }
    }
}
