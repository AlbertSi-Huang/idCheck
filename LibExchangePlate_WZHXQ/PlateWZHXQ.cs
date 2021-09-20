using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using MySqlLibrary;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Data;
using Newtonsoft.Json;

namespace LibExchangePlate
{

    public struct SVisitor
    {
        public string name { set; get; }
        public string gender { set; get; }
        public string nation { set; get; }
        public string idcard { set; get; }
        public string cardType { set; get; }
        public string countryOrAreaCode { set; get; }

        public string countryOrAreaName { set; get; }
        public string cardVersion { set; get; }

        public string currentApplyOrgCode { set; get; }

        public string signNum { set; get; }

    }

    public struct SWZHXQKeepLive
    {
        public string equipmentId { set; get; }
        public string equipmentName { set; get; }
        public string equipmentStatus { set; get; }

        public string checkTime { set; get; }

        public string remark { set; get; }
    }

    public struct SWZHXQUploadPersonInfo
    {
        public string ver;
        public string id;           //唯一码 uuid
        public string passTime;
        public string name;
        public string gender;
        public string nation;
        public string idcard;
        public string birthday;
        public string address;
        public string authority;    //发证机关
        public string validityStart;
        public string validityEnd;
        //public string validDate;    //有效期
        //原来采用ftp上传  后改为base64上传 hsx20180702
        //public string personImgUrl; //采集照片路径
        //public string idcardImgUrl;

        public string personImg;
        public string idcardImg;

        public string areaCode;
        public string x;
        public string y;
        public string equipmentId;  //设备编号
        public string equipmentName;    //设备名称
        public string equipmentType;    //设备类型
        public string stationId;        //警务站编号
        public string stationName;      //警务站名称
        public string backUrl;          //回传的接口url  可选
        public string location;         //设备所在位置名称
        public string status;           //出入标识：1进入，0出
        public string dareaname;        //小区名称
        public string dareacode;        //场所编号
        public string placetype;        //场所类别 01表示封闭式社区，02表示小区，03表示无人管理小区，04表示旅店/宾馆，05表示停车场，06网吧， 07 政府、公检法单位等， 08 企事业单位，09 学校， 10 机场/车站，11 其它，
        public string identity;         //人员类别 1 业主, 2 租户, 3 访客
        public string homeplace;        //家族住址
        public string contact;          //联系电话
        public string isconsist;        //人证是否一致 1 一致，0 不一致
        public string openmode;         //1人脸识别  2门禁卡  3密码开门  4 APP开门  5人脸与身份证识别比对  6手机邀请码  7访客门铃  8可视对讲机 9 其它
        public string cardType;         //1 身份证 2 港澳台居住证 3 中国护照4 国外护照 5 永久居住证 9 其他
        public string compareScore;     //比对分值

        //add by hsx 20181130
        public string countryOrAreaCode { set; get; }   // 156
        public string countryOrAreaName { set; get; }   //中国
        public string cardVersion { set; get; } //证件版本号
        public string currentApplyOrgCode { set; get; } //
        public string signNum { set; get; }
        public string visitReason { set; get; }

        /// <summary>
        /// 手机MAC地址，App开门时填写
        /// </summary>
        public string mac { set; get; }

        /// <summary>
        /// 手机IMSI码，App开门时填写
        /// </summary>
        public string imsi { set; get; }

        /// <summary>
        /// 手机IMEI码，App开门时填写
        /// </summary>
        public string imei { set; get; }

        public List<SVisitor> visitor { set; get; }     //被访人信息
    }

    public class PlateWZHXQ : PlateExchangeAbstract
    {
        private static readonly string databaseName = "TS_UploadPlate";
        TableHandle<EUploadPlate> uploadTable = null;
        public static readonly TableColumnParam<EUploadPlate>[] uploadCol = {
            new TableColumnParam<EUploadPlate>(EUploadPlate.passTime,"varchar(32)",false,EKeyType.PRI),
            new TableColumnParam<EUploadPlate>(EUploadPlate.idCardNumber,"varchar(32)"),
            new TableColumnParam<EUploadPlate>(EUploadPlate.carNumber,"varchar(16)"),
            new TableColumnParam<EUploadPlate>(EUploadPlate.reserved0,"varchar(4)"),
        };

        public PlateWZHXQ()
        {
            Name = "WZHXQ";
        }

        // 定义一个标识确保线程同步
        private static readonly object locker = new object();
        private bool CreateUploadTable()
        {
            // { get; private set; }
            uploadTable = new TableHandle<EUploadPlate>("WZHXQ_uploadPlate", databaseName, uploadCol);
            if (!uploadTable.CreateTable()) return false;
            return true;
        }
        Thread uploadThread = null;
        Thread keepLiveThread = null;
        HttpExchange _httpOper = null;

        /// <summary>
        /// 将上传数据表置为可上传状态
        /// </summary>
        private void SetInitUploadTable()
        {
            uploadTable.ChangeValue(EUploadPlate.reserved0, "0");
        }

        string _strEquipmentStatus = "2";

        private void Run()
        {
            Thread.Sleep(3000);
            DataRow dr = null;
            DataRow drPersonRecord = null;
            DataRow drPersonInfo = null;
            string strBackMsg = string.Empty;
            SetInitUploadTable();
            _httpOper = new HttpExchange();
            string strUrl = "http://" + ConfigWZHXQ.Single().httpIp + ":" + ConfigWZHXQ.Single().httpPort + "/patsys/upload/person";
            SWZHXQUploadPersonInfo wUploadInfo = new SWZHXQUploadPersonInfo();
            wUploadInfo.cardType = "1";
            wUploadInfo.areaCode = ConfigWZHXQ.Single().areaCode;
            string strX = ConfigWZHXQ.Single().Longitude;
            float dx = float.Parse(strX);
            wUploadInfo.x = strX.ToString();
            string strY = ConfigWZHXQ.Single().Latitude;
            float dy = float.Parse(strY);
            wUploadInfo.y = strY.ToString();
            wUploadInfo.equipmentId = ConfigWZHXQ.Single().equipmentId;
            wUploadInfo.equipmentName = ConfigWZHXQ.Single().equipmentName;
            wUploadInfo.equipmentType = ConfigWZHXQ.Single().equipmentType;
            wUploadInfo.stationId = ConfigWZHXQ.Single().stationId;
            wUploadInfo.stationName = ConfigWZHXQ.Single().stationName;
            wUploadInfo.location = ConfigWZHXQ.Single().location;
            wUploadInfo.status = ConfigWZHXQ.Single().status;
            wUploadInfo.dareaname = ConfigWZHXQ.Single().dareaname;
            wUploadInfo.dareacode = ConfigWZHXQ.Single().dareacode;
            wUploadInfo.placetype = ConfigWZHXQ.Single().placetype;   //场所类别
            wUploadInfo.ver = ConfigWZHXQ.Single().ver;
            wUploadInfo.countryOrAreaCode = "156";
            wUploadInfo.countryOrAreaName = "中国";
            wUploadInfo.cardVersion = "";
            wUploadInfo.currentApplyOrgCode = "";
            wUploadInfo.mac = "";
            wUploadInfo.imsi = "";
            wUploadInfo.imei = "";
            wUploadInfo.homeplace = "";
            wUploadInfo.contact = "";
            wUploadInfo.backUrl = "";
            //wUploadInfo.visitor = "[]";
            wUploadInfo.visitReason = "2";
            wUploadInfo.signNum = "";
            wUploadInfo.visitor = new List<SVisitor>();

            string strSiteImgUrl = string.Empty;
            string strCardImgUrl = string.Empty;

            string strSerialCode = ConfigWZHXQ.Single().equipmentId;
            try
            {
                int nSleepTick = 1;
                Trace.WriteLine("准备上传线程完成");
                while (uploadThread.IsAlive)
                {
                    _strEquipmentStatus = "1";
                    lock (locker)
                    {
                        dr = uploadTable.GetOneData(new List<QueryCondition<EUploadPlate>>
                        {
                            new QueryCondition<EUploadPlate>(EUploadPlate.reserved0,"0")
                        });
                    }
                    if (dr == null || dr[EUploadPlate.passTime.ToString()].ToString().Length == 0)
                    {
                        Thread.Sleep(100 * nSleepTick);
                        nSleepTick++;
                        if (nSleepTick == 1000)
                        {
                            nSleepTick = 1000;
                        }
                        continue;
                    }
                    string upId = dr[EUploadPlate.idCardNumber.ToString()].ToString();
                    string upPassTime = dr[EUploadPlate.passTime.ToString()].ToString();
                    string upType = dr[EUploadPlate.reserved0.ToString()].ToString();
                    if (upType.CompareTo("0") == 0)
                    {
                        //过人记录
                        drPersonRecord = DatabaseHandle.Single.FaceDetectTable.GetOneData(new List<QueryCondition<FaceDetectColumn>>
                    { new QueryCondition<FaceDetectColumn>(FaceDetectColumn.idCardNumber,upId),new QueryCondition<FaceDetectColumn>(FaceDetectColumn.compareTime,upPassTime) });

                        //身份证信息
                        drPersonInfo = DatabaseHandle.Single.IdCardTable.GetOneData(new List<QueryCondition<IdCardColumn>> {
                    new QueryCondition<IdCardColumn>(IdCardColumn.idCardNumber,upId)});

                        if (drPersonRecord != null && drPersonInfo != null)
                        {
                            nSleepTick = 1;
                            SPersonRecord record = new SPersonRecord();
                            record._detectResult = drPersonRecord[FaceDetectColumn.detectResult.ToString()].ToString();
                            record._detectScore = drPersonRecord[FaceDetectColumn.detectScore.ToString()].ToString();
                            record._detectType = drPersonRecord[FaceDetectColumn.detectMode.ToString()].ToString();
                            record._imgSite = drPersonRecord[FaceDetectColumn.imgSite.ToString()].ToString();
                            record._imgScene = drPersonRecord[FaceDetectColumn.imgScene.ToString()].ToString();
                            record._personType = drPersonRecord[FaceDetectColumn.personType.ToString()].ToString();

                            record._passTime = drPersonRecord[FaceDetectColumn.compareTime.ToString()].ToString();//dtPassTime.ToString("yyyy-MM-dd HH:mm:ss");

                            SCardInfo card = new SCardInfo();
                            card._idNum = upId;
                            card._name = drPersonInfo[IdCardColumn.personName.ToString()].ToString();
                            card._birthday = drPersonInfo[IdCardColumn.birthday.ToString()].ToString();
                            card._dateEnd = drPersonInfo[IdCardColumn.dateEnd.ToString()].ToString();
                            card._dateStart = drPersonInfo[IdCardColumn.dateStart.ToString()].ToString();
                            card._issure = drPersonInfo[IdCardColumn.issue.ToString()].ToString();
                            card._nation = drPersonInfo[IdCardColumn.ethnic.ToString()].ToString();
                            card._sex = drPersonInfo[IdCardColumn.gender.ToString()].ToString();
                            card._photo = drPersonInfo[IdCardColumn.idCardPhoto.ToString()].ToString();
                            card._address = drPersonInfo[IdCardColumn.address.ToString()].ToString();

                            //1.ftp上传图片 2.http上传详细信息
                            wUploadInfo.id = Guid.NewGuid().ToString("N");
                            string ftpCardName = "person_1_" + wUploadInfo.id + "_001.jpg";
                            string ftpSiteName = "person_2_" + wUploadInfo.id + "_001.jpg";
                            string dateTimeDay = DateTime.Now.ToString("yyyyMMdd");
                            string ftpImgSitePath = "person/" + strSerialCode + "/" + dateTimeDay;
                            string ftpImgCardPath = "idcard/" + strSerialCode + "/" + dateTimeDay;
                            strCardImgUrl = string.Empty; strSiteImgUrl = string.Empty;
                            if (File.Exists(card._photo))
                            {
                                wUploadInfo.idcardImg = Common.ImageDealOper.ImgToBase64String(card._photo); //idcardImgUrl = strCardImgUrl;//ftpCardName;
                            }
                            Trace.WriteLine("record._imgSite == " + record._imgSite);
                            if (record._imgSite != null && File.Exists(record._imgSite))
                            {
                                wUploadInfo.personImg = Common.ImageDealOper.ImgToBase64String(record._imgSite);   //personImgUrl = strSiteImgUrl;//ftpSiteName;
                            }
                            Trace.WriteLine("准备上传基本信息");
                            //拼接上传信息
                            wUploadInfo.name = card._name != null ? card._name : "";
                            wUploadInfo.gender = (Convert.ToInt32(card._idNum.Substring(16, 1)) % 2 == 0) ? "女性" : "男性"; //card._sex != null?card._sex:"";
                            wUploadInfo.nation = card._nation != null ? card._nation + "族" : "";
                            wUploadInfo.idcard = card._idNum;
                            string upBir = string.Empty;
                            try
                            {
                                if (card._birthday != null && card._birthday.Length == 8)
                                {
                                    upBir = card._birthday.Substring(0, 4) + "年" + card._birthday.Substring(4, 2) + "月" + card._birthday.Substring(6, 2) + "日";
                                }
                                else
                                {
                                    upBir = card._idNum.Substring(6, 4) + "年" + card._idNum.Substring(10, 2) + "月" + card._idNum.Substring(12, 2) + "日";
                                }
                                wUploadInfo.birthday = upBir;
                                wUploadInfo.address = card._address != null ? card._address : "";
                                wUploadInfo.authority = card._issure != null ? card._issure : "";
                                wUploadInfo.compareScore = record._detectScore.ToString();

                                string sStrart = "";
                                if (card._dateStart != null && card._dateStart.Length == 8)
                                {
                                    sStrart = card._dateStart.Substring(0, 4) + "." + card._dateStart.Substring(4, 2) + "." + card._dateStart.Substring(6, 2);
                                }
                                wUploadInfo.validityStart = sStrart;
                                if (card._dateEnd != null && card._dateEnd.IndexOf("长期") != -1)
                                {
                                    card._dateEnd = "21990101";
                                }
                                string strEnd = string.Empty;
                                if (card._dateEnd != null && card._dateEnd.Length == 8)
                                {
                                    strEnd = card._dateEnd.Substring(0, 4) + "." + card._dateEnd.Substring(4, 2) + "." + card._dateEnd.Substring(6, 2);

                                }

                                wUploadInfo.validityEnd = strEnd;
                                wUploadInfo.isconsist = (record._detectResult.CompareTo("1") == 0 || record._detectResult.CompareTo("3") == 0) ? "1" : "0";
                                wUploadInfo.identity = record._personType.CompareTo("0") == 0 ? "3" : (record._personType.CompareTo("1") == 0 ? "1" : "2");//record._detectType.CompareTo("1") == 0 ? "1" : "3";
                                wUploadInfo.openmode = record._detectType.CompareTo("0") == 0 ? "5" : "1";
                                wUploadInfo.passTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");//record._passTime;//
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("wlmq平台数据转换错误 ： " + ex.Message);
                            }
                            var requestjson = JsonConvert.SerializeObject(wUploadInfo);
                            Trace.WriteLine(requestjson);
                            bool bUpload = _httpOper.RequestPost(strUrl, (object)wUploadInfo, out strBackMsg);
                            Trace.WriteLine("乌鲁木齐平台返回：" + strBackMsg);
                            bool bRet = DealWithBack(strBackMsg);
                            if (bRet)
                            {
                                Trace.WriteLine(wUploadInfo.id + " 上传成功");
                            }
                            else
                            {
                                uploadTable.ChangeValue(EUploadPlate.reserved0, "1", new List<QueryCondition<EUploadPlate>>
                                {
                                    new QueryCondition<EUploadPlate>(EUploadPlate.idCardNumber,upId),
                                    new QueryCondition<EUploadPlate>(EUploadPlate.passTime,record._passTime)
                                });

                                Trace.WriteLine(wUploadInfo.id + " 上传失败");
                            }

                            //删除数据库记录和图片
                            try
                            {
                                lock (locker)
                                {
                                    DateTime dt;
                                    DateTime.TryParse(record._passTime, out dt);
                                    string strTmp = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                    uploadTable.DeleteData(new List<QueryCondition<EUploadPlate>> {
                                    new QueryCondition<EUploadPlate>(EUploadPlate.idCardNumber,upId),
                                    new QueryCondition<EUploadPlate>(EUploadPlate.passTime,strTmp)
                                });
                                    string pathSite = Path.GetDirectoryName(record._imgSite);
                                    if (File.Exists(pathSite + "\\" + ftpSiteName))
                                        File.Delete(pathSite + "\\" + ftpSiteName);
                                    string pathCard = Path.GetDirectoryName(card._photo);
                                    if (File.Exists(pathCard + "\\" + ftpCardName))
                                        File.Delete(pathCard + "\\" + ftpCardName);
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("乌鲁木齐平台删除失败" + ex.Message);
                            }

                            Thread.Sleep(20);
                        }
                        else
                        {
                            uploadTable.ChangeValue(EUploadPlate.reserved0, "1", new List<QueryCondition<EUploadPlate>>
                                {
                                    new QueryCondition<EUploadPlate>(EUploadPlate.idCardNumber,upId),
                                    new QueryCondition<EUploadPlate>(EUploadPlate.passTime,upPassTime)
                                });
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {

                    }

                }
                _strEquipmentStatus = "2";
            }
            catch (Exception ex)
            {
                _strEquipmentStatus = "2";
                Trace.WriteLine(ex.Message);
            }

        }

        private bool DealWithBack(string msgBack)
        {
            int nPos = msgBack.IndexOf("tranResult");
            if (nPos != -1)
            {
                string strRes = msgBack.Substring(nPos + 13, 3);
                if (strRes.CompareTo("200") == 0)
                {
                    return true;
                }
                else if (strRes.CompareTo("400") == 0)
                {
                    //较时
                    string strTime = "2018-03-05 14:14:22";
                    nPos = msgBack.IndexOf("sendTime");
                    if (nPos != -1)
                    {
                        strTime = msgBack.Substring(nPos + 11, 19);
                        TimeAbout.SetLocalTimeByStr(strTime);
                    }

                    return false;
                }
            }

            return false;
        }
        PlateExchangeBakInfo _broadcastInfo = new PlateExchangeBakInfo();


        public override bool Init(string userName, string pwd, string localHost)
        {
            bool bRet = base.Init(userName, pwd, localHost);
            if (!bRet)
            {
                _broadcastInfo._nLevel = EWainLevel.EWL_error;
                _broadcastInfo._msg = "数据表创建失败";
                OnBroadcastExchangeInfo(_broadcastInfo);
                return false;
            }

            ConfigWZHXQ.Single().InitConfig();

            uploadThread = new Thread(Run);
            uploadThread.IsBackground = true;
            uploadThread.Start();

            keepLiveThread = new Thread(OnKeepLive);
            keepLiveThread.IsBackground = true;
            keepLiveThread.Start();

            bRet = CreateUploadTable();
            if (!bRet)
            {
                _broadcastInfo._nLevel = EWainLevel.EWL_error;
                _broadcastInfo._msg = "上传表创建失败";
                OnBroadcastExchangeInfo(_broadcastInfo);
            }
            return bRet;
        }

        private void OnKeepLive()
        {
            Thread.Sleep(5000);
            string strUrl = "http://" + ConfigWZHXQ.Single().httpIp + ":" + ConfigWZHXQ.Single().httpPort + "/patsys/upload/deviceHeart";
            int nTimeTick = ConfigWZHXQ.Single().nKeepLiveTick;
            string strEquipmentId = ConfigWZHXQ.Single().equipmentId;
            string strEquipmentName = ConfigWZHXQ.Single().equipmentName;

            SWZHXQKeepLive sKeepLive = new SWZHXQKeepLive();
            sKeepLive.equipmentId = strEquipmentId;
            sKeepLive.equipmentName = strEquipmentName;
            sKeepLive.remark = "";

            DateTime _lastTikc = DateTime.Now;

            while (keepLiveThread.IsAlive)
            {
                DateTime dtNow = DateTime.Now;
                TimeSpan ts = dtNow.Subtract(_lastTikc);
                if (ts.TotalMilliseconds < nTimeTick * 1000)
                {
                    Thread.Sleep(100);
                    continue;
                }
                //Trace.WriteLine("准备提交Http Post");
                _lastTikc = DateTime.Now;
                sKeepLive.checkTime = _lastTikc.ToString("yyyy-MM-dd HH:mm:ss");
                sKeepLive.equipmentStatus = _strEquipmentStatus;

                string strMsg = "";
                bool bRet = _httpOper.RequestPost(strUrl, sKeepLive, out strMsg);

                if (bRet)
                {
                    int nPos = strMsg.IndexOf("200");
                    if (nPos == -1)
                    {
                        Trace.WriteLine("心跳接口返回错误" + strMsg);
                    }
                }
                Thread.Sleep(100);
                continue;
            }

        }

        public override bool LoginIn(string reserv)
        {
            return true;
        }

        public override int QueryBlack(string cardId, int nType = 0)
        {
            return 0;
        }

        public override int SavePersonRecord(SPersonRecord info)
        {
            int nRet = base.SavePersonRecord(info);
            if (nRet == -1)
                return nRet;

            DataRow drUpload = uploadTable.NewDataTable().NewRow();
            drUpload[EUploadPlate.carNumber.ToString()] = "";
            drUpload[EUploadPlate.idCardNumber.ToString()] = info._idNum;
            drUpload[EUploadPlate.passTime.ToString()] = info._passTime;
            drUpload[EUploadPlate.reserved0.ToString()] = "0";     //过人记录
            lock (locker)
            {
                Trace.WriteLine("乌鲁木齐平台保存信息:" + info._idNum);
                nRet = uploadTable.InsertData(drUpload);
                if (nRet == 0) return -1;
            }

            return 0;
        }


    }
}
