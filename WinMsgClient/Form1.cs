using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinMsgLibrary;

namespace WinMsgClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref COPYDATASTRUCT lParam);
        public Form1(string[] strArg)
        {
            InitializeComponent();
            MessageBox.Show(strArg[0]);
        }

        IntPtr ipHandle = IntPtr.Zero;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WinMsgLibrary.MsgType.WM_START:
                    {

                        ipHandle = m.WParam;
                        MessageBox.Show("start msg");
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IntPtr ii = IntPtr.Zero;
            COPYDATASTRUCT cds = new COPYDATASTRUCT();
            SendMessage(ipHandle, MsgType.WM_SERACH, ii, ref cds);
        }
    }
}
