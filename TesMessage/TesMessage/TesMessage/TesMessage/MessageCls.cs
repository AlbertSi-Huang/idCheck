using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TesMessage
{
    //WM_COPYDATA消息所要求的数据结构
    public  struct  CopyDataStruct  
    {  
        public  IntPtr  dwData;  
        public  int  cbData;  

        [MarshalAs(UnmanagedType.LPStr)]  
        public  string  lpData;  
    }  
   public  class MessageCls
    {
  public const  int  WM_COPYDATA  =  0x004A;  

        //通过窗口的标题来查找窗口的句柄
        [DllImport("User32.dll",EntryPoint="FindWindow")]  
        private  static  extern  int  FindWindow(string  lpClassName, string  lpWindowName);  

        //在DLL库中的发送消息函数
        [DllImport("User32.dll",EntryPoint="SendMessage")]  
        private  static  extern  int  SendMessage
            (  
            int  hWnd,                        // 目标窗口的句柄  
            int  Msg,                        // 在这里是WM_COPYDATA
            int  wParam,                    // 第一个消息参数
            ref  CopyDataStruct  lParam        // 第二个消息参数
            );   
        /// <summary>
        /// 发送消息，只能传递一个自定义的消息ID和消息字符串，想传一个结构，但没成功
        /// </summary>
        /// <param name="destProcessName">目标进程名称，如果有多个，则给每个都发送</param>
        /// <param name="msgID">自定义数据，可以通过这个来决定如何解析下面的strMsg</param>
        /// <param name="strMsg">传递的消息，是一个字符串</param>
        public static void SendMessage(string destProcessName, int msgID, string strMsg) 
        {
            if(strMsg == null) return;

            //按进程名称查找，同名称的进程可能有许多，所以返回的是一个数组
            Process []foundProcess = Process.GetProcessesByName(destProcessName);
            foreach(Process p in foundProcess) 
            {
                int toWindowHandler = p.MainWindowHandle.ToInt32();
                if(toWindowHandler  !=  0)  
                {  
                     CopyDataStruct  cds;  
                    cds.dwData  =  (IntPtr)  msgID;   //这里可以传入一些自定义的数据，但只能是4字节整数      
                    cds.lpData  =  strMsg;            //消息字符串
                    cds.cbData  =  System.Text.Encoding.Default.GetBytes(strMsg).Length + 1;  //注意，这里的长度是按字节来算的

                    //发送方的窗口的句柄, 由于本系统中的接收方不关心是该消息是从哪个窗口发出的，所以就直接填0了
                    int fromWindowHandler = 0;    
                    SendMessage(toWindowHandler,  WM_COPYDATA,  fromWindowHandler,  ref  cds);  
                }
            }
        }

        /// <summary>
        /// 接收消息，得到消息字符串
        /// </summary>
        /// <param name="m">System.Windows.Forms.Message m</param>
        /// <returns>接收到的消息字符串</returns>
        public static string ReceiveMessage(ref  System.Windows.Forms.Message e)
        {
            if (e.Msg == WM_COPYDATA)
            {
                CopyDataStruct cds = (CopyDataStruct)e.GetLParam(typeof(CopyDataStruct));
                return cds.lpData.ToString();  //将文本信息显示到文本框
                //MessageBox.Show(cds.lpData);
            }
            return null;
        }
    }
}

 