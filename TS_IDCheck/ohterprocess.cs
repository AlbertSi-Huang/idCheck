using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; 
namespace TS_IDCheck
{
    class OhterProcess
    {
        #region     
        public void startExeFile(string myPathName,string myArgu,int myOpenType)
        {

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = myPathName;
            info.Arguments = myArgu;
            if(myOpenType>3)
            {
                return;
            }
            switch(myOpenType)
            {
                case 0:
                    info.WindowStyle = ProcessWindowStyle.Normal;
                break;
                case 1:
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    break;
                case 2:
                    info.WindowStyle = ProcessWindowStyle.Minimized;
                break;
                case 3:
                    info.WindowStyle = ProcessWindowStyle.Maximized;
                break;
                default:
                    info.WindowStyle = ProcessWindowStyle.Maximized;
                break;
            }
            
            Process pro = Process.Start(info);
        //    pro.WaitForExit();
        }
        #endregion
        public void CloseAExeFile(string myFilename) //exe的进程名
        {
            Process[] allProgresse = System.Diagnostics.Process.GetProcessesByName(myFilename);
            foreach (Process closeProgress in allProgresse)
            {
                if (closeProgress.ProcessName.Equals(myFilename))
                {
                    closeProgress.Kill();
                    closeProgress.WaitForExit();
                    break;
                }
            }
        }
    }
    
}
