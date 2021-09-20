using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPLibrary.Abstracts;

namespace TCPLibrary
{
    /// <summary>
    /// 表示处理连接服务器事件的方法
    /// </summary>
    /// <param name="proxySocket"></param>
    public delegate void ConnectedEventHandler(ZProxySocket proxySocket);
    /// <summary>
    /// 表示处理断开服务器连接事件的方法
    /// </summary>
    /// <param name="proxySocket"></param>
    public delegate void DisConnectedEventHandler(ZProxySocket proxySocket);

    /// <summary>
    /// 表示处理接收消息事件的方法
    /// </summary>
    /// <param name="proxySocket"></param>
    /// <param name="message"></param>
    public delegate void MessageReceivedEventHandler(ZProxySocket proxySocket,ZMessage message);
}
