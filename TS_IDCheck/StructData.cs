using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_IDCheck
{
    //public struct SCardInfo
    //{
    //    public string _name;
    //    public string _sex;
    //    public string _birthday;
    //    public string _nation;
    //    public string _idNum;
    //    public string _address;
    //    public string _issure;
    //    public string _dateStart;
    //    public string _dateEnd;
    //    public string _photo;

    //    public bool IsEmpty()
    //    {
    //        if (this._idNum == string.Empty)
    //        {
    //            return true;
    //        }
    //        return false;
    //    }

    //    public bool equals(SCardInfo data)
    //    {
    //        if (data.IsEmpty())
    //        {
    //            return false;
    //        }
    //        if (data._idNum.CompareTo(this._idNum) == 0 && data._name.CompareTo(this._name) == 0)
    //        {
    //            return true;
    //        }
    //        return false;
    //    }

    //    public void ClearData()
    //    {
    //        _name = string.Empty;
    //        _sex = string.Empty;
    //        _birthday = string.Empty;
    //        _idNum = string.Empty;
    //        _address = string.Empty;
    //        _nation = string.Empty;
    //        _issure = string.Empty;
    //        _dateStart = string.Empty;
    //        _dateEnd = string.Empty;
    //        _photo = string.Empty;
    //    }

    //    public void CopyData(SCardInfo data)
    //    {
    //        this._name = data._name;
    //        this._sex = data._sex;
    //        this._nation = data._nation;
    //        this._birthday = data._birthday;
    //        this._idNum = data._idNum;
    //        this._address = data._address;
    //        this._dateStart = data._dateStart;
    //        this._dateEnd = data._dateEnd;
    //        this._issure = data._issure;
    //        this._photo = data._photo;
    //    }
    //}


    public class CUploadRecordData
    {
        
        public string _idNum { set; get; }
        public string _passtime { set; get; }
        public string _blackJobId { set; get; }
        public string _compareScore { set; get; }

        public string _compareResult { set; get; }

        public string _cardImg { set; get; }
        public string _sceneImg { set; get; }
        public string _faceImg { set; get; }

        public string _strFlag { set; get; }

        public string _verifyResult { set; get; }

        public void Clear()
        {
            _idNum = "";
            _passtime = "";
            _blackJobId = "";
            _compareScore = "";
            _compareResult = "";
            _cardImg = "";
            _sceneImg = "";
            _faceImg = "";
            _strFlag = ""; 
        }
    }
}
