using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibExchangePlate
{
    public class ConfigWZHXQ
    {
        bool bInited = false;
        public bool BInited
        {
            set { bInited = value; }
            get { return bInited; }
        }
        private static ConfigWZHXQ instance;

        /// <summary>
        /// 相对路径
        /// </summary>
        private static string CONFIG_FILE = @"config\config_plateWZHXQ.ini";
        private static string CONFIG_SECTION = "SETTING_WZHXQ";
        SystemConfigIni _configOper = null;
        /// <summary>
        /// 保存配置文件路径
        /// </summary>
        private string m_szConfigFilePath = string.Empty;


        public static ConfigWZHXQ Single()
        {
            if (instance == null)
            {
                instance = new ConfigWZHXQ();
            }
            return instance;
        }

        /// <summary>
        /// 版本
        /// </summary>
        public string ver { set; get; }

        /// <summary>
        /// 行政区划代码
        /// </summary>
        public string areaCode { set; get; }


        /// <summary>
        /// 经度
        /// </summary>
        public string Longitude
        {
            set;
            get;
        }

        /// <summary>
        /// 纬度 
        /// </summary>
        public string Latitude
        {
            set; get;
        }

        /// <summary>
        /// 设备编号
        /// </summary>
        public string equipmentId { set; get; }


        /// <summary>
        /// 设备名称
        /// </summary>
        public string equipmentName { set; get; }

        /// <summary>
        /// 设备类型 1访客机；2人行闸机；3车辆道闸
        /// </summary>
        public string equipmentType { set; get; }

        /// <summary>
        /// 警务站编号
        /// </summary>
        public string stationId { set; get; }


        /// <summary>
        /// 警务站名称
        /// </summary>
        public string stationName { set; get; }


        /// <summary>
        /// 设备所在位置名称
        /// </summary>
        public string location { set; get; }

        /// <summary>
        /// 小区名称
        /// </summary>
        public string dareaname { set; get; }

        /// <summary>
        /// 小区编号
        /// </summary>
        public string dareacode { set; get; }

        /// <summary>
        /// 场所类别
        /// </summary>
        public string placetype { set; get; }

        /// <summary>
        /// 出入标识 1进入 0出
        /// </summary>
        public string status { set; get; }

        /// <summary>
        /// 平台ip
        /// </summary>
        public string httpIp { set; get; }

        /// <summary>
        /// 平台端口
        /// </summary>
        public string httpPort { set; get; }

        /// <summary>
        /// ftp路径
        /// </summary>
        public string PhotoPath { set; get; }

            
        public int nKeepLiveTick { set; get; }
        public bool InitConfig()
        {
            if (bInited)
                return true;
            string strExeLocation = Assembly.GetExecutingAssembly().Location;
            int nPos = strExeLocation.LastIndexOf('\\');
            strExeLocation = strExeLocation.Substring(0, nPos);
            m_szConfigFilePath = string.Format("{0}\\{1}", strExeLocation, CONFIG_FILE);

            if (_configOper == null)
            {
                _configOper = new SystemConfigIni(m_szConfigFilePath);
            }

            ver = _configOper.InitItem(CONFIG_SECTION, "ver", "2.0");
            Longitude = _configOper.InitItem(CONFIG_SECTION, "Longitude", "111.2345678");
            Latitude = _configOper.InitItem(CONFIG_SECTION, "Latitude", "222.3");
            areaCode = _configOper.InitItem(CONFIG_SECTION, "areaCode", "123456");
            equipmentId = _configOper.InitItem(CONFIG_SECTION, "equipmentId", "11111");
            equipmentName = _configOper.InitItem(CONFIG_SECTION, "equipmentName", "aaa");
            equipmentType = _configOper.InitItem(CONFIG_SECTION, "equipmentType", "1");
            stationId = _configOper.InitItem(CONFIG_SECTION, "stationId", "123");
            stationName = _configOper.InitItem(CONFIG_SECTION, "stationName", "taisau");
            location = _configOper.InitItem(CONFIG_SECTION, "location", "top");
            dareaname = _configOper.InitItem(CONFIG_SECTION, "dareaname", "areaname");
            dareacode = _configOper.InitItem(CONFIG_SECTION, "dareacode", "code");
            placetype = _configOper.InitItem(CONFIG_SECTION, "placetype", "2");
            status = _configOper.InitItem(CONFIG_SECTION, "status", "1");
            httpIp = _configOper.InitItem(CONFIG_SECTION, "httpIp", "218.84.207.99");
            httpPort = _configOper.InitItem(CONFIG_SECTION, "httpPort", "8888");
            nKeepLiveTick = _configOper.InitItem(CONFIG_SECTION, "keepLive", 10);       //默认10s

            bInited = true;
            return bInited;
        }

    }
}
