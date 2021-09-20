using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPLibrary.DeailMsg
{
    public class MsgChickIdcard : BaseDeailMsg
    {
        public string Serial
        {
            set;
            get;
        }

        public string Cid
        {
            set;
            get;
        }

        /// <summary>
        /// 身份证开始时间  如果身份证有更新，同样要更新
        /// </summary>
        public string DateStart
        {
            set;
            get;
        }
    }

    public class MsgCheckIdcardBack : BaseDeailMsg
    {
        public bool BNeed
        {
            set;get;
        }

        public string CNum
        {
            set;get;
        }
    }
}
