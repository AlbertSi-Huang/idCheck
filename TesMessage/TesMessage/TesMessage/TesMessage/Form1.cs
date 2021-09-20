using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace TesMessage
{
    public class MsgType
    {
        public const int USER = 0x0400;
        public const int WM_START = USER + 101;
        public const int WM_UPDATE = USER + 102;
        public const int WM_SERACH = USER + 103;
    }

    public partial class MSGForm : Form
    {
        public MSGForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageCls.SendMessage("TesMessage2.exe", MsgType.WM_START, this.textBox1.Text.Trim());
         
        }
        public void message()
        {
            Message msg = new Message();
            MessageCls.ReceiveMessage(ref msg);
        } 
        //接收消息方法
        protected override void WndProc(ref System.Windows.Forms.Message e)
        {
            string msg = MessageCls.ReceiveMessage(ref e);
            if (msg != null)
                this.label3.Text = msg;
            base.WndProc(ref e);
         
        }

        string _startPath = Directory.GetCurrentDirectory();
        ProcessStartInfo _startInfo = new ProcessStartInfo();
        Process _pro = new Process();
        private void MSGForm_Load(object sender, EventArgs e)
        {
            _startInfo.FileName = _startPath + @"\TesMessage2.exe";
            _pro.StartInfo = _startInfo;
            _pro.Start();

        }
    }
}
