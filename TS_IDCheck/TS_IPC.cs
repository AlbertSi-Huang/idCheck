using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TS_IDCheck
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public class MsgType
    {
        public const int USER = 0x0400;
        public const int WM_START = USER + 101;
        public const int WM_UPDATE = USER + 102;
        public const int WM_SERACH = USER + 103;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COPYDATASTRUCT
    {
        public int dwData;    //not used  
        public int cbData;    //长度  
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }

    /// <summary>
    /// 进程间通信 通过sendMessage发送消息
    /// </summary>
    public class TS_IPC : Form
    {
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        private IntPtr _handle = IntPtr.Zero;

        string _startPath = Directory.GetCurrentDirectory();
        ProcessStartInfo _startInfo = new ProcessStartInfo();
        Process _pro = new Process();

        private static NoCardSearchDelegate OnNoCardSearch;
        public event NoCardSearchDelegate noCardSearchEvent
        {
            add { OnNoCardSearch += new NoCardSearchDelegate(value); }
            remove { OnNoCardSearch -= new NoCardSearchDelegate(value); }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case MsgType.WM_SERACH:
                    {
                        //手动查找
                        COPYDATASTRUCT mystr = new COPYDATASTRUCT();
                        Type mytype = mystr.GetType();
                        mystr = (COPYDATASTRUCT)m.GetLParam(mytype);
                        
                        //此事件主程序触发 类似读卡完成处理
                        OnNoCardSearch(mystr.lpData);
                    }
                    break;

                default:

                    base.WndProc(ref m);
                    break;
            }
        }

        private static TS_IPC instance = null;

        public static TS_IPC Single()
        {
            if(instance == null)
            {
                instance = new TS_IPC();
            }
            return instance;
        }

        private TS_IPC()
        {
            _startInfo.FileName = _startPath + @"\doublescreen.exe";
            _pro.StartInfo = _startInfo;
            _pro.Start();
        }
        
        /// <summary>
        /// 启动后发送本窗口句柄和文件路径到副屏程序
        /// </summary>
        public void SendStartInfo()
        {
            if(_pro != null)
            {
                COPYDATASTRUCT cps = new COPYDATASTRUCT();
                cps.lpData = ConfigOperator.Single().StrSavePath;
                cps.cbData = cps.lpData.Length;
                SendMessage(_pro.MainWindowHandle, MsgType.WM_START, this.Handle,ref cps);
            }
        }

        /// <summary>
        /// 发送更新信息到副屏程序 保存完成后调用
        /// </summary>
        /// <param name="nUpdate"></param>
        public void SendUpdateInfo(int nUpdate)
        {
            if (_pro != null)
            {
                COPYDATASTRUCT cps = new COPYDATASTRUCT();
                cps.lpData = nUpdate.ToString();
                cps.cbData = cps.lpData.Length;
                SendMessage(_pro.MainWindowHandle, MsgType.WM_UPDATE, this.Handle, ref cps);
            }
        }


    }
}
