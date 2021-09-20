using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TCPLibrary.Abstracts;

namespace TCPLibrary.DefaultImplements
{
    /// <summary>
    /// ZDataBuffer的默认实现
    /// </summary>
    public class BaseDataBuffer:ZDataBuffer
    {
        /// <summary>
        /// 按照规定协议，重写TryReadMessage方法
        /// </summary>
        /// <returns></returns>
        internal override ZMessage TryReadMessage()
        {
            if (_length >= 19)   //  head + length + msg + cmd
            {
                using (MemoryStream ms = new MemoryStream(_buffer))
                {
                    BinaryReader br = new BinaryReader(ms);
                    //int msgtype = br.ReadInt32();  //读取消息类型
                    //int msglength = br.ReadInt32();  //读取消息长度
                    byte[] msgHead = br.ReadBytes(7);
                    int msglength = br.ReadInt32();
                    if (_length - 19 >= msglength)  //如果缓冲区中存在一条完整消息，则读取
                    {
                        EMESSAGETYPE emt = (EMESSAGETYPE)br.ReadInt32();
                        ECOMMANDTYPE ect = (ECOMMANDTYPE)br.ReadInt32();
                        byte[] msgcontent = br.ReadBytes(msglength);
                        byte[] bTail = br.ReadBytes(7);
                        BaseMessage bm = new BaseMessage(emt, ect, msgcontent); //还原成一条完整的消息
                        Remove(26 + msglength);  //注意！ 移除已读数据

                        return bm;  //返回读取到的消息
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }
    }
}
