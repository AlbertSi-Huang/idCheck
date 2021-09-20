using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using TCPLibrary.DeailMsg;
using TCPLibrary.DefaultImplements;
using TS_IDCheck.Info;
using TaiShou.LogHelper;

namespace TS_IDCheck
{
    /// <summary>
    /// H读卡器
    /// </summary>
    public class HSReadIdCard
    {
        public const int cbDataSize = 128;
        [DllImport("termb.dll")]
        static extern int InitCommExt();//自动搜索身份证阅读器并连接身份证阅读器 
        [DllImport("termb.dll")]
        static extern int CloseComm();//断开与身份证阅读器连接 
        [DllImport("termb.dll")]
        static extern int Authenticate();//判断是否有放卡，且是否身份证 
        [DllImport("termb.dll")]
        public static extern int Read_Content(int index);//读卡操作,信息文件存储在dll所在下
        [DllImport("termb.dll")]
        static extern int GetBmpPhotoExt();//解析身份证照片
        [DllImport("termb.dll")]
        static extern int GetCardInfo(int index, StringBuilder value);//解析身份证信息 

        private bool m_bInited = false;
        private bool m_isReadCard = false;
        private FileOperator fileOper;

        /// <summary>
        /// 线程名称
        /// </summary>
        private string ThreadName = "【HSReadIdCard】";

        /// <summary>
        /// 循环读卡线程
        /// </summary>
        private Thread m_ThreadHandle = null;

        /// <summary>
        /// 线程运行状态
        /// </summary>
        private bool m_ThreadRunStatus = false;

        /// <summary>
        /// 确认是否需要再次读身份证
        /// </summary>
        private bool m_isNeedCapture = true;

        /// <summary>
        /// 确认是否需要再次读身份证
        /// </summary>
        public bool IsNeedCapture
        {
            get { return m_isNeedCapture; }
            set { m_isNeedCapture = value; }
        }

        private static CaptureCardCompleteDelegate OnReadComplete;
        public event CaptureCardCompleteDelegate readCompleteEvent
        {
            add { OnReadComplete += new CaptureCardCompleteDelegate(value); }
            remove { OnReadComplete -= new CaptureCardCompleteDelegate(value); }
        }

        /// <summary>
        /// 实例化
        /// </summary>
        public HSReadIdCard()
        {
            try
            {
                Trace.WriteLine(ThreadName + "实例化 HSReadIdCard");
                m_isNeedCapture = true;
                m_isReadCard = false;
                m_ThreadRunStatus = false;
                fileOper = new FileOperator();

                m_ThreadHandle = new Thread(ReadCard);
                m_ThreadHandle.IsBackground = true;
            }
            catch
            {
            }
        }

        /// <summary>
        /// 通过TCP发送身份证信息
        /// </summary>
        /// <param name="info"></param>
        private void UploadCardInfo(SCardInfo info)
        {
            try
            {
                Trace.WriteLine(ThreadName + "UploadCardInfo begin");
                MsgCardInfo mcc = new MsgCardInfo();
                mcc.Serial = ConfigOperator.Single().StrSerialNum;
                mcc.CId = info._idNum;
                mcc.CVStart = info._dateStart;
                mcc.CName = info._name;
                mcc.CBir = info._birthday;
                mcc.CAddr = info._address;
                mcc.CIss = info._issure;
                mcc.CNation = info._nation;
                mcc.CSex = info._sex;
                mcc.CVEnd = info._dateEnd;
                mcc.CPhoto = ImageDealOper.ImgToBase64String(info._photo);
                //TS_TcpClient.Single().ObjectSerializer(mcc, EMESSAGETYPE.MSG_DATA, ECOMMANDTYPE.NET_UPLOAD_CARDINFO);
                Trace.WriteLine(ThreadName + "UploadCardInfo end");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "UploadCardInfo异常:" + ex.Message);
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
                Trace.WriteLine(ThreadName + "文件数据初始化...");
                bool bRet = false;
                if (!fileOper.BInited)
                {
                    bRet = fileOper.InitFileOper();
                    if (!bRet)
                        return bRet;
                }
                GetCardLists();
                Trace.WriteLine(ThreadName + "文件数据初始化完成");
                return bRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "文件数据初始化异常:" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 启动读卡
        /// </summary>
        public void StartRun()
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
        public void StopRun()
        {
            try
            {
                Trace.WriteLine(ThreadName + "开始停止读卡...");
                m_ThreadRunStatus = false;
                Thread.Sleep(500);
                if (!m_ThreadHandle.Join(10))
                {
                    m_ThreadHandle.Abort();
                }
                CloseIDCard();
                Trace.WriteLine(ThreadName + "停止读卡完成");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "停止运行异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 读卡
        /// </summary>
        public void ReadCard()
        {
            Thread.Sleep(5000);

            //if (!m_bInited)
            //{
            //    Trace.WriteLine(ThreadName + "读卡器没有初始化成功，退出");
            //    return;
            //}
            byte[] pucManaInfo = new byte[4];
            byte[] pucManaMsg = new byte[8];
            byte[] pucCHMsg = new byte[256];//文字信息最长256字节
            byte[] pucPHMsg = new byte[1024];//相片信息最长1024字节
            byte[] pucFPMsg = new byte[1024];//指纹信息最长1024字节\

            try
            {
                string filePath = Directory.GetCurrentDirectory();
                int iRet = 0;
                m_ThreadRunStatus = true;
                Trace.WriteLine(ThreadName + "HS读卡开始");
                while (true)
                {
                    //如果不需要采集 则直接循环
                    if (!m_isNeedCapture)
                    {
                        Thread.Sleep(300);
                        continue;
                    }
                    //线程退出
                    if (m_ThreadRunStatus == false)
                    {
                        Trace.WriteLine(ThreadName + "HS循环读卡线程退出");
                        return;
                    }
                    if (File.Exists(filePath + "\\fp.bin"))
                    {
                        File.Delete(filePath + "\\fp.bin");
                    }
                    if (File.Exists(filePath + "\\wz.txt"))
                    {
                        //如果存在则删除
                        File.Delete(filePath + "\\wz.txt");
                    }
                    if (File.Exists(filePath + "\\zp.bmp"))
                    {
                        //如果存在则删除
                        File.Delete(filePath + "\\zp.bmp");
                    }
                    //找卡
                    iRet = Authenticate();
                    if (iRet != 1)
                    {
                        Thread.Sleep(300);
                        continue;
                    }

                    //选卡
                    iRet = Read_Content(1);
                    if (iRet != 1)
                    {
                        Thread.Sleep(300);
                        continue;
                    }
                    iRet = GetBmpPhotoExt();  //获取图片
                    if (iRet != 1)
                    {
                        Thread.Sleep(300);
                        continue;
                    }
                    m_isReadCard = true;
                    SCardInfo cardInfo = new SCardInfo();  //身份证信息
                    StringBuilder sb = new StringBuilder(cbDataSize);
                    for (int i = 0; i < 9; i++)
                    {
                        GetCardInfo(i, sb);
                        switch (i)
                        {
                            case 0: cardInfo._name = sb.ToString(); break;
                            case 1: cardInfo._sex = sb.ToString(); break;
                            case 2:
                                {
                                    cardInfo._nation = sb.ToString();
                                    if(cardInfo._nation == null) { cardInfo._nation = ""; }
                                }
                                break;
                            case 3:
                                string birthday = sb.ToString();
                                string year = birthday.Substring(0, 4) + "年";
                                string moth = birthday.Substring(4, 2) + "月";
                                string day = birthday.Substring(6) + "日";
                                cardInfo._birthday = year + moth + day;
                                break;
                            case 4:
                                cardInfo._address = sb.ToString();
                                break;
                            case 5: cardInfo._idNum = sb.ToString(); break;
                            case 6:
                                cardInfo._issure = sb.ToString();
                                break;
                            case 7: cardInfo._dateStart = sb.ToString(); break;
                            case 8: cardInfo._dateEnd = sb.ToString(); break;
                        }
                    }
                    GetCardInfo(ref cardInfo);
                    OnReadComplete(cardInfo);
                    UploadCardInfo(cardInfo);
                    SaveReadCard(cardInfo);
                    Thread.Sleep(300);
                }
            }
            catch (Exception ex)
            {
                m_ThreadRunStatus = false;
                Trace.WriteLine(ThreadName + "循环读卡线程异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 填充身份证信息到string中
        /// </summary>
        /// <param name="info"></param>
        private void PushCardInfo(SCardInfo info)
        {
            try
            {
                string strInfo = info._idNum;
                strInfo += ",";
                strInfo += info._name;
                strInfo += ",";
                strInfo += info._sex;
                strInfo += ",";
                strInfo += info._nation;
                strInfo += ",";
                strInfo += info._birthday;
                strInfo += ",";
                strInfo += info._address;
                strInfo += ",";
                strInfo += info._issure;
                strInfo += ",";
                strInfo += info._dateStart;
                strInfo += ",";
                strInfo += info._dateEnd;
                strInfo += ",";
                strInfo += info._photo;
                strCardNumLists.Add(strInfo);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 保存身份证信息
        /// </summary>
        /// <param name="info"></param>
        private void SaveReadCard(SCardInfo info)
        {
            try
            {
                foreach (string s in strCardNumLists)
                {
                    string[] ss = s.Split(',');

                    if (ss[0].CompareTo(info._idNum) == 0 && ss[7].CompareTo(info._dateStart) == 0)
                    {
                        return;
                    }
                }
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();  //开始监视代码运行时间

                //save to file
                fileOper.SaveCardInfo(info,"0");
                //save to memory
                PushCardInfo(info);

                watch.Stop();  //停止监视
                TimeSpan timespan = watch.Elapsed;  //获取当前实例测量得出的总时间
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "SaveReadCard异常:" + ex.Message);
            }
        }

        private List<string> strCardNumLists = new List<string>();

        /// <summary>
        /// 获取身份证列表
        /// </summary>
        private void GetCardLists()
        {
            try
            {
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();  //开始监视代码运行时间
                if (fileOper.BInited)
                {
                    fileOper.ReadCardInfo(out strCardNumLists);
                }
                watch.Stop();  //停止监视
                TimeSpan timespan = watch.Elapsed;  //获取当前实例测量得出的总时间
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ThreadName + "GetCardLists异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 获取身份证信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool GetCardInfo(ref SCardInfo info)
        {
            try
            {
                if (!m_bInited)
                {
                    return false;
                }
                if (!m_isReadCard)
                {
                    return false;
                }
                string filePath = Directory.GetCurrentDirectory();

                String JpegImagePath = ConfigOperator.Single().StrSavePath;
                if (Directory.Exists(JpegImagePath) == false)//如果不存在就创建file文件夹
                {
                    Directory.CreateDirectory(JpegImagePath);
                }
                Base64Crypt crypt = new Base64Crypt();

                string strEncNum = crypt.Encode(info._idNum + info._dateStart);
                String jpegImage = JpegImagePath + @"\cardImg\" + strEncNum + "_zp.jpg";
                info._photo = jpegImage;
                if (File.Exists(jpegImage))
                {
                    m_isReadCard = false;
                    return true;
                }

                string photoPath = filePath + "\\zp.wlt";
                int iii = GetBmpPhotoExt();
                File.Copy(filePath + "\\zp.bmp", filePath + "\\zp_temp.bmp", true);

                using (Bitmap bmp = new Bitmap(filePath + "\\zp_temp.bmp"))
                {
                    bmp.Save(jpegImage, ImageFormat.Jpeg);
                    bmp.Dispose();
                }

                m_isReadCard = false;
                info._photo = jpegImage;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 关闭读卡器
        /// </summary>
        private void CloseIDCard()
        {
            try
            {
                if (m_bInited)
                {
                    CloseComm();
                    m_bInited = false;
                    Trace.Write(ThreadName + "读卡器关闭完成");
                }
            }
            catch (Exception ex)
            {
                Trace.Write(ThreadName + "关闭读卡器异常:" + ex.Message);
            }
        }

        /// <summary>
        /// 打开读卡器
        /// </summary>
        /// <returns></returns>
        public bool OpenIDCard()
        {
            try
            {
                int iRet = InitCommExt();
                if (iRet <= 0)
                {
                    Trace.WriteLine(ThreadName + "打开读卡器失败");
                    return false;
                }

                m_bInited = true;
                Trace.WriteLine(ThreadName + "打开读卡器成功");
                return true;
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ThreadName + "打开读卡器异常" + ex.Message);
                return false;
            }
        }
    }
}
