using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using TCPLibrary.DeailMsg;
using System.Configuration;
namespace TS_IDCheck
{
    /// <summary>
    /// 配置操作类 单例形式调用  可获取 可设置
    /// </summary>
    public class ConfigOperator
    {
        bool bInited = false;
        public bool BInited
        {
            set { bInited = value; }
            get { return bInited; }
        }
        private static ConfigOperator instance;

        //private static string CONFIG_FILE = @"config\SystemConfig.xml";
        /// <summary>
        /// 相对路径
        /// </summary>
        private static string CONFIG_FILE = @"config\systemconfig.ini";
        private static string CONFIG_SECTION = "SETTING_SYSTEM";
        private static string CONFIG_PLATE = "SETTING_PLATE";
        SystemConfigIni _configOper = null;


        /// <summary>
        /// 保存配置文件路径
        /// </summary>
        private string m_szConfigFilePath = string.Empty;


        public static ConfigOperator Single()
        {
            if (instance == null)
            {
                instance = new ConfigOperator();
            }
            return instance;
        }



        /// <summary>
        /// 检测等级 0不检测 1超时、断网不检测 2严格检测
        /// </summary>
        public int StrictGrade { set; get; }
        /// <summary>
        /// 继电器端口 COM1 格式填写
        /// </summary>
        public string StrRelaySerial { set; get; }
        public string DeviceRegisterUrl { set; get; }
        public string DeviceRegisterUrlArmy { set; get; }

        /// <summary>
        /// 黑名单查找 默认公安网：http://10.20.50.110:9092/verificationInterface/passlog/blackList
        /// </summary>
        public string QueryBlackUrlPolice { set; get; }

        public string QueryBlackUrlVedio { set; get; }
        public string QueryBlackUrlNet { set; get; }

        /// <summary>
        /// 黑名单查找 兵团网:http://10.84.2.174:9092/verificationInterface/passlog/blackList
        /// </summary>
        public string QueryBlackUrlArmy { set; get; }

        /// <summary>
        /// 
        /// </summary>
        public string UploadPersonUrlPolice { set; get; }
        public string UploadPersonUrlVedio { set; get; }
        public string UploadPersonUrlNet { set; get; }

        public string UploadPersonUrlArmy { set; get; }

        public string UploadFileUrlPolice { set; get; }
        public string UploadFileUrlVedio { set; get; }
        public string UploadFileUrlNet { set; get; }
        public string UploadFileUrlArmy { set; get; }
        /// <summary>
        /// 是否需要获取票务信息
        /// </summary>
        public int GetTicketInfo { get; set; }

        public string GetTicketUrl { set; get; }

        /// <summary>
        /// 文件保存路径
        /// </summary>
        public string StrSavePath { set; get; }

        /// <summary>
        /// 文件保存时间
        /// </summary>
        public string StrSaveDays { set; get; }

        public string LocalCard { set; get; }

        /// <summary>
        /// 平台设备编号
        /// </summary>
        public string StrThreeDeviceCode { set; get; }

        /// <summary>
        /// 平台token
        /// </summary>
        public string StrThreeToken { set; get; }

        /// <summary>
        /// 平台有效时间
        /// </summary>
        public string StrThreeAvilid { set; get; }

        /// <summary>
        /// 平台License
        /// </summary>
        public string StrThreeLicense { set; get; }

        /// <summary>
        /// 二级平台设备编号
        /// </summary>
        public string StrThreeDeviceCodeT { set; get; }

        /// <summary>
        /// 二级平台License
        /// </summary>
        public string StrThreeLicenseT { set; get; }

        /// <summary>
        /// 二级平台token
        /// </summary>
        public string StrThreeTokenT { set; get; }

        /// <summary>
        /// 二级平台有效时间
        /// </summary>
        public string StrThreeAvilidT { set; get; }

        /// <summary>
        /// 平台选择 乌鲁木齐智慧小区="WZHXQ" 阿拉山="ALS"
        /// </summary>
        public string ExchangePlateChoose
        {
            set; get;
        }

        /*平台共用信息*/

        public string StrThreeLatitude { set; get; }
        public string StrThreeLongitude { set; get; }


        //public string BAutoStart { set; get; }

        /// <summary>
        /// 比对分值
        /// </summary>
        public string FDetectScore { set; get; }


        public string NInOut { set; get; }

        /// <summary>
        /// 本地设备号
        /// </summary>
        public string StrSerialNum { set; get; }

        public string StrfileUploadCnt { set; get; }

        /// <summary>
        /// 平台选择
        /// </summary>
        public int PlateChoose { set; get; }


        public int nDoubleScreenX { set; get; }
        public int nDoubleScreenY { set; get; }

        public int DoorCloseDelay { set; get; }

        public int nIsDoubleScreen { set; get; }

        /// <summary>
        /// 刷卡类型：0 身份证 ， 1 ic卡 ， 2 护照。
        /// </summary>
        public string CardReaderBrand { set; get; }

        /// <summary>
        /// 视频旋转角度 0 不处理 1：顺时针旋转90度 2：倒转 3：顺时针旋转270度
        /// </summary>
        public int RotateFlip { set; get; }

        /// <summary>
        /// 保存配置文件路径
        /// </summary>
        private string _szConfigFilePath = "";


        // private int 


        #region 读写配置操作
        private string GetDefaultValue(string szKeyName, string defauleValue)
        {
            return SystemConfig.GetConfigData(_szConfigFilePath, szKeyName, defauleValue);
        }

        private int GetDefaultValue(string szKeyName, int defauleValue)
        {
            return SystemConfig.GetConfigData(_szConfigFilePath, szKeyName, defauleValue);
        }

        private float GetDefaultValue(string szKeyName, float defauleValue)
        {
            return SystemConfig.GetConfigData(_szConfigFilePath, szKeyName, defauleValue);
        }

        private bool GetDefaultValue(string szKeyName, bool defauleValue)
        {
            return SystemConfig.GetConfigData(_szConfigFilePath, szKeyName, defauleValue);
        }

        private bool SetConfigValue(string szKeyName, string szValue)
        {
            return SystemConfig.WriteConfigData(_szConfigFilePath, szKeyName, szValue);
        }
        #endregion

        public bool InitConfig()
        {
            if (bInited) return true;
            m_szConfigFilePath = string.Format("{0}\\{1}", SystemConfig.GetWorkDirectory(), CONFIG_FILE);
            if (_configOper == null)
            {
                _configOper = new SystemConfigIni(m_szConfigFilePath);
            }

            //if (bInited)
            //    return true;

            //Trace.WriteLine("create config");
            //_szConfigFilePath = string.Format("{0}\\{1}", SystemConfig.GetWorkDirectory(), CONFIG_FILE);
            StrictGrade = _configOper.InitItem(CONFIG_SECTION, nameof(StrictGrade), 1);
            StrRelaySerial = _configOper.InitItem(CONFIG_SECTION, "relay_serial", "COM1");
            StrSavePath = _configOper.InitItem(CONFIG_SECTION, "save_path", @"d:\saves");
            StrSaveDays = _configOper.InitItem(CONFIG_SECTION, "save_days", "30");
            LocalCard = _configOper.InitItem(CONFIG_SECTION, "local_card", "");
            StrThreeDeviceCode = _configOper.InitItem(CONFIG_SECTION, "devicecode", "660000FL00100098");
            StrThreeLicense = _configOper.InitItem(CONFIG_SECTION, "threeLicense", "20170621660000FL001660000FL00100098");
            StrThreeToken = _configOper.InitItem(CONFIG_SECTION, "threetoken", "");
            StrThreeAvilid = _configOper.InitItem(CONFIG_SECTION, "threeavilid", "");
            StrThreeDeviceCodeT = _configOper.InitItem(CONFIG_SECTION, "devicecodeT", "");
            StrThreeLicenseT = _configOper.InitItem(CONFIG_SECTION, "threelicenseT", "");
            StrThreeTokenT = _configOper.InitItem(CONFIG_SECTION, "threetokenT", "");
            StrThreeAvilidT = _configOper.InitItem(CONFIG_SECTION, "threeavilidT", "");
            StrThreeLatitude = _configOper.InitItem(CONFIG_SECTION, "threeLatitude", "111.0");
            StrThreeLongitude = _configOper.InitItem(CONFIG_SECTION, "threeLongitude", "112.0");
            //BAutoStart = GetDefaultValue("auto_start", "1");
            FDetectScore = _configOper.InitItem(CONFIG_SECTION, "face_detect_score", "58.0");
            NInOut = _configOper.InitItem(CONFIG_SECTION, "in_or_out", "1");
            StrSerialNum = _configOper.InitItem(CONFIG_SECTION, "serial_num", "ts0001");
            StrfileUploadCnt = _configOper.InitItem(CONFIG_SECTION, "upload_file", "d:\\saves");
            PlateChoose = Convert.ToInt32(_configOper.InitItem(CONFIG_SECTION, "plate_choose", "3"));
            nDoubleScreenX = Convert.ToInt32(_configOper.InitItem(CONFIG_SECTION, "double_width", "768"));
            nDoubleScreenY = Convert.ToInt32(_configOper.InitItem(CONFIG_SECTION, "double_height", "1024"));
            DoorCloseDelay = Convert.ToInt32(_configOper.InitItem(CONFIG_SECTION, "DoorCloseDelay", "3000"));
            nIsDoubleScreen = Convert.ToInt32(_configOper.InitItem(CONFIG_SECTION, "double_screen", "1"));
            CardReaderBrand = _configOper.InitItem(CONFIG_SECTION, "CardReaderBrand", "1");//刷卡类型（默认身份证）
            RotateFlip = Convert.ToInt32(_configOper.InitItem(CONFIG_SECTION, "RotateFlip", 0));
            ExchangePlateChoose = _configOper.InitItem(CONFIG_SECTION, nameof(ExchangePlateChoose), "");
            //注册
            DeviceRegisterUrl = _configOper.InitItem(CONFIG_PLATE, nameof(DeviceRegisterUrl), "http://10.20.50.110:9094/DeviceManageService/ACSServer.html");
            DeviceRegisterUrlArmy = _configOper.InitItem(CONFIG_PLATE, nameof(DeviceRegisterUrlArmy), "http://10.20.50.110:9094/DeviceManageService/ACSServer.html");
            //黑名单查找
            QueryBlackUrlPolice = _configOper.InitItem(CONFIG_PLATE, nameof(QueryBlackUrlPolice), "http://10.20.50.110:9092/verificationInterface/passlog/blackList");
            QueryBlackUrlVedio = _configOper.InitItem(CONFIG_PLATE, nameof(QueryBlackUrlVedio), "http://21.0.0.125:9092/verificationInterface/passlog/blackList");
            QueryBlackUrlNet = _configOper.InitItem(CONFIG_PLATE, nameof(QueryBlackUrlNet), "http://124.117.209.133:29092/verificationInterface/passlog/blackList");
            QueryBlackUrlArmy = _configOper.InitItem(CONFIG_PLATE, nameof(QueryBlackUrlArmy), "http://10.84.2.174:9092/verificationInterface/passlog/blackList");

            UploadPersonUrlPolice = _configOper.InitItem(CONFIG_PLATE, nameof(UploadPersonUrlPolice), "http://10.20.50.110:9092/verificationInterface/passlog/personLog");
            UploadPersonUrlVedio = _configOper.InitItem(CONFIG_PLATE, nameof(UploadPersonUrlVedio), "http://21.0.0.125:9092/verificationInterface/passlog/personLog");
            UploadPersonUrlNet = _configOper.InitItem(CONFIG_PLATE, nameof(UploadPersonUrlNet), "http://124.117.209.133:29092/verificationInterface/passlog/personLog");
            UploadPersonUrlArmy = _configOper.InitItem(CONFIG_PLATE, nameof(UploadPersonUrlArmy), "http://10.84.2.174:9092/verificationInterface/passlog/personLog");

            UploadFileUrlPolice = _configOper.InitItem(CONFIG_PLATE, nameof(UploadFileUrlPolice), "http://10.20.50.110:9095/FileStorageService/FileUpload");
            UploadFileUrlVedio = _configOper.InitItem(CONFIG_PLATE, nameof(UploadFileUrlVedio), "http://21.0.0.125:9095/FileStorageService/FileUpload");
            UploadFileUrlNet = _configOper.InitItem(CONFIG_PLATE, nameof(UploadFileUrlNet), "http://124.117.209.133:29095/FileStorageService/FileUpload");
            UploadFileUrlArmy = _configOper.InitItem(CONFIG_PLATE, nameof(UploadFileUrlArmy), "http://10.84.2.174:9095/FileStorageService/FileUpload");


            GetTicketInfo = _configOper.InitItem(CONFIG_SECTION, nameof(GetTicketInfo), 0);
            GetTicketUrl = _configOper.InitItem(CONFIG_SECTION, nameof(GetTicketUrl), "http://v6dutm.natappfree.cc/klmy?wsdl");
            bInited = true;
            return bInited;
        }

        public void SetRegistInfo(int nPlate, string token, string strVil)
        {
            if (nPlate == 1)
            {
                StrThreeToken = token;
                StrThreeAvilid = strVil;

                _configOper.SetConfigItem(CONFIG_SECTION, "threetoken", token);
                _configOper.SetConfigItem(CONFIG_SECTION, "StrThreeAvilid", strVil);

            }
            else if (nPlate == 2)
            {
                StrThreeAvilidT = strVil;
                StrThreeTokenT = token;
                _configOper.SetConfigItem(CONFIG_SECTION, "threetokenT", token);
                _configOper.SetConfigItem(CONFIG_SECTION, "threeavilidT", strVil);
            }
        }

    }
}
