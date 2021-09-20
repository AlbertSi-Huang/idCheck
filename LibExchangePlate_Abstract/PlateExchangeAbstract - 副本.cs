using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using MySqlLibrary;

namespace LibExchangePlate
{
    public class PlateExchangeBakInfo
    {
        public string _msg { set; get; }
        public string _name { set; get; }
        public EWainLevel _nLevel { set; get; }

        public string _funcName { set; get; }

        public PlateExchangeBakInfo()
        {
            _msg = "";
            _name = "";
            _funcName = "";
            _nLevel = EWainLevel.EWL_success;
        }
    }

    public delegate void BroadcastExchangeInfo(PlateExchangeBakInfo info);

    public abstract class PlateExchangeAbstract
    {
        public event BroadcastExchangeInfo broadcastExchangeInfo;
        
        public void OnBroadcastExchangeInfo(PlateExchangeBakInfo info)
        {
            if(broadcastExchangeInfo != null)
            {
                broadcastExchangeInfo(info);
            }
        }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string Name { set; get; }

        public virtual bool Init(string userName, string pwd, string localHost)
        {
            PlateExchangeBakInfo _broadcastInfo = new PlateExchangeBakInfo();

            _broadcastInfo._funcName = "init";
            _broadcastInfo._name = Name;
            string strUserName = string.Empty, strPwd = string.Empty, strLocalHost = string.Empty;
            if (userName.Length < 1 || pwd.Length < 1 || localHost.Length < 1)
            {
                strUserName = "root";
                strPwd = "123456";
                strLocalHost = "localhost";
            }
            else
            {
                strUserName = userName;
                strPwd = pwd;
                strLocalHost = localHost;
            }

            bool bRet = DatabaseHandle.Single.Init(strUserName, strPwd, strLocalHost);
            return bRet;
        }

        public abstract bool LoginIn(string reserv);

        /// <summary>
        /// 名单查询
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="nType"></param>
        /// <returns></returns>
        public abstract int QueryBlack(string cardId, int nType = 0);

        public virtual int SaveIcCardInfo(string icNum, string idNum)
        {
            DataRow dr = DatabaseHandle.Single.IcCardTable.NewDataTable().NewRow();
            dr[icCardColum.idCardNumber.ToString()] = idNum;
            dr[icCardColum.icCardNumber.ToString()] = icNum;
            bool bNeed = ICCardNeedUpdate(icNum, idNum);
            if (!bNeed) return 1;
            int nRet = DatabaseHandle.Single.IcCardTable.InsertData(dr);
            if (nRet == 0) return -1;
            return 0;
        }

        public virtual int SaveCardInfo(SCardInfo info)
        {
            if(info._idNum == null || info._idNum.Length < 10)
            {
                return 2;
            }

            DataRow dr = DatabaseHandle.Single.IdCardTable.NewDataTable().NewRow();
            dr[IdCardColumn.idCardNumber.ToString()] = info._idNum;
            dr[IdCardColumn.personName.ToString()] = info._name != null ? info._name : "";
            dr[IdCardColumn.gender.ToString()] = info._sex != null ? info._sex : "";
            dr[IdCardColumn.ethnic.ToString()] = info._nation != null ? info._nation : "";
            dr[IdCardColumn.birthday.ToString()] = info._birthday != null ? info._birthday : "";
            dr[IdCardColumn.address.ToString()] = info._address != null ? info._address : "";
            dr[IdCardColumn.issue.ToString()] = info._issure != null ? info._issure : "";
            dr[IdCardColumn.dateStart.ToString()] = info._dateStart != null ? info._dateStart : "";
            dr[IdCardColumn.dateEnd.ToString()] = info._dateEnd != null ? info._dateEnd : "";
            dr[IdCardColumn.idCardPhoto.ToString()] = info._photo != null ? info._photo : "";
            bool bNeed = NeedUpdate(info._idNum, info._address, info._photo);
            if (!bNeed)
            {
                Trace.WriteLine("不需要更新证件信息");
                return 1;
            }

            int nRet = DatabaseHandle.Single.IdCardTable.InsertData(dr);
            if (nRet == 0)
            {
                Trace.WriteLine("更新证件信息失败");
                return -1;
            }
            return 0;
        }

        private bool NeedUpdate(string id, string addr, string photo)
        {
            bool bNeed = true;
            DataRow dr = DatabaseHandle.Single.IdCardTable.GetOneData(
                new List<QueryCondition<IdCardColumn>>
                { new QueryCondition<IdCardColumn>(IdCardColumn.idCardNumber, id) });
            if (dr == null) return bNeed;

            string address = dr[IdCardColumn.address.ToString()].ToString();
            string libPhoto = dr[IdCardColumn.idCardPhoto.ToString()].ToString();

            if (photo != null && File.Exists(photo) && addr != null && addr.Length > 1)
            {
                //Trace.WriteLine("写入的照片= " + photo + "原有的照片 = " + libPhoto);
                if (libPhoto.CompareTo(photo) != 0)
                {
                    //Trace.WriteLine("删除原有证件记录");
                    DatabaseHandle.Single.IdCardTable.DeleteData(new List<QueryCondition<IdCardColumn>> {
                        new QueryCondition<IdCardColumn>(IdCardColumn.idCardNumber,id),
                    });
                    return bNeed;
                }
            }
            if (addr == null || addr.Length == 0)
            {
                //Trace.WriteLine("addr 为空返回false");
                return false;
            }
            if (address != null && address.Length != 0)
            {
                return false;
            }
            return bNeed;
        }

        private bool ICCardNeedUpdate(string icNum, string idNum)
        {
            bool bNeed = true;
            DataRow dr = DatabaseHandle.Single.IcCardTable.GetOneData(new List<QueryCondition<icCardColum>>
            {
                new QueryCondition<icCardColum>(icCardColum.icCardNumber,icNum),
            });
            if (dr == null)
            {
                dr = DatabaseHandle.Single.IcCardTable.GetOneData(new List<QueryCondition<icCardColum>>
                {
                    new QueryCondition<icCardColum>(icCardColum.idCardNumber,idNum),
                });
                if (dr != null)
                {
                    DatabaseHandle.Single.IcCardTable.DeleteData(new List<QueryCondition<icCardColum>>
                {
                    new QueryCondition<icCardColum>(icCardColum.idCardNumber,idNum),
                });
                }
                return bNeed;
            }
            ///当绑定的身份证不一样时候  先删除再插入
            if (dr[icCardColum.idCardNumber.ToString()].ToString().CompareTo(idNum) != 0)
            {
                DatabaseHandle.Single.IcCardTable.DeleteData(new List<QueryCondition<icCardColum>>
                {
                    new QueryCondition<icCardColum>(icCardColum.icCardNumber,icNum),
                });
                return bNeed;
            }
            return false;
        }

        private bool RecordNeedUpdate(string id, string passTime)
        {
            bool bNeed = true;
            DataRow dr = DatabaseHandle.Single.FaceDetectTable.GetOneData(new List<QueryCondition<FaceDetectColumn>>
            {
                //new QueryCondition<FaceDetectColumn>(FaceDetectColumn.idCardNumber, id),
                new QueryCondition<FaceDetectColumn>(FaceDetectColumn.compareTime,passTime)
            });
            if (dr == null) return bNeed;

            return false;
        }

        public virtual int SavePersonRecord(SPersonRecord info)
        {
            if(info._idNum == null || info._idNum.Length < 8)
            {
                Trace.WriteLine("info._idNum 错误");
                return -1;
            }

            DataRow dr = DatabaseHandle.Single.FaceDetectTable.NewDataTable().NewRow();
            dr[FaceDetectColumn.idCardNumber.ToString()] = info._idNum;
            dr[FaceDetectColumn.compareTime.ToString()] = info._passTime;
            dr[FaceDetectColumn.detectMode.ToString()] = info._detectType;
            dr[FaceDetectColumn.detectResult.ToString()] = info._detectResult;
            dr[FaceDetectColumn.detectScore.ToString()] = info._detectScore;
            dr[FaceDetectColumn.personType.ToString()] = info._personType;
            dr[FaceDetectColumn.identity.ToString()] = info._identity;
            dr[FaceDetectColumn.imgScene.ToString()] = info._imgScene;
            dr[FaceDetectColumn.imgSite.ToString()] = info._imgSite;
            dr[FaceDetectColumn.imgCard.ToString()] = "";
            bool bNeed = RecordNeedUpdate(info._idNum, info._passTime);
            if (!bNeed) return 1;

            int nRet = -1;

            nRet = DatabaseHandle.Single.FaceDetectTable.InsertData(dr);
            

            if (nRet == 0) return -1;
            else
            {
                Trace.WriteLine("比对记录保存成功");
            }
            return 0;
        }
        public abstract int SaveCarRecord(SCarRecord info);

    }
}
