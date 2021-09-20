using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPLibrary.Abstracts;
using System.Net.Sockets;

namespace TCPLibrary.DefaultImplements
{
    /// <summary>
    /// ZClientSocket的默认实现
    /// </summary>
    public class BaseClientSocket:ZClientSocket
    {
        protected override ZProxySocket GetProxy(Socket socket)
        {
            return new BaseProxySocket(socket);
        }
    }
}
