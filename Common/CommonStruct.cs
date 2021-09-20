using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SCardInfo
    {
        public string _name;
        public string _sex;
        public string _birthday;
        public string _nation;
        public string _idNum;
        public string _address;
        public string _issure;
        public string _dateStart;
        public string _dateEnd;
        public string _photo;
        public string _photoBase64;

        public SCardInfo()
        {
            _name = "";
            _sex = "";
            _birthday = "";
            _nation = "";
            _idNum = "";
            _address = "";
            _issure = "";
            _dateStart = "";
            _dateEnd = "";
            _photo = "";
            _photoBase64 = "";
        }
        public bool IsEmpty()
        {
            if (this._idNum == string.Empty)
            {
                return true;
            }
            return false;
        }

        public bool equals(SCardInfo data)
        {
            if (data.IsEmpty())
            {
                return false;
            }
            if (data._idNum.CompareTo(this._idNum) == 0 && data._name.CompareTo(this._name) == 0)
            {
                return true;
            }
            return false;
        }

        public void ClearData(SCardInfo data)
        {
            data._name = string.Empty;
            data._sex = string.Empty;
            data._birthday = string.Empty;
            data._idNum = string.Empty;
            data._address = string.Empty;
            data._nation = string.Empty;
            data._issure = string.Empty;
            data._dateStart = string.Empty;
            data._dateEnd = string.Empty;
            data._photo = string.Empty;
        }

        public void ClearData()
        {
            _name = string.Empty;
            _sex = string.Empty;
            _birthday = string.Empty;
            _idNum = string.Empty;
            _address = string.Empty;
            _nation = string.Empty;
            _issure = string.Empty;
            _dateStart = string.Empty;
            _dateEnd = string.Empty;
            _photo = string.Empty;
        }
        public void CopyData(SCardInfo data)
        {
            this._name = data._name;
            this._sex = data._sex;
            this._nation = data._nation;
            this._birthday = data._birthday;
            this._idNum = data._idNum;
            this._address = data._address;
            this._dateStart = data._dateStart;
            this._dateEnd = data._dateEnd;
            this._issure = data._issure;
            this._photo = data._photo;
        }
    }

    /// <summary>
    /// 汽车抓拍信息
    /// </summary>
    public struct SCarRecord
    {
        public string _carNum;
        public string _idNum;
        public string _imgCar;
        public string _passTime;
        public string _carColor;
        public string _imgCarNum;
        public string _carType;
    }

    public struct SPersonRecord
    {
        public string _idNum;
        public string _passTime;
        public string _detectType;
        public string _detectResult;
        public string _detectScore;
        public string _personType;

        public string _imgScene;
        public string _imgSite;
        public string _identity;
    }

    /// <summary>
    /// 告警信息提示等级
    /// </summary>
    public enum EWainLevel
    {
        EWL_success = 0,            //显示提示信息
        EWL_debug,                  //警告信息
        EWL_waining,                //一般错误信息，容易引起未知发生 
        EWL_error,                  //程序运行已出错，需要重新启动或上报开发者解决
    }
}
