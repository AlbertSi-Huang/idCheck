using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExeReadPassport630
{
    public class PassportInfo
    {
        private string _CardType;//卡类型：0-身份证，2-护照
        private string _Type;//护照类型
        private string _MRZ;//护照号码MRZ
        private string _Name;//本国姓名（护照版面识别）
        private string _EnglishName;//英文姓名
        private string _Sex;//性别
        private string _DateOfBirth;//生日
        private string _DateOfExpiry;//有效期至
        private string _CountryCode;//签发国代码
        private string _EnglishFamilyName; //英文姓
        private string _EnglishGivienName;//英文名
        private string _MRZ1;//MRZ1
        private string _MRZ2;//MRZ2
        private string _Nationality;//持证人国籍代码
        private string _PassportNo;//护照号码（直接识别）
        private string _PlaceOfBirth;//出生地（仅限中国护照）
        private string _PlaceOfIssue;//签发地点（仅限中国护照）
        private string _DateOfIssue;//签发日期（仅限中国护照）
        private string _RFIDMRZ;//RFID MRZ
        private string _OCRMRZ;//OCR MRZ
        private string _PlaceOfBirthPinyin;//出生地拼音（仅限中国护照）
        private string _PlaceOfIssuePinyin;//签发地点拼音（仅限中国护照）
        private string _PersonalIdNo;//身份证号码（仅限台湾和韩国护照）
        private string _NamePinyinOCR;//本国姓名拼音OCR
        private string _SexOCR;//性别OCR
        private string _NationalityOCR;//持证人国籍代码OCR
        private string _PersonalIdNoOCR;//身份证号码OCR
        private string _PlaceOfBirthOCR;//出生地OCR（仅限中国护照）
        private string _DateOfExpiryOCR;//有效期至OCR
        private string _AuthorityOCR;//签发机关OCR
        private string _FamilyName;//本国姓
        private string _GivienName;//本国名
        private string _Photo;//可见光图像
        private string _PhotoHead;//证件头像图像
        private string _Nation;//民族

        public string CardType
        {
            get { return _CardType; }
            set { _CardType = value; }
        }
        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        public string MRZ
        {
            get { return _MRZ; }
            set { _MRZ = value; }
        }
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        public string EnglishName
        {
            get { return _EnglishName; }
            set { _EnglishName = value; }
        }
        public string Sex
        {
            get { return _Sex; }
            set { _Sex = value; }
        }
        public string DateOfBirth
        {
            get { return _DateOfBirth; }
            set { _DateOfBirth = value; }
        }
        public string DateOfExpiry
        {
            get { return _DateOfExpiry; }
            set { _DateOfExpiry = value; }
        }
        public string CountryCode
        {
            get { return _CountryCode; }
            set { _CountryCode = value; }
        }
        public string EnglishFamilyName
        {
            get { return _EnglishFamilyName; }
            set { _EnglishFamilyName = value; }
        }
        public string EnglishGivienName
        {
            get { return _EnglishGivienName; }
            set { _EnglishGivienName = value; }
        }
        public string MRZ1
        {
            get { return _MRZ1; }
            set { _MRZ1 = value; }
        }
        public string MRZ2
        {
            get { return _MRZ2; }
            set { _MRZ2 = value; }
        }
        public string Nationality
        {
            get { return _Nationality; }
            set { _Nationality = value; }
        }
        public string PassportNo
        {
            get { return _PassportNo; }
            set { _PassportNo = value; }
        }
        public string PlaceOfBirth
        {
            get { return _PlaceOfBirth; }
            set { _PlaceOfBirth = value; }
        }
        public string PlaceOfIssue
        {
            get { return _PlaceOfIssue; }
            set { _PlaceOfIssue = value; }
        }
        public string DateOfIssue
        {
            get { return _DateOfIssue; }
            set { _DateOfIssue = value; }
        }
        public string RFIDMRZ
        {
            get { return _RFIDMRZ; }
            set { _RFIDMRZ = value; }
        }
        public string OCRMRZ
        {
            get { return _OCRMRZ; }
            set { _OCRMRZ = value; }
        }
        public string PlaceOfBirthPinyin
        {
            get { return _PlaceOfBirthPinyin; }
            set { _PlaceOfBirthPinyin = value; }
        }
        public string PlaceOfIssuePinyin
        {
            get { return _PlaceOfIssuePinyin; }
            set { _PlaceOfIssuePinyin = value; }
        }
        public string PersonalIdNo
        {
            get { return _PersonalIdNo; }
            set { _PersonalIdNo = value; }
        }
        public string NamePinyinOCR
        {
            get { return _NamePinyinOCR; }
            set { _NamePinyinOCR = value; }
        }
        public string SexOCR
        {
            get { return _SexOCR; }
            set { _SexOCR = value; }
        }
        public string NationalityOCR
        {
            get { return _NationalityOCR; }
            set { _NationalityOCR = value; }
        }
        public string PersonalIdNoOCR
        {
            get { return _PersonalIdNoOCR; }
            set { _PersonalIdNoOCR = value; }
        }
        public string PlaceOfBirthOCR
        {
            get { return _PlaceOfBirthOCR; }
            set { _PlaceOfBirthOCR = value; }
        }
        public string DateOfExpiryOCR
        {
            get { return _DateOfExpiryOCR; }
            set { _DateOfExpiryOCR = value; }
        }
        public string AuthorityOCR
        {
            get { return _AuthorityOCR; }
            set { _AuthorityOCR = value; }
        }
        public string FamilyName
        {
            get { return _FamilyName; }
            set { _FamilyName = value; }
        }
        public string GivienName
        {
            get { return _GivienName; }
            set { _GivienName = value; }
        }

        public string Photo
        {
            get { return _Photo; }
            set { _Photo = value; }
        }

        public string PhotoHead
        {
            get { return _PhotoHead; }
            set { _PhotoHead = value; }
        }

        public string Nation
        {
            get { return _Nation; }
            set { _Nation = value; }
        }

        /// <summary>
        /// 缺省构造
        /// </summary>
        public PassportInfo()
        {
            _PersonalIdNo = "";
            _PassportNo = "";
            _Name = ""; _Sex = ""; _DateOfBirth = "";
        }

        /// <summary>
        /// 拷贝构造
        /// </summary>
        /// <param name="cPP"></param>
        public PassportInfo(PassportInfo cPP)
        {

        }

    }
}
