using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPLibrary.DeailMsg
{
    public class MsgDownConfig : BaseDeailMsg
    {
        public string Channel
        {
            set;
            get;
        }

        /// <summary>
        /// 设备名
        /// </summary>
        public string DeviceName
        {
            set;
            get;
        }

        /// <summary>
        /// 安装地址
        /// </summary>
        public string InstallSite
        {
            set;
            get;
        }

        public int AutoStart
        {
            set;
            get;
        }

        public string DetectScoreLow
        {
            set;
            get;
        }

        public string DetectScoreHigh
        {
            set;
            get;
        }

        public string LocalCard
        {
            set;
            get;
        }

        /// <summary>
        /// 待机界面超时时间 单位秒
        /// </summary>
        public int CacheTime
        {
            set;
            get;
        }

        /// <summary>
        /// 本地缓存 天数
        /// </summary>
        public int SaveDays
        {
            set;
            get;
        }

        /// <summary>
        /// 本地缓存 目录
        /// </summary>
        public string SavePath
        {
            set;
            get;
        }
    }

    public class MsgDownConfigBack : BaseDeailMsg
    {
        public string Serial
        {
            set;
            get;
        }

        public string StrReturn
        {
            set;
            get;
        }
    }
    
}
