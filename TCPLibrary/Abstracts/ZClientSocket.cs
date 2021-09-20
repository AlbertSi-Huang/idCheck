using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TCPLibrary.Abstracts
{
    /// <summary>
    /// TCP通信客户端
    /// </summary>
    public abstract class ZClientSocket
    {
        //工作socket
        private Socket _socket;
        /// <summary>
        /// 当前连接状态
        /// </summary>
        private bool _connected = false;

        public bool _Connected
        {
            set { _connected = value; }
            get { return _connected; }
        }

        /// <summary>
        /// 代理socket
        /// </summary>
        private ZProxySocket _proxy;
        /// <summary>
        /// 收到服务端消息时激发该事件
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived;
        /// <summary>
        /// 与服务端建立连接时激发该事件
        /// </summary>
        public event ConnectedEventHandler Connected;
        /// <summary>
        /// 与服务端断开连接时激发该事件
        /// </summary>
        public event DisConnectedEventHandler DisConnected;

        /// <summary>
        /// 创建代理socket
        /// </summary>
        /// <param name="socket"></param>
        protected abstract ZProxySocket GetProxy(Socket socket);

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, int port)
        {
            if (!_connected)
            {
                try
                {
                    if (_socket == null)
                    {
                        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }
                    _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(OnConnect), null);
                    
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// 异步连接服务器回调方法
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);

                _proxy = GetProxy(_socket);  //创建代理socket
                _proxy.DisConnected += new DisConnectedEventHandler(_proxy_DisConnected);
                _proxy.MessageReceived += new MessageReceivedEventHandler(_proxy_MessageReceived);
                _proxy.StartReceive();  //开启接收数据
                _connected = true;
                if (Connected != null)  //成功 激发Connected事件
                {
                    Connected(_proxy);
                }
            }
            catch
            {
                if (Connected != null)  //失败 激发Connected事件
                {
                    Connected(null);  
                }
            }
        }
        /// <summary>
        /// 接收服务端消息
        /// </summary>
        /// <param name="proxySocket"></param>
        /// <param name="message"></param>
        void _proxy_MessageReceived(ZProxySocket proxySocket, ZMessage message)
        {
            if (MessageReceived != null)
            {
                MessageReceived(proxySocket, message);  //激发事件
            }
        }
        /// <summary>
        /// 与服务端断开连接
        /// </summary>
        /// <param name="proxySocket"></param>
        void _proxy_DisConnected(ZProxySocket proxySocket)
        {
            if (DisConnected != null)
            {
                DisConnected(proxySocket);  //激发事件
            }
        }
    }
}
