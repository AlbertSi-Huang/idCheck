using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestartApplication
{
    class Program
    {
        /// <summary>
        /// 关闭进程
        /// </summary>
        /// <param name="processName">进程名</param>
        private static void KillProcess(string processName)
        {
            Process[] myproc = Process.GetProcesses();
            foreach (Process item in myproc)
            {
                //Console.WriteLine(item.ProcessName);
                //Console.ReadKey();
                if (item.ProcessName == processName)
                {
                    item.Kill();
                }
            }
        }

        static void Main(string[] args)
        {
            Thread.Sleep(10);
            KillProcess("doublescreen");
            KillProcess("TS_IDCheck");
            System.Diagnostics.Process.Start("shutdown", @"/r /t 0");
            //System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
            //myProcess.StartInfo.FileName = "cmd.exe";//启动cmd命令
            //myProcess.StartInfo.UseShellExecute = false;//是否使用系统外壳程序启动进程
            //myProcess.StartInfo.RedirectStandardInput = true;//是否从流中读取
            //myProcess.StartInfo.RedirectStandardOutput = true;//是否写入流
            //myProcess.StartInfo.RedirectStandardError = true;//是否将错误信息写入流
            //myProcess.Start();//启动进程
            //myProcess.StandardInput.WriteLine("shutdown -r -t 0");
            //Process restartProcess = null;
            //restartProcess = new Process();
            //string strPath = Directory.GetCurrentDirectory();
            //restartProcess.StartInfo.FileName = strPath + @"\TS_IDCheck.exe";
            //restartProcess.Start();
        }
    }
}
