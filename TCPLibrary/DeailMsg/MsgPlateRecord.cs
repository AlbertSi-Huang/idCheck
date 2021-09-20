using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPLibrary.DeailMsg
{
    public class MsgPlateRecord : BaseDeailMsg
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

        /// <summary>
        /// 车牌号
        /// </summary>
        public string PlateNum
        {
            set;
            get;
        }

        /// <summary>
        /// 有效期
        /// </summary>
        public string ValidDate
        {
            set;
            get;
        }

        /// <summary>
        /// 车牌照
        /// </summary>
        public string PlateImg
        {
            set;
            get;
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
