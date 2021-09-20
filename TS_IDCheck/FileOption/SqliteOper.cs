using Common;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_IDCheck
{
    public class SqliteOper
    {
        private SqliteHelper _sqliteOper;
        public void DataBaseInit()
        {
            try
            {
                if(_sqliteOper == null)
                {
                    _sqliteOper = new SqliteHelper();
                }
                _sqliteOper.ConnectToDatabase();
            }catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            //创建数据表
            string[] strColName = { "idnum", "name", "sex", "birthday", "nation", "issure", "address", "dateStart", "dateEnd" };
            string[] strColType = { "varchar(20)", "varchar(32)", "varchar(4)", "varchar(32)", "varchar(16)", "varchar(64)", "varchar(128)", "varchar(16)", "varchar(16)" };
            _sqliteOper.CreateTable("idCardInfo", strColName, strColType);

            string[] strCardRecordColName = { "idnum", "passtime","blackJobId", "comparescore", "compareresult", "cardimg", "sceneimg", "faceimg", "verifyResult", "nFlag", "reserved1" };
            string[] strCardRecordColType = { "varchar(20)", "varchar(32)", "varchar(40)","varchar(8)", "varchar(8)", "varchar(128)", "varchar(128)", "varchar(128)", "varchar(4)", "varchar(4)", "varchar(16)" };
            _sqliteOper.CreateTable("idCardRecord", strCardRecordColName, strCardRecordColType);
        }

        public bool DatabaseInsert(string strTableName,string[] strColValue)
        {
            if(strTableName.CompareTo("idCardInfo") == 0)
            {
                if(strColValue.Length != 9)
                {
                    return false;
                }
            }else if(strTableName.CompareTo("idCardRecord") == 0)
            {
                if(strColValue.Length != 11)
                {
                    return false;
                }
            }

            _sqliteOper.InsertValues(strTableName, strColValue);
            return true;
        }

        public bool DatabaseReadCardInfo(string idNum,ref SCardInfo info)
        {
            string[] strColName = { "idnum", "name", "sex", "birthday", "nation", "issure", "address", "dateStart", "dateEnd" };
            string[] items = { "idnum" };
            string[] operation = { "=" };
            string[] colValue = { idNum };

            SQLiteDataReader reader = _sqliteOper.ReadTable("idCardInfo", strColName, items, operation, colValue);
            if (reader.Read())
            {
                int nIndex = 0;
                info._idNum = reader.GetString(nIndex++);
                info._name = reader.GetString(nIndex++);
                info._sex = reader.GetString(nIndex++);
                info._birthday = reader.GetString(nIndex++);
                info._nation = reader.GetString(nIndex++);
                info._issure = reader.GetString(nIndex++);
                info._address = reader.GetString(nIndex++);
                info._dateStart = reader.GetString(nIndex++);
                info._dateEnd = reader.GetString(nIndex++);
                return true;
            }
            return false;
        }

        public bool DatabaseReadRecordInfo(ref CUploadRecordData data,string strFlag = "0")
        {
            string[] strCardRecordColName = { "idnum", "passtime", "blackJobId", "comparescore", "compareresult", "cardimg", "sceneimg", "faceimg", "verifyResult", "nFlag", "reserved1" };
            string[] items = { "nFlag" };
            string[] operation = { "=" };
            string[] colValue = { strFlag };
            SQLiteDataReader reader = _sqliteOper.ReadTable("idCardRecord", strCardRecordColName, items, operation, colValue);
            if (reader.Read())
            {
                int nIndex = 0;
                data._idNum = reader.GetString(nIndex++);
                data._passtime = reader.GetString(nIndex++);
                data._blackJobId = reader.GetString(nIndex++);
                data._compareScore = reader.GetString(nIndex++);
                data._compareResult = reader.GetString(nIndex++);
                data._cardImg = reader.GetString(nIndex++);
                data._sceneImg = reader.GetString(nIndex++);
                data._faceImg = reader.GetString(nIndex++);
                data._verifyResult = reader.GetString(nIndex++);
                data._strFlag = reader.GetString(nIndex++);

                return true;
            }
            return false;
        }

        public bool DatabaseReadRecordInfo(ref CUploadRecordData data,string idNum ,string passTime)
        {
            string[] strCardRecordColName = { "idnum", "passtime", "blackJobId", "comparescore", "compareresult", "cardimg", "sceneimg", "faceimg", "verifyResult", "nFlag", "reserved1" };
            string[] items = { "idnum", "passtime" };
            string[] operation = { "=" };
            string[] colValue = { idNum,passTime };
            
            SQLiteDataReader reader = _sqliteOper.ReadTable("idCardRecord", strCardRecordColName, items, operation, colValue);
            if (reader.Read())
            {
                int nIndex = 0;
                data._idNum = reader.GetString(nIndex++);
                data._passtime = reader.GetString(nIndex++);
                data._blackJobId = reader.GetString(nIndex++);
                data._compareScore = reader.GetString(nIndex++);
                data._compareResult = reader.GetString(nIndex++);
                data._cardImg = reader.GetString(nIndex++);
                data._sceneImg = reader.GetString(nIndex++);
                data._faceImg = reader.GetString(nIndex++);
                data._verifyResult = reader.GetString(nIndex++);
                data._strFlag = reader.GetString(nIndex++);
                return true;
            }
            return false; 
            
        }

        public void DatabaseUpdateRecord(string idNum,string passTime,string strFlag)
        {
            string[] strCardRecordColName = { "idnum", "passtime", "blackJobId", "comparescore", "compareresult", "cardimg", "sceneimg", "faceimg", "verifyResult", "nFlag", "reserved1" };
            string[] items = { "nFlag", };
            string[] operation = { "=" };
            string[] colValue = { strFlag };
            _sqliteOper.UpdateValues("idCardRecord", items, colValue, "passtime", passTime);
        }

        public void DatabaseDeleteRecord(string idNum,string passTime)
        {
            string[] strCardRecordColName = { "idnum", "passtime", "blackJobId", "comparescore", "compareresult", "cardimg", "sceneimg", "faceimg", "verifyResult", "nFlag", "reserved1" };
            string[] items = { "idnum", "passtime" };
            string[] operation = { "=" ,"="};
            string[] colValue = { idNum, passTime };

            _sqliteOper.DeleteValuesAND("idCardRecord", items, colValue, operation);
        }



    }
}
