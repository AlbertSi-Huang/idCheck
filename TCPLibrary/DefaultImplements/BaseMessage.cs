using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPLibrary.Abstracts;
using System.IO;

namespace TCPLibrary.DefaultImplements
{
    /// <summary>
    /// ZMessage的默认实现
    /// </summary>
    public class BaseMessage:ZMessage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public int MsgType
        {
            get;
            set;
        }
        /// <summary>
        /// 消息内容
        /// </summary>
        public byte[] MsgContent
        {
            get;
            set;
        }
        
        public EMESSAGETYPE EMsgType = EMESSAGETYPE.MSG_UNKNOW;
        public ECOMMANDTYPE eCmdType = ECOMMANDTYPE.NET_COMMAND_UNKNOW;

        /// <summary>
        /// 消息头尾
        /// </summary>
        const string strHead = "TS_HEAD";
        const string strTail = "TS_TAIL";
        byte[] MsgHead;
        byte[] MsgTail; 

        public BaseMessage(int msgType, byte[] msgContent)
        {
            MsgType = msgType;
            MsgContent = msgContent;
        }

        public BaseMessage(EMESSAGETYPE emt, ECOMMANDTYPE ect, byte[] msgContent)
        {
            EMsgType = emt;
            eCmdType = ect;
            MsgContent = msgContent;
            MsgHead = System.Text.Encoding.Default.GetBytes(strHead);
            MsgTail = System.Text.Encoding.Default.GetBytes(strTail);
        }

        /// <summary>
        /// 按照规定协议，重写RawData属性
        /// </summary>
        public override byte[] RawData
        {
            get
            {

                byte[] rawdata = new byte[MsgHead.Length + 12 + MsgContent.Length + MsgTail.Length];  //消息类型 + 消息长度 + 消息内容
                using (MemoryStream ms = new MemoryStream(rawdata))
                {
                    BinaryWriter bw = new BinaryWriter(ms);
                    bw.Write(MsgHead);
                    bw.Write(MsgContent.Length);
                    int nEmt = (int)EMsgType;
                    int nEct = (int)eCmdType;
                    bw.Write(nEmt);
                    bw.Write(nEct);

                    bw.Write(MsgContent); //最后写入消息内容
                    bw.Write(MsgTail);
                    return rawdata;
                }
            }
        }


    }

    #region 以下两个类型组合成为协议类型

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum  EMESSAGETYPE
    {
        MSG_UNKNOW = -1,

        MSG_DATA,
        MSG_CONTROL,
        MSG_CMD,

        MSG_COUNT
    }

    /// <summary>
    /// 命令类型
    /// </summary>
    public enum ECOMMANDTYPE
    {
        NET_COMMAND_UNKNOW = -1,

        NET_UPLOAD_CARDINFO,
        NET_UPLOAD_CARDIMG,
        NET_UPLOAD_RECORD,
        NET_UPLOAD_PLATERECORD,
        NET_UPLOAD_CONFIG,
        NET_CHICK_IDCARD,
        NET_CHECK_IDCARD_BACK,
        NET_KEEPLIVE,
        NET_DOWNLOAD_CONTROL,
        NET_DOWNLOAD_CONFIG,
        NET_DOWNLOAD_CONFIG_BACK,
        NET_DOWNLOAD_OPENDOOR,

        
        NET_COMMAND_COUNT,
    }


    #endregion
}
