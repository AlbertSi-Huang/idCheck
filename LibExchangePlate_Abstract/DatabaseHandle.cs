using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlLibrary;
using System.Diagnostics;

namespace LibExchangePlate
{
    public enum IdCardColumn
    {
        idCardNumber,   //身份证号
        personName,     //姓名
        gender,         //性别
        ethnic,         //民族
        birthday,       //出生日期
        address,        //住址
        issue,          //签发机关
        dateStart,      //有效期限-开始
        dateEnd,        //有效期限-结束
        idCardPhoto,    //身份证照片
    }

    /// <summary>
    /// ic卡信息
    /// </summary>
    public enum icCardColum
    {
        icCardNumber,
        idCardNumber,
    }

    /// <summary>
    /// 人脸比对列表
    /// </summary>
    public enum FaceDetectColumn
    {
        compareTime,
        idCardNumber,
        detectMode,     //刷卡，手动输入，白名单
        detectResult,
        detectScore,
        personType,
        imgCard,
        imgScene,
        imgSite,
        identity,           //身份
        reserved0,
    }

    /// <summary>
    /// 汽车抓拍
    /// </summary>
    public enum ECarCapture
    {
        passTime,           //通过时间
        idCardNumber,
        carNum,
        imgCar,
        imgCarNum,
        carColor,
        carType,
        reserved0,
    }

    public enum EUploadPlate
    {
        passTime,
        idCardNumber,
        carNumber,
        reserved0,         //      保留
    }
    
    public class DatabaseHandle
    {
        static object objLock = new object();
        static readonly string DATABASENAME = "TS_UploadPlate";

        public static readonly TableColumnParam<IdCardColumn>[] IdCardColumns =
        {
            new TableColumnParam<IdCardColumn>(IdCardColumn.idCardNumber,  "varchar(32)",false,EKeyType.PRI),
            new TableColumnParam<IdCardColumn>(IdCardColumn.personName,  "varchar(48)"),
            new TableColumnParam<IdCardColumn>(IdCardColumn.gender,  "varchar(4)"),
            new TableColumnParam<IdCardColumn>(IdCardColumn.ethnic,  "varchar(8)"),
            new TableColumnParam<IdCardColumn>(IdCardColumn.birthday,  "varchar(16)"),
            new TableColumnParam<IdCardColumn>(IdCardColumn.address,  "varchar(256)"),
            new TableColumnParam<IdCardColumn>(IdCardColumn.issue,  "varchar(64)"),
            new TableColumnParam<IdCardColumn>(IdCardColumn.dateStart,  "varchar(16)"),
            new TableColumnParam<IdCardColumn>(IdCardColumn.dateEnd,  "varchar(16)"),
            new TableColumnParam<IdCardColumn>(IdCardColumn.idCardPhoto,  "varchar(256)"),
        };

        public static readonly TableColumnParam<icCardColum>[] icCardColumns =
        {
            new TableColumnParam<icCardColum>(icCardColum.icCardNumber,"varchar(32)",false,EKeyType.PRI),
            new TableColumnParam<icCardColum>(icCardColum.idCardNumber,"varchar(32)"),
        };

        public static readonly TableColumnParam<FaceDetectColumn>[] CompareColumns =
        {
            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.compareTime, "varchar(32)",false,EKeyType.PRI),
            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.idCardNumber,  "varchar(32)"),

            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.detectMode,  "tinyint"),
            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.detectResult,  "tinyint"),
            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.detectScore,  "varchar(10)"),
            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.personType,"tinyint"),

            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.imgCard,  "varchar(256)"),
            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.imgScene,  "varchar(256)"),
            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.imgSite,  "varchar(256)"),

            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.identity,"varchar(4)"),
            new TableColumnParam<FaceDetectColumn>(FaceDetectColumn.reserved0,"varchar(16)"),
        };

        public static readonly TableColumnParam<ECarCapture>[] CarRecord =
        {
            new TableColumnParam<ECarCapture>(ECarCapture.passTime,  "varchar(32)",false,EKeyType.PRI),   //BINARY:字符集的二元校对规则的简写。排序和比较基于数值字符值。因此也就自然区分了大小写。    
            new TableColumnParam<ECarCapture>(ECarCapture.carNum,"varchar(16)"),
            new TableColumnParam<ECarCapture>(ECarCapture.idCardNumber,"varchar(32)"),
            new TableColumnParam<ECarCapture>(ECarCapture.imgCar,"varchar(256)"),
            new TableColumnParam<ECarCapture>(ECarCapture.imgCarNum,"varchar(256)"),
            new TableColumnParam<ECarCapture>(ECarCapture.carColor,"varchar(16)"),
            new TableColumnParam<ECarCapture>(ECarCapture.carType,"varchar(16)"),
            new TableColumnParam<ECarCapture>(ECarCapture.reserved0,"varchar(16)"),
        };

        private static DatabaseHandle _instance = null;
        public static DatabaseHandle Single
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new DatabaseHandle();
                }
                return _instance;
            }
        }

        private DatabaseHandle()
        {

        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <param name="localHost"></param>
        /// <returns></returns>
        private bool InitDatabase(string userName, string pwd, string localHost)
        {
            bool bRet = DbHandle.Init(DATABASENAME, localHost, userName, pwd);
            return bRet;
        }

        public TableHandle<IdCardColumn> IdCardTable { get; private set; }

        public TableHandle<icCardColum> IcCardTable { get; private set; }

        //比对记录表
        public TableHandle<FaceDetectColumn> FaceDetectTable { get; private set; }

        public TableHandle<ECarCapture> CarRecords { get; private set; }

        private  bool InitTable()
        {
            IdCardTable = new TableHandle<IdCardColumn>(nameof(IdCardTable), DATABASENAME, IdCardColumns);
            if (!IdCardTable.CreateTable())
            {
                return false;
            }

            IcCardTable = new TableHandle<icCardColum>(nameof(IcCardTable), DATABASENAME, icCardColumns);
            if (!IcCardTable.CreateTable())
            {
                return false;
            }

            FaceDetectTable = new TableHandle<FaceDetectColumn>(nameof(FaceDetectTable), DATABASENAME, CompareColumns);

            if (!FaceDetectTable.CreateTable())
            {
                return false;
            }

            CarRecords = new TableHandle<ECarCapture>(nameof(CarRecords), DATABASENAME, CarRecord);
            if (!CarRecords.CreateTable())
            {
                return false;
            }
            return true;
        }

        bool _bInited = false;

        public  bool Init(string userName,string pwd,string localHost)
        {
            if (_bInited) return _bInited;

            if (!InitDatabase(userName, pwd, localHost))
                return false;

            if (!InitTable())
            {
                Trace.WriteLine("创建数据表失败");
                return false;
            }
            _bInited = true;
            return true;
        }
        

    }
}
