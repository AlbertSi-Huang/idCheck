using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace doublescreen
{
    static class Program
    {

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();
         [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]

        static void Main()
        {
#if DEBUG
            AllocConsole();//调用系统API，调用控制台窗口
#endif
            bool ret;
            System.Threading.Mutex m = new System.Threading.Mutex(true, Application.ProductName, out ret);
            if (ret)
            {
                System.Windows.Forms.Application.EnableVisualStyles();   //这两行实现   XP   可视风格   
                System.Windows.Forms.Application.DoEvents();
                System.Windows.Forms.Application.Run(new fmainwindow());
                //  frmMain   为你程序的主窗体，如果是控制台程序不用这句   
                m.ReleaseMutex();
            }
            else
            {
                MessageBox.Show(null, "有一个和本程序相同的应用程序已经在运行，请不要同时运行多个本程序。\n\n这个程序即将退出。", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // 提示信息，可以删除。   
                Application.Exit();//退出程序   
            }
        }
    }
}

