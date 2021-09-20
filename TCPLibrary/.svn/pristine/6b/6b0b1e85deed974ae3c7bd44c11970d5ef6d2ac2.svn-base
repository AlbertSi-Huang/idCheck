using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPLibrary.Abstracts
{
    /// <summary>
    /// 表示通讯过程中一个完整数据包（消息）
    /// </summary>
    public abstract class ZMessage
    {
        /// <summary>
        /// 数据包中原始数据（字节流）,派生类必须遵守“应用层协议”实现该属性
        /// </summary>
        public abstract byte[] RawData
        {
            get;
        }
    }
}
