using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinMsgLibrary;

namespace WinMsgServer
{
    

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        string _startPath = Directory.GetCurrentDirectory();
        ProcessStartInfo _startInfo = new ProcessStartInfo();
        Process _pro = new Process();


        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case WinMsgLibrary.MsgType.WM_SERACH:
                    {
                        MessageBox.Show("search");
                    }
                    break;

                default:

                    base.WndProc(ref m);
                    break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _startInfo.FileName = _startPath + @"\WinMsgClient.exe";
            _pro.StartInfo = _startInfo;
            _pro.Start();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_pro != null)
            {
                COPYDATASTRUCT cps = new COPYDATASTRUCT();
                SendMessage(_pro.MainWindowHandle, WinMsgLibrary.MsgType.WM_START, this.Handle, ref cps);
            }
        }
    }
}
