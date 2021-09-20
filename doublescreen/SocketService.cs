using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace doublescreen
{
    public delegate void GetNoNetDelegate();
    public class SocketService
    {
        private Thread threadWatch = null; //负责监听客户端的线程
        private Socket socketWatch = null; //负责监听客户端的套接字
        public delegate void SocketServiceRecvHandler(string  ss);
        public event SocketServiceRecvHandler SocketServiceRecv;
        public event IdInput.IdInputRefreshhandle IdInputRefresh;
        private Socket socConnection = null;
        static bool sGetClientReq = false;
        
        public enum dbScreenSendType
        {
            SendQuerry = 1,
            SendCompare ,
            SendOpenDoor
        }
        public enum SendToScreenType
        {
            SendGetCompareResult = 1,
            SendQuerryGetPic,
            SendQuerrError,
            SendBlackList,
            SendUrlCommitSucc,
            SendUrlCommitFail,
            SendUrlCommitSuccNoNet,             //没网的情况下比对成功
            SendUrlCommitFailNoNet,             //没网的情况下比对失败
            SendTicketFail,                     //票务检查失败
        }
        private static SocketService instance = null;
        public static SocketService Single()
        {
            if (instance == null)
            {
                instance = new SocketService();
            }
            return instance;
        }
        public void ServiceStart()
        {
            Console.WriteLine(DateTime.Now.ToString() +  " 开启tcp服务");
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //服务端发送信息 需要1个IP地址和端口号
            IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
            //将IP地址和端口号绑定到网络节点endpoint上 
            IPEndPoint endpoint = new IPEndPoint(ipaddress, 2020); //获取文本框上输入的端口号
            try
            {
                //监听绑定的网络节点
                socketWatch.Bind(endpoint);
                //将套接字的监听队列长度限制为20
                socketWatch.Listen(20);

                //创建一个监听线程 
                threadWatch = new Thread(WatchConnecting);
                //将窗体线程设置为与后台同步
                threadWatch.IsBackground = true;
                //启动线程
                threadWatch.Start();
            }
            catch(Exception ex)
            {
                string mystring = string.Empty;
                mystring = ex.ToString();
            }
        }
        
        private void WatchConnecting()
        {
            while (true)  //持续不断监听客户端发来的请求
            {
                socConnection = socketWatch.Accept();
                //   txtMsg.AppendText("客户端连接成功" + "\r\n");
                //创建一个通信线程 
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ServerRecMsg);
                Thread thr = new Thread(pts);
                thr.IsBackground = true;
                //启动线程
                thr.Start(socConnection);
            }
        }
        /// <summary>
        /// 发送信息到客户端的方法
        /// </summary>
        /// <param name="sendMsg">发送的字符串信息</param>
        public void ServerSendMsg(string sendMsg)
        {
            if(!sGetClientReq)
            {
                return;
            }
            //将输入的字符串转换成 机器可以识别的字节数组
            byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //向客户端发送字节数组信息
            try
            {
                socConnection.Send(arrSendMsg);
            }
            catch (Exception ex)
            {
                string mystring = string.Empty;
                mystring = ex.ToString();
            }
            //将发送的字符串信息附加到文本框txtMsg上
            //     txtMsg.AppendText("So-flash:" + GetCurrentTime() + "\r\n" + sendMsg + "\r\n");
        }
        public void ServerSendMsg(dbScreenSendType  sendType,string sendMsg)
        {
            string mystr = sendType.ToString() + "," + sendMsg;

            ServerSendMsg(mystr);
        }
           

       
        /// <summary>
        /// 接收客户端发来的信息 
        /// </summary>
        /// <param name="socketClientPara">客户端套接字对象</param>

        private void ServerRecMsg(object socketClientPara)
        {
            Socket socketServer = socketClientPara as Socket;

            if (socketServer == null || !socketServer.Connected) return;

            while (true)
            {
                //创建一个内存缓冲区 其大小为1024*1024字节  即1M
                byte[] arrServerRecMsg = new byte[1024 * 1024];
                //将接收到的信息存入到内存缓冲区,并返回其字节数组的长度
                int length = 0;
                try
                {
                    length = socketServer.Receive(arrServerRecMsg);
                }
                catch(Exception ex)
                {
                    return;
                }
                
                if (length > 2)
                {
                    string strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);
                    Console.WriteLine(strSRecMsg);
                    if (strSRecMsg == "hello")
                    {
                        sGetClientReq = true;
                    }
                    else
                    {

                        // Console.WriteLine(strSRecMsg);
                        string mystr = string.Empty;
                        string[] myStrArry;
                        myStrArry = strSRecMsg.Split(',');
                        if (myStrArry.Length <= 0)
                        {
                            IdInputRefresh("Qerror");
                            return;
                        }
                        string log = "网络信息为";
                        for (int i = 0; i < myStrArry.Length; i++)
                        {
                            log = log + myStrArry[i];
                        }
                        MTWFile.Single().WriteLine(log);
                        int tmp = 0;
                        try
                        {
                            tmp = Convert.ToInt32(myStrArry[0]);
                        }
                        catch
                        {
                            IdInputRefresh("Qerror");
                            return;
                        }
                        if ((tmp == (int)SocketService.SendToScreenType.SendGetCompareResult) || 
                            (tmp == (int)SocketService.SendToScreenType.SendBlackList))
                        {
                            if (SocketServiceRecv != null)
                            {
                                MTWFile.Single().WriteLine("Ok or Blacklist");
                                SocketServiceRecv(strSRecMsg);
                            }
                        }                      
                        else if (tmp == (int)SocketService.SendToScreenType.SendQuerrError)
                        {
                            MTWFile.Single().WriteLine("Qerror");
                            if (IdInputRefresh != null)
                            {
                                IdInputRefresh("Qerror");
                            }
                            //InputFormUpdata += new IdInput.InputFormRecvHandler(IdInput);
                            // InputFormUpdata();
                        }
                        else if (tmp == (int)SocketService.SendToScreenType.SendQuerryGetPic)
                        {
                            MTWFile.Single().WriteLine("GetPic");
                            if (IdInputRefresh != null)
                            {
                                IdInputRefresh("GetPic");
                            }
                            //  InputFormUpdata += new IdInput.InputFormRecvHandler("Qerror");
                            //InputFormUpdata("GetPic");
                        }
                        else if(tmp == (int)SocketService.SendToScreenType.SendUrlCommitSucc)
                        {
                            fmainwindow.ref_win.setTextValue("文件上传成功");
                        }
                        else if (tmp == (int)SocketService.SendToScreenType.SendUrlCommitFail)
                        {
                            fmainwindow.ref_win.setTextValue("文件上传失败");
                        }else if(tmp == (int)SocketService.SendToScreenType.SendUrlCommitSuccNoNet || 
                            tmp == (int)SocketService.SendToScreenType.SendUrlCommitFailNoNet)
                        {
                            fmainwindow.ref_win.setTextValue("");
                            GetNoNetEvent();
                        }
                    }
                }
            }
        }

        public event GetNoNetDelegate GetNoNetEvent;

        public void ServerSendOpenDoor()
        {
            string mystr = dbScreenSendType.SendOpenDoor.ToString();
          
            ServerSendMsg(mystr);
        }
    }
}
