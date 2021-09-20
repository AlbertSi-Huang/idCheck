using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPLibrary.Abstracts;
using System.Net.Sockets;

namespace TCPLibrary.DefaultImplements
{
    /// <summary>
    /// ZProxySocket的默认实现
    /// </summary>
    public class BaseProxySocket:ZProxySocket
    {
        public BaseProxySocket(Socket socket)
            : base(socket)
        {

        }
        protected override ZDataBuffer GetDataBuffer()
        {
            return new BaseDataBuffer();
        }
    }
}
