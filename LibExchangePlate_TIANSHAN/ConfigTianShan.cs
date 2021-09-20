using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LibExchangePlate
{
    public class ConfigTianShan
    {
        bool bInited = false;
        public bool Binited { set { bInited = value; } get { return bInited; } }

        private static ConfigTianShan instatnce;

        private static string CONFIG_FILE = @"config\config_plateTIANSHAN.ini";
        private static string CONFIG_SECTION = "SETTING_TIANSHAN";
        SystemConfigIni _configOper = null;

        /// <summary>
        /// 保存配置文件路径
        /// </summary>
        private string m_szConfigFilePath = string.Empty;

        public static ConfigTianShan Single()
        {
            if (instatnce == null)
            {
                instatnce = new ConfigTianShan();
            }
            return instatnce;
        }

        public string DevImport { set; get; }
        public string DevType { set; get; }
        public string DevAddress { set; get; }
        public string DevName { set; get; }
        public string DevId { set; get; }
        public string DevSN { set; get; }

        public string CustomID { set; get; }
        public string CustomPwd { set; get; }

        public string WebsiteAddress { set; get; }

        /// <summary>
        /// 0不加密 1加密
        /// </summary>
        public string Encryption { set; get; }

        /// <summary>
        /// 加密密钥
        /// </summary>
        public string EncryptKey { set; get; }

        /// <summary>
        /// encry 为 1 时必填 否则为空
        /// </summary>
        public string Appkey { set; get; }

        /// <summary>
        /// 关联的车牌设备id
        /// </summary>
        public string PlateDevice { set; get; }

        /// <summary>
        /// 设备唯一码
        /// </summary>
        public string SerialNum { set; get; }  
        
        /// <summary>
        /// 版本  
        /// </summary>
        public string Version { set; get; }    

        public int KeepHeartTick { set; get; }
        

        #region 读写配置操作
        //private string GetDefaultValue(string szKeyName, string defauleValue)
        //{
        //    return SystemConfig.GetConfigData(m_szConfigFilePath, szKeyName, defauleValue);
        //}

        //private int GetDefaultValue(string szKeyName, int defauleValue)
        //{
        //    return SystemConfig.GetConfigData(m_szConfigFilePath, szKeyName, defauleValue);
        //}

        //private float GetDefaultValue(string szKeyName, float defauleValue)
        //{
        //    return SystemConfig.GetConfigData(m_szConfigFilePath, szKeyName, defauleValue);
        //}

        //private bool GetDefaultValue(string szKeyName, bool defauleValue)
        //{
        //    return SystemConfig.GetConfigData(m_szConfigFilePath, szKeyName, defauleValue);
        //}


        //private bool SetConfigValue(string szKeyName, string szValue)
        //{
        //    return SystemConfig.WriteConfigData(m_szConfigFilePath, szKeyName, szValue);
        //}
        #endregion

        public bool InitConfig()
        {
            if (bInited) return true;
            string strExeLocation = Assembly.GetExecutingAssembly().Location;
            int nPos = strExeLocation.LastIndexOf('\\');
            strExeLocation = strExeLocation.Substring(0, nPos);
            m_szConfigFilePath = string.Format("{0}\\{1}", strExeLocation, CONFIG_FILE);
            if (_configOper == null)
            {
                _configOper = new SystemConfigIni(m_szConfigFilePath);
            }
            
            DevImport = _configOper.InitItem(CONFIG_SECTION, "DevImport", "1");
            DevType = _configOper.InitItem(CONFIG_SECTION, "DevType", "01");
            DevName = _configOper.InitItem(CONFIG_SECTION, "DevName", "设备01");
            DevId = _configOper.InitItem(CONFIG_SECTION, "DevId", "541xx");
            DevSN = _configOper.InitItem(CONFIG_SECTION, "DevSN", "test1");
            DevAddress = _configOper.InitItem(CONFIG_SECTION, "DevAddress", "胜利小区东门口");
            CustomID = _configOper.InitItem(CONFIG_SECTION, "CustomID", "123456");
            CustomPwd = _configOper.InitItem(CONFIG_SECTION, "CustomPwd", "6523011202");
            WebsiteAddress = _configOper.InitItem(CONFIG_SECTION, "WebsiteAddress", "220.171.84.74:72");
            
            Encryption = _configOper.InitItem(CONFIG_SECTION, "Encryption", "1");
            EncryptKey = _configOper.InitItem(CONFIG_SECTION, "EncryptKey", "B20FJRH8");
            Appkey = _configOper.InitItem(CONFIG_SECTION, "Appkey", "test");
            PlateDevice = _configOper.InitItem(CONFIG_SECTION, "PlateDevice", "123456");
            SerialNum = _configOper.InitItem(CONFIG_SECTION, "SerialNum", "65010200100100101");
            Version = _configOper.InitItem(CONFIG_SECTION, "Version", "V1.1");
            KeepHeartTick = _configOper.InitItem(CONFIG_SECTION, nameof(KeepHeartTick), 10);
            bInited = true;
            return bInited;
        }

    }
}
