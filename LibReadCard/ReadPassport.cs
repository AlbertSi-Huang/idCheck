using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;

/***********************
 * 功能：护照读取
 * 作者：刘飞翔
 * 时间：2018-4-25
 * 
 * 
 * *********************/

namespace LibReadCard
{

    public class ReadPassport : IReadCard
    {
        #region 字段定义
        private string m_sIpUserID = "";//用户ID，由文通提供
        private bool m_bIsIDCardLoaded = false;//IDCard载入成功与否标志
        private const int nType = 13;//证件类型，13为护照

        private int nDGGroup=0;//表示要读取的DG，对于普通设备调用传0
        private bool bRecogVIZ = true;//true表示识别视读区相关类容，false则相反
        private int nSaveImageType = 0;//保存图片类型：1表示保存白光全图,2表示保存红外全图,4表示保存紫外全图,8表示保存版面头像,16 表示保存芯片的头像
        private PassportInfo cPassport = new PassportInfo();
        //int i = 1 | 2 | 3;

        /// <summary>
        /// 循环读卡线程
        /// </summary>
        private Thread m_ThreadHandle = null;

        /// <summary>
        /// 线程运行状态
        /// </summary>
        private bool m_ThreadRunStatus = false;

        /// <summary>
        /// 确认是否需要再次读护照
        /// </summary>
        private bool m_isNeedCapture = true;
        /// <summary>
        /// 是否打开机器
        /// </summary>
        private bool m_isOpened = false;

        private bool m_isPassportRead = false;//护照读取成功

        private String strRunPath = Application.StartupPath + "\\images";//图片保存路径文件

        private Bitmap Photo = null;//护照全照
        private Bitmap HeadPhoto = null;//护照头像
        
        //private FileOperator fileOper;

        public bool isNeedCapture
        {
            get { return m_isNeedCapture; }
            set { m_isNeedCapture = value; }
        }

        #endregion

        #region DLL引用
        [DllImport("kernel32")]
        public static extern int LoadLibrary(string strDllName);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int InitIDCard(char[] cArrUserID, int nType, char[] cArrDirectory);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetRecogResult(int nIndex, char[] cArrBuffer, ref int nBufferLen);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogIDCard();

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetFieldName(int nIndex, char[] cArrBuffer, ref int nBufferLen);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int AcquireImage(int nCardType);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SaveImage(char[] cArrFileName);
        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SaveHeadImage(char[] cArrFileName);


        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetCurrentDevice(char[] cArrDeviceName, int nLength);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern void GetVersionInfo(char[] cArrVersion, int nLength);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CheckDeviceOnline();

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool SetAcquireImageType(int nLightType, int nImageType);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool SetUserDefinedImageSize(int nWidth, int nHeight);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool SetAcquireImageResolution(int nResolutionX, int nResolutionY);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SetIDCardID(int nMainID, int[] nSubID, int nSubIdCount);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int AddIDCardID(int nMainID, int[] nSubID, int nSubIdCount);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogIDCardEX(int nMainID, int nSubID);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetButtonDownType();

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetGrabSignalType();

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SetSpecialAttribute(int nType, int nSet);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern void FreeIDCard();
        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetDeviceSN(char[] cArrSn, int nLength);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBusinessCardResult(int nID, int nIndex, char[] cArrBuffer, ref int nBufferLen);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogBusinessCard(int nType);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBusinessCardFieldName(int nID, char[] cArrBuffer, ref int nBufferLen);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBusinessCardResultCount(int nID);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int LoadImageToMemory(string path, int nType);

        [DllImport("IDCard.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int ClassifyIDCard(ref int nCardType);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogChipCard(int nDGGroup, bool bRecogVIZ, int nSaveImageType);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogGeneralMRZCard(bool bRecogVIZ, int nSaveImageType);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogCommonCard(int nSaveImageType);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SaveImageEx(char[] lpFileName, int nType);

        [DllImport("IDCard", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetDataGroupContent(int nDGIndex, bool bRawData, byte[] lpBuffer, ref int len);
        #endregion

        #region 构造函数
        /// <summary>
        /// 缺省构造
        /// </summary>
        public ReadPassport()
        {
            m_ThreadHandle = new Thread(AutoClassAndRecognize);
            m_ThreadHandle.IsBackground = true;
        }

        /// <summary>
        /// 构造重载
        /// </summary>
        /// <param name="sIpUserID">用户ID</param>
        public ReadPassport(string sIpUserID)
        {
            try
            {
                m_isNeedCapture = true;
                m_ThreadHandle = new Thread(ReadCard);
                m_ThreadHandle.IsBackground = true;

                this.m_sIpUserID = sIpUserID;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("实例化 ReadPassport异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 构造重载
        /// </summary>
        /// <param name="sIpUserID"></param>
        /// <param name="nDG"></param>
        public ReadPassport(string sIpUserID, string sDG)
        {
            try
            {
                m_isNeedCapture = true;
                //fileOper = new FileOperator();
                m_ThreadHandle = new Thread(ReadCard);
                m_ThreadHandle.IsBackground = true;

                string[] sArray = sDG.Split('|');
                foreach (string i in sArray)
                {
                    this.nDGGroup |= (1 << (int.Parse(i) - 1));
                }

                this.m_sIpUserID = sIpUserID;
                this.m_sIpUserID = sIpUserID;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("实例化 ReadPassport异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 构造重载
        /// </summary>
        /// <param name="sIpUserID"></param>
        /// <param name="nDG"></param>
        /// <param name="bVIZ"></param>
        public ReadPassport(string sIpUserID, string sDG, bool bVIZ)
        {
            try
            {
                m_isNeedCapture = true;
                //fileOper = new FileOperator();
                m_ThreadHandle = new Thread(ReadCard);
                m_ThreadHandle.IsBackground = true;

                string[] sArray = sDG.Split('|');
                foreach (string i in sArray)
                {
                    this.nDGGroup |= (1 << (int.Parse(i) - 1));
                }
                this.bRecogVIZ = bVIZ;
                this.m_sIpUserID = sIpUserID;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("实例化 ReadPassport异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 构造重载
        /// </summary>
        /// <param name="sIpUserID"></param>
        /// <param name="nDG"></param>
        /// <param name="bVIZ"></param>
        /// <param name="nSaveImage"></param>
        public ReadPassport(string sIpUserID, string sDG, bool bVIZ, string sSaveImage)
        {
            try
            {
                m_isNeedCapture = true;
                //fileOper = new FileOperator();
                m_ThreadHandle = new Thread(ReadCard);
                m_ThreadHandle.IsBackground = true;

                string[] sArray = sDG.Split('|');
                foreach (string i in sArray)
                {
                    this.nDGGroup |= (1 << (int.Parse(i) - 1));
                }
                string[] sArrayImage = sSaveImage.Split('|');
                foreach (string i in sArray)
                {
                    this.nSaveImageType |= int.Parse(i);
                }
                this.bRecogVIZ = bVIZ;
                this.m_sIpUserID = sIpUserID;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("实例化 ReadPassport异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 构造重载
        /// </summary>
        /// <param name="nDG"></param>
        /// <param name="bVIZ"></param>
        /// <param name="nSaveImage"></param>
        public ReadPassport(string sDG, bool bVIZ, string sSaveImage)
        {
            try
            {
                m_isNeedCapture = true;
                //fileOper = new FileOperator();
                m_ThreadHandle = new Thread(ReadCard);
                m_ThreadHandle.IsBackground = true;

                string[] sArray = sDG.Split('|');
                foreach (string i in sArray)
                {
                    this.nDGGroup |= (1 << (int.Parse(i) - 1));
                }
                string[] sArrayImage = sSaveImage.Split('|');
                foreach (string i in sArray)
                {
                    this.nSaveImageType |= int.Parse(i);
                }
                this.bRecogVIZ = bVIZ;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("实例化 ReadPassport异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 拷贝重载
        /// </summary>
        /// <param name="cReadPassPort">重载对象</param>
        public ReadPassport(ReadPassport cReadPassPort)
        {
            this.m_sIpUserID = cReadPassPort.m_sIpUserID;
            this.m_bIsIDCardLoaded = cReadPassPort.m_bIsIDCardLoaded;
            m_ThreadHandle = new Thread(ReadCard);
            m_ThreadHandle.IsBackground = true;
        }

        #endregion

        #region 创建实例

        #endregion

        #region 字段设置与读取
        /// <summary>
        /// 设置要读取的DG
        /// </summary>
        /// <param name="nDG">nDG的取值组合：例如：1|2|3|4</param>
        public void SetDGGroup(string sDG)
        {
            try
            {
                string[] sArray = sDG.Split('|');
                foreach (string i in sArray)
                {
                    this.nDGGroup |= (1 << (int.Parse(i) - 1));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 设置要读取的RecogVIZ
        /// </summary>
        /// <param name="bVIZ"></param>
        public void SetRecogVIZ(bool bVIZ)
        {
            this.bRecogVIZ = bVIZ;
        }
        /// <summary>
        /// 设置要读取的SaveImageType
        /// </summary>
        /// <param name="sSaveImage">取值组合：例如：1|2|3|4</param>
        public void SetSaveImageType(string sSaveImage)
        {
            try
            {
                string[] sArray = sSaveImage.Split('|');
                foreach (string i in sArray)
                {
                    this.nSaveImageType |= int.Parse(i);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 设置用户ID
        /// </summary>
        /// <param name="sIpUserID"></param>
        public  void SetUserID(string sIpUserID)
        {
            this.m_sIpUserID = sIpUserID;
        }

        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <returns></returns>
        public  string GetUserID()
        {
            return this.m_sIpUserID;
        }

        /// <summary>
        /// 获取当前载入状态（是否成功）
        /// </summary>
        /// <returns></returns>
        public bool GetIsIDCardLoaded()
        {
            return this.m_bIsIDCardLoaded;
        }

        public PassportInfo GetPassportInfo()
        {
            return cPassport;
        }

        #endregion

        /// <summary>
        /// 是否打开
        /// </summary>
        public bool IsOpened
        {
            set { m_isOpened = value; }
            get { return m_isOpened; }
        }

        /// <summary>
        /// 是否需要读卡
        /// </summary>
        public  bool IsNeedCapture
        {
            get { return m_isNeedCapture; }
            set { m_isNeedCapture = value; }
        }

        /// <summary>
        /// 读卡器名称
        /// </summary>
        public  string Name { set; get; }

        /// <summary>
        /// 完成读卡事件
        /// </summary>
        public event ReadCardDelegate OnReadCardEvent;

        #region 读卡器初始化
        /// <summary>
        /// 读卡器初始化
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            //如片保存路径创建
            checkDir(this.strRunPath);
            InitPassPort();
            return this.m_bIsIDCardLoaded;
        }

        /// <summary>
        /// 初始化护照读卡器
        /// </summary>
        /// <returns></returns>
        public string InitPassPort()
        {
            try
            {
                string Result = "";
                //检查Dll是否已经载入
                if (m_bIsIDCardLoaded)
                {
                    Result = "初始化成功";
                    return Result;
                }

                #region 载入DLL
                int nRet;
                nRet = LoadLibrary("IDCard");
                if (nRet == 0)
                {
                    Result = "IDCard.Dll加载失败";
                    return Result;
                }
                #endregion

                //Load engine
                
                //获取用户ID
                if (this.m_sIpUserID.ToString() == "")
                {
                    InitUserID();
                }

                nRet = InitIDCard(this.m_sIpUserID.ToCharArray(), 1, null);
                if (nRet != 0)
                {
                    if (nRet == 1)
                    {
                        Result = "用户ID错误";
                    }
                    if (nRet == 2)
                    {
                        Result = "设备初始化失败";
                    }
                    if (nRet == 3)
                    {
                        Result = "初始化核心失败";
                    }
                    if (nRet == 4)
                    {
                        Result = "未找到授权文件";
                    }

                    return Result;
                }

                SetSpecialAttribute(1, 1);
                m_bIsIDCardLoaded = true;
                m_isPassportRead = false;
                Result = "初始化成功";

                return Result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        /// <summary>
        /// 循环读卡
        /// </summary>
        public void ReadCard()
        {
            int nRet;
            //成功连接
            if (m_bIsIDCardLoaded)
            {
                try
                {
                    #region 设置当前要识别的证件类型，并将之前已经设置的证件类型清除掉
                    int[] nSubID = new int[1];
                    nSubID[0] = 0;
                    nRet = SetIDCardID(nType, nSubID, 1);//返回0代表成功，其他失败
                    #endregion

                    #region 证件分类：对即将识别的证件自动分类
                    int ncardType = 0;
                    nRet = ClassifyIDCard(ref ncardType);
                    //if (nRet == -1)//没有设置有效证件类型
                    //{

                    //}
                    //if (nRet == -4)//没有加载图像
                    //{

                    //}
                    //if (nRet == -5)//分类失败，没找到匹配模板
                    //{

                    //}
                    //if (nRet == -6)//因拒识造成的分类失败
                    //{

                    //}
                    //if (nRet == -1002)//未设置识别证件类型
                    //{

                    //}
                    //if (nRet == -1005)//图像采集失败
                    //{

                    //}
                    #endregion

                    #region 证件识别
                    if (ncardType == 1)//芯片卡
                    {
                        nRet = RecogChipCard(nDGGroup, bRecogVIZ, nSaveImageType);
                    }

                    if (ncardType == 2)//普通MRZ证件
                    {
                        nRet = RecogGeneralMRZCard(bRecogVIZ, nSaveImageType);
                    }

                    if (ncardType == 3)//不含有MRZ的普通证件
                    {
                        nRet = RecogCommonCard(nSaveImageType);
                    }

                    if (nRet < 0)//识别失败
                    {
                        
                        return;
                    }

                    #endregion

                    #region //识别成功,取得识别结果
                    int MAX_CH_NUM = 128;
                    char[] cArrFieldValue = new char[MAX_CH_NUM];
                    if (cPassport != null)
                    {
                        for (int i = 0; ; i++)
                        {

                            nRet = GetRecogResult(i, cArrFieldValue, ref MAX_CH_NUM);
                            if (nRet == 3)
                            {
                                break;
                            }
                            string sFieldValue = new string(cArrFieldValue);
                            sFieldValue = sFieldValue.Substring(0, sFieldValue.IndexOf('\0'));
                            #region cPassport对象赋值
                            switch (i)
                            {
                                case 0:
                                    cPassport.Type = sFieldValue;
                                    continue;
                                case 1:
                                    cPassport.MRZ = sFieldValue;
                                    continue;
                                case 2:
                                    cPassport.Name = sFieldValue;
                                    continue;
                                case 3:
                                    cPassport.EnglishName = sFieldValue;
                                    continue;
                                case 4:
                                    cPassport.Sex = sFieldValue;
                                    continue;
                                case 5:
                                    cPassport.DateOfBirth = sFieldValue;
                                    continue;
                                case 6:
                                    cPassport.DateOfExpiry = sFieldValue;
                                    continue;
                                case 7:
                                    cPassport.CountryCode = sFieldValue;
                                    continue;
                                case 8:
                                    cPassport.EnglishFamilyName = sFieldValue;
                                    continue;
                                case 9:
                                    cPassport.EnglishGivienName = sFieldValue;
                                    continue;
                                case 10:
                                    cPassport.MRZ1 = sFieldValue;
                                    continue;
                                case 11:
                                    cPassport.MRZ2 = sFieldValue;
                                    continue;
                                case 12:
                                    cPassport.Nationality = sFieldValue;
                                    continue;
                                case 13:
                                    cPassport.PassportNo = sFieldValue;
                                    continue;
                                case 14:
                                    cPassport.PlaceOfBirth = sFieldValue;
                                    continue;
                                case 15:
                                    cPassport.PlaceOfIssue = sFieldValue;
                                    continue;
                                case 16:
                                    cPassport.DateOfIssue = sFieldValue;
                                    continue;
                                case 17:
                                    cPassport.RFIDMRZ = sFieldValue;
                                    continue;
                                case 18:
                                    cPassport.OCRMRZ = sFieldValue;
                                    continue;
                                case 19:
                                    cPassport.PlaceOfBirthPinyin = sFieldValue;
                                    continue;
                                case 20:
                                    cPassport.PlaceOfIssuePinyin = sFieldValue;
                                    continue;
                                case 21:
                                    cPassport.PersonalIdNo = sFieldValue;
                                    continue;
                                case 22:
                                    cPassport.NamePinyinOCR = sFieldValue;
                                    continue;
                                case 23:
                                    cPassport.SexOCR = sFieldValue;
                                    continue;
                                case 24:
                                    cPassport.NationalityOCR = sFieldValue;
                                    continue;
                                case 25:
                                    cPassport.PersonalIdNoOCR = sFieldValue;
                                    continue;
                                case 26:
                                    cPassport.PlaceOfBirthOCR = sFieldValue;
                                    continue;
                                case 27:
                                    cPassport.DateOfExpiryOCR = sFieldValue;
                                    continue;
                                case 28:
                                    cPassport.AuthorityOCR = sFieldValue;
                                    continue;
                                case 29:
                                    cPassport.FamilyName = sFieldValue;
                                    continue;
                                case 30:
                                    cPassport.GivienName = sFieldValue;
                                    continue;
                                default:
                                    break;
                            }
                            #endregion
                        }
                    }
                    #endregion

                    m_isPassportRead = true;//读卡成功标志

                    #region 保存图片
                    String strImgPath = strRunPath + "\\" + cPassport.MRZ.Trim().ToString() + ".bmp";
                    char[] carrImgPath = strImgPath.ToCharArray();
                    nRet = SaveImageEx(carrImgPath, nSaveImageType);//
                    //SaveHeadImage("./Text.bmp".ToCharArray());//保存
                    //SaveImages();
                    if (nRet == 0)//图片保存成功
                    {
                        cPassport.Photo = strRunPath + "\\" + cPassport.MRZ.Trim().ToString() + ".bmp";
                        cPassport.PhotoHead = strRunPath + "\\" + cPassport.MRZ.Trim().ToString() + "Head.bmp";
                    }
                    #endregion

                    #region 读取护照信息后触发事件
                    if (OnReadCardEvent != null)
                    {
                        OnReadCardEvent(cPassport, 2);
                    }
                    #endregion

                    #region 通过TCP发送读取到的护照信息
                    UploadPassportInfo();
                    #endregion

                    #region 保存护照信息到本地硬盘
                    SaveReadPassPort();
                    #endregion

                    #region 定时器自动读取护照信息
                    StartRun();//开始线程循环读取
                    #endregion
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 关闭读卡
        /// </summary>
        public void Close()
        {
            try
            {
                if (m_bIsIDCardLoaded)
                {
                    StopRun();//停止线程
                    FreeIDCard();
                    m_bIsIDCardLoaded = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 初始化用户id
        /// </summary>
        private void InitUserID()
        {
            PassportUserID.CreateInstance();
            this.m_sIpUserID = PassportUserID.GetUserID();
        }

        /// <summary>
        /// 读卡
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoClassAndRecognize()
        {
            if (!m_bIsIDCardLoaded)
            {
                return;
            }
            int nRet = GetGrabSignalType();//判断是否采集信号触发：0-没有，1-有
            if (nRet == 1)
            {
                #region 设置当前要识别的证件类型，并将之前已经设置的证件类型清除掉

                int[] nSubID = new int[1];
                nSubID[0] = 0;
                nRet = SetIDCardID(nType, nSubID, 1);//返回0代表成功，其他失败
                #endregion

                #region 证件分类：对即将识别的证件自动分类
                int ncardType = 0;
                nRet = ClassifyIDCard(ref ncardType);
                if (nRet == -1)//没有设置有效证件类型
                {

                }
                if (nRet == -4)//没有加载图像
                {

                }
                if (nRet == -5)//分类失败，没找到匹配模板
                {

                }
                if (nRet == -6)//因拒识造成的分类失败
                {

                }
                if (nRet == -1002)//未设置识别证件类型
                {

                }
                if (nRet == -1005)//图像采集失败
                {

                }
                #endregion

                #region 证件识别
                if (ncardType == 1)//芯片卡
                {
                    nRet = RecogChipCard(nDGGroup, bRecogVIZ, nSaveImageType);
                }

                if (ncardType == 2)//普通MRZ证件
                {
                    nRet = RecogGeneralMRZCard(bRecogVIZ, nSaveImageType);
                }

                if (ncardType == 3)//不含有MRZ的普通证件
                {
                    nRet = RecogCommonCard(nSaveImageType);
                }

                if (nRet < 0)//识别失败
                {

                    return;
                }

                #endregion

                #region 识别成功,取得识别结果
                int MAX_CH_NUM = 128;
                char[] cArrFieldValue = new char[MAX_CH_NUM];
                if (cPassport != null)
                {
                    for (int i = 0; ; i++)
                    {

                        nRet = GetRecogResult(i, cArrFieldValue, ref MAX_CH_NUM);
                        if (nRet == 3)
                        {
                            break;
                        }
                        string sFieldValue = new string(cArrFieldValue);
                        sFieldValue = sFieldValue.Substring(0, sFieldValue.IndexOf('\0'));
                        #region cPassport对象赋值
                        switch (i)
                        {
                            case 0:
                                cPassport.Type = sFieldValue;
                                continue;
                            case 1:
                                cPassport.MRZ = sFieldValue;
                                continue;
                            case 2:
                                cPassport.Name = sFieldValue;
                                continue;
                            case 3:
                                cPassport.EnglishName = sFieldValue;
                                continue;
                            case 4:
                                cPassport.Sex = sFieldValue;
                                continue;
                            case 5:
                                cPassport.DateOfBirth = sFieldValue;
                                continue;
                            case 6:
                                cPassport.DateOfExpiry = sFieldValue;
                                continue;
                            case 7:
                                cPassport.CountryCode = sFieldValue;
                                continue;
                            case 8:
                                cPassport.EnglishFamilyName = sFieldValue;
                                continue;
                            case 9:
                                cPassport.EnglishGivienName = sFieldValue;
                                continue;
                            case 10:
                                cPassport.MRZ1 = sFieldValue;
                                continue;
                            case 11:
                                cPassport.MRZ2 = sFieldValue;
                                continue;
                            case 12:
                                cPassport.Nationality = sFieldValue;
                                continue;
                            case 13:
                                cPassport.PassportNo = sFieldValue;
                                continue;
                            case 14:
                                cPassport.PlaceOfBirth = sFieldValue;
                                continue;
                            case 15:
                                cPassport.PlaceOfIssue = sFieldValue;
                                continue;
                            case 16:
                                cPassport.DateOfIssue = sFieldValue;
                                continue;
                            case 17:
                                cPassport.RFIDMRZ = sFieldValue;
                                continue;
                            case 18:
                                cPassport.OCRMRZ = sFieldValue;
                                continue;
                            case 19:
                                cPassport.PlaceOfBirthPinyin = sFieldValue;
                                continue;
                            case 20:
                                cPassport.PlaceOfIssuePinyin = sFieldValue;
                                continue;
                            case 21:
                                cPassport.PersonalIdNo = sFieldValue;
                                continue;
                            case 22:
                                cPassport.NamePinyinOCR = sFieldValue;
                                continue;
                            case 23:
                                cPassport.SexOCR = sFieldValue;
                                continue;
                            case 24:
                                cPassport.NationalityOCR = sFieldValue;
                                continue;
                            case 25:
                                cPassport.PersonalIdNoOCR = sFieldValue;
                                continue;
                            case 26:
                                cPassport.PlaceOfBirthOCR = sFieldValue;
                                continue;
                            case 27:
                                cPassport.DateOfExpiryOCR = sFieldValue;
                                continue;
                            case 28:
                                cPassport.AuthorityOCR = sFieldValue;
                                continue;
                            case 29:
                                cPassport.FamilyName = sFieldValue;
                                continue;
                            case 30:
                                cPassport.GivienName = sFieldValue;
                                continue;
                            default:
                                break;
                        }
                        #endregion
                    }
                }
                #endregion

                #region 保存图片
                String strImgPath =  strRunPath + "\\" + cPassport.MRZ.Trim().ToString() + ".bmp";
                char[] carrImgPath = strImgPath.ToCharArray();
                nRet = SaveImageEx(carrImgPath, nSaveImageType);//
                if (nRet == 0)//图片保存成功
                {
                    cPassport.Photo = strRunPath + "\\" + cPassport.MRZ.Trim().ToString() + ".bmp";
                    cPassport.PhotoHead = strRunPath + "\\" + cPassport.MRZ.Trim().ToString() + "Head.bmp";
                }
                //SaveImages();
                #endregion

                #region 读取护照信息后触发事件
                if (OnReadCardEvent != null)
                {
                    OnReadCardEvent(cPassport, 2);
                }
                #endregion

                #region 通过TCP发送读取到的护照信息
                UploadPassportInfo();
                #endregion

                #region 保存护照信息到本地硬盘
                SaveReadPassPort();
                #endregion

            }

        }

        /// <summary>  
        /// 检查指定目录是否存在,如不存在则创建  
        /// </summary>  
        /// <param name="url"></param>  
        /// <returns></returns>  
        private bool checkDir(string url)
        {
            try
            {
                if (!Directory.Exists(url))//如果不存在就创建file文件夹　　             　　                
                    Directory.CreateDirectory(url);//创建该文件夹　　              
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        private bool SaveImages()
        {
            try
            {
                if (!m_bIsIDCardLoaded)
                {
                    return false;
                }
                if (!m_isPassportRead)
                {
                    return false;
                }
                //string filePath = Directory.GetCurrentDirectory();

                //String JpegImagePath = ConfigOperator.Single().StrSavePath;
                //if (Directory.Exists(JpegImagePath) == false)//如果不存在就创建file文件夹
                //{
                //    Directory.CreateDirectory(JpegImagePath);
                //}
                //Base64Crypt crypt = new Base64Crypt();

                //string strEncNum = crypt.Encode(info._idNum + info._dateStart);
                //String jpegImage = JpegImagePath + @"\cardImg\" + strEncNum + "_zp.jpg";
                //info._photo = jpegImage;
                //if (File.Exists(jpegImage))
                //{
                //    m_isReadCard = false;
                //    return true;
                //}

                //string photoPath = filePath + "\\zp.wlt";
                //int iii = GetBmpPhotoExt();
                //File.Copy(filePath + "\\zp.bmp", filePath + "\\zp_temp.bmp", true);

                //using (Bitmap bmp = new Bitmap(filePath + "\\zp_temp.bmp"))
                //{
                //    bmp.Save(jpegImage, ImageFormat.Jpeg);
                //    bmp.Dispose();
                //}

                //m_isReadCard = false;
                //info._photo = jpegImage;

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }

        }


        /// <summary>
        /// 通过TCP发送读取到的护照信息
        /// </summary>
        private void UploadPassportInfo()
        {
            try
            {
                //MsgCardInfo mcc = new MsgCardInfo();
                //mcc.Serial = ConfigOperator.Single().StrSerialNum;
                //mcc.CId = info._idNum;
                //mcc.CVStart = info._dateStart;
                //mcc.CName = info._name;
                //mcc.CBir = info._birthday;
                //mcc.CAddr = info._address;
                //mcc.CIss = info._issure;
                //mcc.CNation = info._nation;
                //mcc.CSex = info._sex;
                //mcc.CVEnd = info._dateEnd;
                //mcc.CPhoto = ImageDealOper.ImgToBase64String(info._photo);
                //TS_TcpClient.Single().ObjectSerializer(mcc, EMESSAGETYPE.MSG_DATA, ECOMMANDTYPE.NET_UPLOAD_CARDINFO);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 保存护照信息到本地硬盘
        /// </summary>
        private void SaveReadPassPort()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 文件数据初始化
        /// </summary>
        /// <returns></returns>
        public bool InitFileData()
        {
            try
            {
                //Trace.WriteLine("文件数据初始化...");
                bool bRet = false;
                //if (!fileOper.BInited)
                //{
                //    bRet = fileOper.InitFileOper();
                //    if (!bRet)
                //        return bRet;
                //}
                ////GetCardLists();
                //Trace.WriteLine("文件数据初始化完成");
                return bRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("文件数据初始化异常:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 启动读卡
        /// </summary>
        private void StartRun()
        {
            try
            {
                m_ThreadHandle.Start();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 停止运行
        /// </summary>
        private void StopRun()
        {
            try
            {
                Trace.WriteLine("开始停止读卡...");
                m_ThreadRunStatus = false;
                Thread.Sleep(500);
                if (!m_ThreadHandle.Join(10))
                {
                    m_ThreadHandle.Abort();
                }
                Close();
                Trace.WriteLine("停止读卡完成");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("停止运行异常:" + ex.Message);
            }
        }

    }
}
