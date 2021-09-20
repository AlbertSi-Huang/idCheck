using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPLibrary.Abstracts;
using System.Net.Sockets;

namespace TCPLibrary.DefaultImplements
{
    /// <summary>
    /// ZServerSocket的默认实现
    /// </summary>
    public class BaseServerSocket:ZServerSocket
    {
        protected override ZProxySocket GetProxy(Socket socket)
        {
            return new BaseProxySocket(socket);
        }
    }
}
