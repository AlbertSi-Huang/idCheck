using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using TS_IDCheck.http;
using System.IO;
using TS_IDCheck.log;
using Common;
using System.Windows.Interop;
using System.Diagnostics;
using TS_IDCheck.Info;
using System.Runtime.InteropServices;

namespace TS_IDCheck
{

   
    public class dbscclient
    {
        Socket socketClient = null;
        Thread threadClient = null;
        //定义委托，在SaveRecordFiles类里声明事件，用于通过socket发送消息到副屏软件
        //   public delegate void SendMagToScreenHandler();
        private static CaptureCardCompleteDelegate OnReadComplete;
        public event CaptureCardCompleteDelegate readCompleteEvent
        {
            add { OnReadComplete += new CaptureCardCompleteDelegate(value); }
            remove { OnReadComplete -= new CaptureCardCompleteDelegate(value); }
        }

        private static ReadCardCompleteDelegate OnReadCardComplete;
        public event ReadCardCompleteDelegate readCardCompleteEvent
        {
            add { OnReadCardComplete += new ReadCardCompleteDelegate(value); }
            remove { OnReadCardComplete -= new ReadCardCompleteDelegate(value); }
        }

        private static dbscclient instance = null;
        private static bool _IsConnected = false;

        /// <summary>
        /// 读卡器类型：0-身份证，2-护照
        /// </summary>
        private static string _cardReaderBrand = "1";
        public static dbscclient Single()
        {
            if (instance == null)
            {
                instance = new dbscclient();
            }
            return instance;
        }

        public static dbscclient Single(string sType)
        {
            if (instance == null)
            {
                instance = new dbscclient();
            }
            _cardReaderBrand = sType;//卡类型
            return instance;
        }

        public bool IsConnected()
        {
            return _IsConnected;
        }
        
        public void BeginListen()
        {
            
            try
            {
                //定义一个套字节监听  包含3个参数(IP4寻址协议,流式连接,TCP协议)
                socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //需要获取文本框中的IP地址
                IPAddress ipaddress = IPAddress.Parse("127.0.0.1");
                //将获取的ip地址和端口号绑定到网络节点endpoint上
                IPEndPoint endpoint = new IPEndPoint(ipaddress, 2020);
                Thread.Sleep(2000);
                //这里客户端套接字连接到网络节点(服务端)用的方法是Connect 而不是Bind
                socketClient.Connect(endpoint);
                if (socketClient.Connected)
                {
                    Trace.WriteLine(DateTime.Now.ToString() + "连接服务器成功");
                }
                //socketClient.BeginConnect(endpoint, asyncCallback, socketClient);
                //创建一个线程 用于监听服务端发来的消息
                threadClient = new Thread(RecMsg);
                //将窗体线程设置为与后台同步
                threadClient.IsBackground = true;
                //启动线程
                threadClient.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void asyncCallback(IAsyncResult ar)
        {
            Socket pox = ar as Socket;

            
        }

        /// <summary>
        /// 侧屏信息类型
        /// </summary>
        public enum dbScreenSendType
        {
            SendQuerry = 1,
            SendCompare,
            SendOpenDoor,
            StaticCloseDoor,
            StaticOpenDoor
        }

        /// <summary>
        /// 发送至侧屏信息类型
        /// </summary>
        public enum SendToScreenType
        {
            SendGetCopareResult = 1,
            SendQuerryGetPic,
            SendQuerrError,
            SendBlackList,
            SendUrlCommitSucc,
            SendUrlCommitFail,
            SendUrlCommitSuccNoNet,             //没网的情况下比对成功
            SendUrlCommitFailNoNet,             //没网的情况下比对失败
        }

        private string _lastCheckId = "";

        /// <summary>
        /// 接收服务端发来信息的方法
        /// </summary>
        public void RecMsg()
        {
            while (true) //持续监听服务端发来的消息
            {
                //定义一个1M的内存缓冲区 用于临时性存储接收到的信息
                byte[] arrRecMsg = new byte[1024 * 1024];
                //将客户端套接字接收到的数据存入内存缓冲区, 并获取其长度
                int length = 0;
                try
                {
                    length = socketClient.Receive(arrRecMsg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Dbreceiving error: ",ex.Message);
                    return;
                }
               
                if (length > 3)
                {
                    Console.WriteLine(arrRecMsg);
                    _IsConnected = true;
                    string ss = Encoding.UTF8.GetString(arrRecMsg, 0, length);
                    Trace.WriteLine("得到侧屏命令");
                    Trace.WriteLine(ss);
                    string[] mystr;
                    mystr = ss.Split(',');
                    if (mystr[0] == dbScreenSendType.SendQuerry.ToString())
                    {
                        if (mystr[1] != null)
                        {
                            ThirdQuerryByIdMsg myMsg = new ThirdQuerryByIdMsg();
                            ThirdQuerryByIdReturnMsg tmpInfo = new ThirdQuerryByIdReturnMsg();
                            myMsg.method = "querryPersonInfo";
                            //myMsg.invokeKey = SanTranProto.Single().GetToken();
                            myMsg.cardNo = mystr[1];
                            _lastCheckId = mystr[1];
                            myMsg.personName = "";
                            //查询成功，有图像返回
                            int nPlateChoose = ConfigOperator.Single().PlateChoose;
                            string strUrl = string.Empty;
                            if(nPlateChoose == 1 || nPlateChoose == 4)
                            {
                                strUrl = "http://10.20.50.110:9092/verificationInterface/dataQuery/queryPersonInfo";
                            }
                            else if(nPlateChoose == 2)
                            {
                                strUrl = "http://21.0.0.125:9092/verificationInterface/dataQuery/queryPersonInfo";

                            }
                            else if(nPlateChoose == 3)
                            {
                                strUrl = "http://124.117.209.133:29092/verificationInterface/dataQuery/queryPersonInfo";
                            }
                            string tt = SanTranProto.Single().ThirdQueryById(myMsg,strUrl);

                            Trace.WriteLine("身份证查询返回");
                            Trace.WriteLine(tt);
                            if (tt != null)
                            {
                                //反序列化
                                tmpInfo = JsonConvert.DeserializeObject<ThirdQuerryByIdReturnMsg>(tt);
                                if (tmpInfo.message == "调用成功")
                                {
                                    Trace.WriteLine("图像查询返回正确");
                                    //base64转为图片并保存
                                    byte[] arr2 = Convert.FromBase64String(tmpInfo.personInfo.photo);
                                    using (MemoryStream ms2 = new MemoryStream(arr2))
                                    {
                                        System.Drawing.Bitmap bmp2 = new System.Drawing.Bitmap(ms2);
                                        SaveRecordFile mysave = new SaveRecordFile();
                                        mysave.SaveQuerryImg(bmp2);
                                        //  bmp2.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        bmp2.Dispose();
                                        //发送得到图片信息
                                        //SendToScreenType.SendQuerryGetPic.ToString()
                                        ClientSendMsg("2,0");
                                    }
                                }
                                else
                                {
                                    ClientSendMsg("3,0");
                                }
                            }
                            else
                            {
                                ClientSendMsg(SendToScreenType.SendQuerrError.ToString());
                            }
                        }
                    }

                    else if (mystr[0] == dbScreenSendType.SendOpenDoor.ToString())
                    {
                        Trace.WriteLine("接收到侧屏传来开门信息");
                        ComOpenDoor myopen = ComOpenDoor.getInstance();//开门
                        myopen.WriteCom();
                    }
                    else if (mystr[0] == dbScreenSendType.StaticCloseDoor.ToString())
                    {
                        //常开
                        Trace.WriteLine("执行侧屏发送的常开命令");
                        ComOpenDoor myopen = ComOpenDoor.getInstance();
                        //myopen.Double_StaticOpenDoor();
                        myopen.WriteCom();
                    }
                    else if (mystr[0] == dbScreenSendType.StaticOpenDoor.ToString())
                    {
                        //常闭
                        Trace.WriteLine("执行侧屏发送的常闭命令");
                        ComOpenDoor myopen = ComOpenDoor.getInstance();
                        myopen.Double_StaticCloseDoor();
                    }
                    else if (mystr[0] == dbScreenSendType.SendCompare.ToString())
                    {
                        //提示比对 类似发送读卡成功信息

                        Trace.WriteLine("图像查询后比对");
                        SCardInfo cardInfo = new SCardInfo();
                        cardInfo._idNum = _lastCheckId;
                        string savePath = ConfigOperator.Single().StrSavePath;
                        string queryPic = savePath + @"\Querrypic\" + "tmp.jpg";
                        cardInfo._photo = queryPic;
                        OnReadComplete(cardInfo);
                    }
                }
            }
            //将套接字获取到的字节数组转换为人可以看懂的字符串
            // string strRecMsg = Encoding.UTF8.GetString(arrRecMsg, 0, length);
            //将发送的信息追加到聊天内容文本框中
        }


        /// <summary>
        /// 发送字符串信息到服务端的方法
        /// </summary>
        /// <param name="sendMsg">发送的字符串信息</param>
        public void ClientSendMsg(string sendMsg)
        {
            //将输入的内容字符串转换为机器可以识别的字节数组
            byte[] arrClientSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            //调用客户端套接字发送字节数组  
            try
            {
                int nRet = socketClient.Send(arrClientSendMsg);
                if(nRet == 0)
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //将发送的信息追加到聊天内容文本框中
        }
    }
}   

