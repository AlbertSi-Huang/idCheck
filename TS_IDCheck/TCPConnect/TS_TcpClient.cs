using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPLibrary.Abstracts;
using TCPLibrary.DefaultImplements;
using TCPLibrary.DeailMsg;
using System.Runtime.Serialization.Json;
using System.IO;
using TCPLibrary;

namespace TS_IDCheck
{
    public class TS_TcpClient
    {
        private BaseClientSocket m_client;
        private BaseProxySocket m_proxy;
        
        private static OpenDoorDelegate OnOpenDoor;
        public event OpenDoorDelegate openDoor
        {
            add { OnOpenDoor += new OpenDoorDelegate(value); }
            remove { OnOpenDoor -= new OpenDoorDelegate(value); }
        }

        private static ConnectManagerDelegate OnConnectChange;
        public event ConnectManagerDelegate connectManagerEvent
        {
            add { OnConnectChange += new ConnectManagerDelegate(value); }
            remove { OnConnectChange -= new ConnectManagerDelegate(value); }
        }

        private static ChangeConfigDelegate OnConfigChange;
        public event ChangeConfigDelegate configChangeEvent
        {
            add { OnConfigChange += new ChangeConfigDelegate(value); }
            remove { OnConfigChange -= new ChangeConfigDelegate(value); }
        }

        private static RestratDelegate OnRestratDelegate;
        public event RestratDelegate restartgChangeEvent
        {
            add { OnRestratDelegate += new RestratDelegate(value); }
            remove { OnRestratDelegate -= new RestratDelegate(value); }
        }

        //定时重连
        private System.Timers.Timer timeKeepLive;

        /// <summary>
        /// 连接ip
        /// </summary>
        private string m_connectIp;

        /// <summary>
        /// 连接端口
        /// </summary>
        private int m_connectPort;

        private bool isConnected = false;
        public bool IsConnected
        {
            set
            {
                if(isConnected != value)
                {
                    isConnected = value;
                    OnConnectChange(isConnected);
                }
            }
            get { return isConnected; }
        }

        private static TS_TcpClient instance = null;
        public static TS_TcpClient Single()
        {
            if(instance == null)
            {
                instance = new TS_TcpClient();
            }
            return instance;
        }
        private TS_TcpClient()
        {
            m_client = new BaseClientSocket();
            m_client.Connected += new ConnectedEventHandler(ClientConnect);
            m_client.DisConnected += new DisConnectedEventHandler(ClientDisConnected);
            m_client.MessageReceived += new MessageReceivedEventHandler(ClientMessageReceived);

            timeKeepLive = new System.Timers.Timer(5000);   //5 second reConnect
            timeKeepLive.Elapsed += TimeKeepLiveElapsed;
            
        }

        public bool SetIpAndPort(string ip,int port)
        {
            m_connectIp = ip;
            m_connectPort = port;
            return true;
        }
        public void ClientStartConn()
        {
            if(m_client != null)
            {
                if (!m_client._Connected)
                {
                    m_client.Connect(m_connectIp,m_connectPort);
                    timeKeepLive.Enabled = true;
                    return ;
                }
            }
            return ;
        }

        private void TimeKeepLiveElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!m_client._Connected)
                ClientStartConn();
        }

        /// <summary>
        /// 客户端接收信息回调
        /// </summary>
        /// <param name="proxySocket"></param>
        /// <param name="message"></param>
        private void ClientMessageReceived(ZProxySocket proxySocket, ZMessage message)
        {
            BaseMessage msg = message as BaseMessage;
            if (msg.eCmdType == ECOMMANDTYPE.NET_DOWNLOAD_CONTROL)
            {
                using (MemoryStream ms = new MemoryStream(msg.MsgContent))
                {
                    DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(MsgDownControl));
                    MsgDownControl mdc = (MsgDownControl)deseralizer.ReadObject(ms);
                    OnRestratDelegate(mdc.ControlNum);
                }
            }
            else if (msg.eCmdType == ECOMMANDTYPE.NET_DOWNLOAD_OPENDOOR)
            {
                //开门
            }
            //else if (msg.eCmdType == ECOMMANDTYPE.NET_DOWNLOAD_CLOSEDOOR)
            //{
            //    //关门
            //}
            else if (msg.eCmdType == ECOMMANDTYPE.NET_DOWNLOAD_CONFIG_BACK)
            {
                using (MemoryStream ms = new MemoryStream(msg.MsgContent))
                {
                    DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(MsgDownConfig));
                    MsgDownConfig mdc = (MsgDownConfig)deseralizer.ReadObject(ms);
                    OnConfigChange(mdc);
                }
            }
        }

        private void ClientDisConnected(ZProxySocket proxySocket)
        {
            //proxySocket
            IsConnected = false;
            m_client._Connected = false;
            timeKeepLive.Enabled = true;
        }

        /// <summary>
        /// 连接服务器回调
        /// </summary>
        /// <param name="proxySocket"></param>
        void ClientConnect(ZProxySocket proxySocket)
        {
            if(proxySocket != null)
            {
                //IsConnected = true;
                //m_proxy = proxySocket as BaseProxySocket;
                //MsgUpConfig muc = new MsgUpConfig();
                //muc.AutoStart = Convert.ToBoolean( ConfigOperator.Single().BAutoStart) ? 1 : 0;
                //muc.DetectScoreHigh = ConfigOperator.Single().FDetectScore.ToString();
                //muc.LocalCard = ConfigOperator.Single().LocalCard;
                //muc.Serial = ConfigOperator.Single().StrSerialNum;
                //muc.CacheTime = Convert.ToInt32( ConfigOperator.Single().CacheTime);
                //muc.SaveDays = Convert.ToInt32(ConfigOperator.Single().StrSaveDays);
                //muc.InstallSite = ConfigOperator.Single().StrInstallSite;
                //muc.DetectScoreLow = "20";
                //muc.Serial = ConfigOperator.Single().StrSerialNum;
                //muc.DeviceName = ConfigOperator.Single().StrSerialName;
                //muc.SavePath = ConfigOperator.Single().StrSavePath;
                //ObjectSerializer(muc, EMESSAGETYPE.MSG_DATA, ECOMMANDTYPE.NET_UPLOAD_CONFIG);
                
            }
        }
        
        /// <summary>
        /// 对象系列化并发送
        /// </summary>
        /// <param name="bdm">发送对象</param>
        /// <param name="emt">消息类型</param>
        /// <param name="ect">命令类型</param>
        public void ObjectSerializer(BaseDeailMsg bdm , EMESSAGETYPE emt,ECOMMANDTYPE ect)
        {
            if (!IsConnected)
            {
                return;
            }
            BaseDeailMsg b = null;
            DataContractJsonSerializer deseralizer = null;
            switch (ect)
            {
                case ECOMMANDTYPE.NET_UPLOAD_CONFIG:
                    {
                        //上传配置  上线时调用 
                        b = new MsgUpConfig();
                        b = bdm as MsgUpConfig;
                        deseralizer = new DataContractJsonSerializer(typeof(MsgUpConfig));
                    }
                    break;

                case ECOMMANDTYPE.NET_CHICK_IDCARD:
                    {
                        //身份证是否存在检测  读到卡时调用
                        b = new MsgChickIdcard();
                        b = bdm as MsgChickIdcard;
                        deseralizer = new DataContractJsonSerializer(typeof(MsgChickIdcard));
                    }
                    break;

                case ECOMMANDTYPE.NET_UPLOAD_CARDINFO:
                    {
                        //上传身份证信息 
                        b = new MsgCardInfo();
                        b = bdm as MsgCardInfo;
                        
                        deseralizer = new DataContractJsonSerializer(typeof(MsgCardInfo));
                    }
                    break;

                case ECOMMANDTYPE.NET_UPLOAD_CARDIMG:
                    {
                        //上传身份证照片
                        b = new MsgCardImg();
                        b = bdm as MsgCardImg;
                        deseralizer = new DataContractJsonSerializer(typeof(MsgCardImg));
                    }
                    break;

                case ECOMMANDTYPE.NET_UPLOAD_RECORD:
                    {
                        //上传身份证照片
                        b = new MsgManRecord();
                        b = bdm as MsgManRecord;
                        deseralizer = new DataContractJsonSerializer(typeof(MsgManRecord));
                    }
                    break;

            }

            MemoryStream msObj = new MemoryStream();
            deseralizer.WriteObject(msObj, b);
            msObj.Position = 0;
            StreamReader sr = new StreamReader(msObj);
            string json = sr.ReadToEnd();
            sr.Close();
            msObj.Close();
            m_proxy.SendMessage(new BaseMessage(emt, ect, Encoding.Unicode.GetBytes(json)));
        }


    }



}
