using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TesMessage2
{
    public class MsgType
    {
        public const int USER = 0x0400;
        public const int WM_START = USER + 101;
        public const int WM_UPDATE = USER + 102;
        public const int WM_SERACH = USER + 103;
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //发送
        private void button1_Click(object sender, EventArgs e)
        {
            string ss = this.textBox1.Text.Trim();
            MessageCls.SendMessage("TesMessage.exe", MsgType.WM_START, ss);
        } 
        //接收消息方法
        protected override void WndProc(ref System.Windows.Forms.Message e)
        {
            string msg=MessageCls.ReceiveMessage(ref e);
            if(msg!=null)
                this.label3.Text = msg; 
            base.WndProc(ref e);

        }
    }
}
