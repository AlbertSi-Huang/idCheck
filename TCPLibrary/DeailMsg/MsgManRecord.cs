using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPLibrary.DeailMsg
{
    public enum EDetectResult
    {
        EDR_UNKNOW = -1,

        EDR_SUCCESS,                //成功
        EDR_FAILED,                   //失败
        EDR_REGISTER,              //提示登记

        EDR_COUNT,
    }

    

    public class MsgManRecord : BaseDeailMsg
    {
        public string CId
        {
            set;
            get;
        }

        public string Serial
        {
            set;
            get;
        }

        public string CreateTime
        {
            set;
            get;
        }

        public string DScore
        {
            set;
            get;
        }

        public EDetectResult DRes
        {
            set;
            get;
        }
        
        /// <summary>
        /// 现场照
        /// </summary>
        public string SiteImg
        {
            set;
            get;
        }

        public string PlateNum
        {
            set;
            get;
        }

        public string PlateImg
        {
            set;get;
        }

        /// <summary>
        /// 
        /// </summary>
        public int InOut
        {
            set;
            get;
        }

        /// <summary>
        /// 更新状态 1实时数据 2历史数据
        /// </summary>
        public int UpdateState
        {
            set;
            get;
        }
        
    }


}
