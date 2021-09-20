using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TCPLibrary.Abstracts
{
    /// <summary>
    /// 通讯双方用到的代理Socket
    /// </summary>
    public abstract class ZProxySocket
    {
        /// <summary>
        /// 工作Socket
        /// </summary>
        private Socket _socket;
        /// <summary>
        /// 接收数据缓冲区
        /// </summary>
        private byte[] _buffer_received = new byte[1024];

        /// <summary>
        /// 远程终端IP
        /// </summary>
        public string RemoteIP
        {
            get
            {
                try
                {
                    return (_socket.RemoteEndPoint as IPEndPoint).Address.ToString();
                }
                catch
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 远程终端端口
        /// </summary>
        public int RemotePort
        {
            get
            {
                try
                {
                    return (_socket.RemoteEndPoint as IPEndPoint).Port;
                }
                catch
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="socket"></param>
        public ZProxySocket(Socket socket)
        {
            _socket = socket;
        }
        /// <summary>
        /// 与服务器断开连接时激发该事件
        /// </summary>
        internal event DisConnectedEventHandler DisConnected;
        /// <summary>
        /// 接收到消息时激发该事件
        /// </summary>
        internal event MessageReceivedEventHandler MessageReceived;

        /// <summary>
        /// 创建数据缓冲区
        /// </summary>
        /// <returns></returns>
        protected abstract ZDataBuffer GetDataBuffer();

        /// <summary>
        /// 开启数据接收泵
        /// </summary>
        internal void StartReceive()
        {
            if (_socket != null)
            {
                _socket.BeginReceive(_buffer_received, 0, 1024, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
        }

        /// <summary>
        /// 异步接收数据回调方法
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int real_received = _socket.EndReceive(ar);
                if (real_received != 0)
                {
                    ZDataBuffer databuffer = GetDataBuffer();
                    byte[] uncomplete = ar.AsyncState as byte[];  //上次未处理完的数据
                    if (uncomplete != null)
                    {
                        databuffer.WriteBytes(uncomplete, 0, uncomplete.Length);  //将上次未处理完的数据重新写入缓冲区
                    }
                    databuffer.WriteBytes(_buffer_received, 0, real_received);  //将本次接收到的数据写入缓冲区

                    ZMessage message = null;
                    while ((message = databuffer.TryReadMessage()) != null)  //尝试从缓冲区中读取一条完整消息
                    {
                        if (MessageReceived != null)
                        {
                            MessageReceived.BeginInvoke(this, message, null, null);  //异步激发事件
                        }
                    }
                    _socket.BeginReceive(_buffer_received, 0, 1024, SocketFlags.None, new AsyncCallback(OnReceive), databuffer.UnCompelete);  //将未处理完的数据存放在state参数中，并开始下一次异步接收数据
                }
                else
                {
                    if (DisConnected != null)
                    {
                        DisConnected(this);
                    }
                }
            }
            catch
            {
                if (DisConnected != null)
                {
                    DisConnected(this);
                }
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="message"></param>
        public int SendMessage(ZMessage message)
        {
            try
            {
                return _socket.Send(message.RawData);  //
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
    }
}
