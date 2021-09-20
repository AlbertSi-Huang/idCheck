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
using System.Windows;

namespace LibExchangePlate
{
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
        public string contace;          //联系电话
        public string isconsist;        //人证是否一致 1 一致，0 不一致
        public string openmode;         //1人脸识别  2门禁卡  3密码开门  4 APP开门  5人脸与身份证识别比对  6手机邀请码  7访客门铃  8可视对讲机 9 其它
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
        HttpExchange _httpOper = null;

        private void Run()
        {
            Thread.Sleep(3000);
            DataRow dr = null;
            DataRow drPersonRecord = null;
            DataRow drPersonInfo = null;
            string strBackMsg = string.Empty;

            _httpOper = new HttpExchange();
            string strUrl = "http://" + ConfigWZHXQ.Single().httpIp + ":" + ConfigWZHXQ.Single().httpPort + "/patsys/upload/person";
            SWZHXQUploadPersonInfo wUploadInfo = new SWZHXQUploadPersonInfo();
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
            string strSiteImgUrl = string.Empty;
            string strCardImgUrl = string.Empty;

            string strSerialCode = ConfigWZHXQ.Single().equipmentId;
            try
            {
                int nSleepTick = 1;
                while (uploadThread.IsAlive)
                {
                    lock (locker)
                    {
                        dr = uploadTable.GetOneData();
                    }
                    if (dr == null || dr[EUploadPlate.passTime.ToString()].ToString().Length == 0)
                    {
                        Thread.Sleep(100 * nSleepTick);
                        nSleepTick++;
                        if(nSleepTick == 1000)
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
                            wUploadInfo.name = card._name != null && card._name.Length > 0 ? card._name : "空";
                            wUploadInfo.gender = (Convert.ToInt32(card._idNum.Substring(16, 1)) % 2 == 0) ? "女" : "男"; //card._sex != null?card._sex:"";
                            wUploadInfo.nation = card._nation != null && card._nation.Length > 0? card._nation : "空";
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
                                wUploadInfo.address = card._address != null && card._address.Length>0 ? card._address : "空";
                                wUploadInfo.authority = card._issure != null && card._issure.Length>0 ? card._issure : "空";

                                string sStrart = "";
                                if (card._dateStart != null && card._dateStart.Length == 8)
                                {
                                    sStrart = card._dateStart.Substring(0, 4) + "-" + card._dateStart.Substring(4, 2) + "-" + card._dateStart.Substring(6, 2);
                                }
                                wUploadInfo.validityStart = sStrart;
                                if (card._dateEnd != null && card._dateEnd.IndexOf("长期") != -1)
                                {
                                    card._dateEnd = "21990101";
                                }
                                string strEnd = string.Empty;
                                if (card._dateEnd != null && card._dateEnd.Length == 8)
                                {
                                    strEnd = card._dateEnd.Substring(0, 4) + "-" + card._dateEnd.Substring(4, 2) + "-" + card._dateEnd.Substring(6, 2);

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
                            bool bUpload = _httpOper.RequestPost(strUrl, (object)wUploadInfo, out strBackMsg);
                            Trace.WriteLine("乌鲁木齐上传返回：" + strBackMsg);
                            DealWithBack(strBackMsg);
                            if (bUpload)
                            {
                                Trace.WriteLine(wUploadInfo.id + " 上传成功"  );
                            }
                            else
                            {
                                Trace.WriteLine(wUploadInfo.id + " 上传失败:" + strBackMsg);
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
                    }
                    else if (upType.CompareTo("1") == 0)
                    {
                        //过车记录
                    }

                }
            }
            catch (Exception ex)
            {
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
                    }

                    TimeAbout.SetLocalTimeByStr(strTime);
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
            bRet = CreateUploadTable();
            if (!bRet)
            {
                _broadcastInfo._nLevel = EWainLevel.EWL_error;
                _broadcastInfo._msg = "上传表创建失败";
                OnBroadcastExchangeInfo(_broadcastInfo);
            }else
            {
                Trace.WriteLine("乌鲁木齐平台创建成功");
            }
            return bRet;
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
            {
                Trace.WriteLine("乌鲁木齐平台基本记录保存失败");
                return nRet;
            }
            DataRow drUpload = uploadTable.NewDataTable().NewRow();
            drUpload[EUploadPlate.carNumber.ToString()] = "";
            drUpload[EUploadPlate.idCardNumber.ToString()] = info._idNum;
            drUpload[EUploadPlate.passTime.ToString()] = info._passTime;
            drUpload[EUploadPlate.reserved0.ToString()] = "0";     //过人记录
            lock (locker)
            {
                nRet = uploadTable.InsertData(drUpload);
                if (nRet == 0)
                {
                    Trace.WriteLine("乌鲁木齐平台保存失败：" + info._idNum);
                    return -1;
                }
            }

            return 0;
        }

        public override int SaveCarRecord(SCarRecord info)
        {
            DataRow dr = DatabaseHandle.Single.CarRecords.NewDataTable().NewRow();
            dr[ECarCapture.carNum.ToString()] = info._carNum;
            dr[ECarCapture.carColor.ToString()] = info._carColor;
            dr[ECarCapture.idCardNumber.ToString()] = info._idNum;
            dr[ECarCapture.imgCar.ToString()] = info._imgCar;
            dr[ECarCapture.imgCarNum.ToString()] = info._imgCarNum;
            dr[ECarCapture.passTime.ToString()] = info._passTime;
            int nRet = DatabaseHandle.Single.CarRecords.InsertData(dr);
            if (nRet == 0) return -1;
            lock (locker)
            {
                DataRow drUpload = uploadTable.NewDataTable().NewRow();
                drUpload[EUploadPlate.carNumber.ToString()] = info._carNum;
                drUpload[EUploadPlate.idCardNumber.ToString()] = info._idNum;
                drUpload[EUploadPlate.passTime.ToString()] = info._passTime;
                drUpload[EUploadPlate.reserved0.ToString()] = "1"; //过车记录
                nRet = uploadTable.InsertData(drUpload);
                if (nRet == 0) return -1;
            }

            return 0;
        }
    }
}
