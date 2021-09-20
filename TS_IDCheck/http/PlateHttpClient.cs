using System;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Diagnostics;
using Common;

namespace TS_IDCheck.http
{
    public struct SPersonInfo
    {
        public string _personName;
        public string _cardNo;
        public string _genderCode;
        public string _nationCode;
        public string _addr;
        public string _photo;       //base64
    }

    public struct SHttpBackInfo
    {
        public string _status;
        public string _message;
        public SPersonInfo _spi;
    }

    public class PlateHttpClient : BaseThread
    {

        //CPHttpClient cpHc = new CPHttpClient(ConfigLoad.ServerIP, ConfigLoad.ServerPort);

        bool _bNeedSearch = false;
        string _strSearchNum = "";
        object objLock = new object();

        public bool InitPlateHttp()
        {
            return true;
        }

        private void PlateHttpClientNoCardSearchEvent(string idNum)
        {
            if(idNum != null && idNum.Length == 18)
            {
                _bNeedSearch = true;
                _strSearchNum = idNum;
            }
        }

        public override void Run()
        {
            while(IsAlive)
            {
                if(!_bNeedSearch)
                {
                    Thread.Sleep(500);
                    continue;
                }

                //查找常住人口~~~~~~~~  如果副屏程序做此处不需要调用
                string strPlateBack =  SearchResidentPopleInfo(_strSearchNum);
                Console.WriteLine(strPlateBack);
                Trace.WriteLine(strPlateBack);

                if (strPlateBack.Length > 0)
                {
                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(strPlateBack);
                    using (MemoryStream ms = new MemoryStream(byteArray))
                    {
                        DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(SHttpBackInfo));
                        SHttpBackInfo sbi = (SHttpBackInfo)deseralizer.ReadObject(ms);

                        if (sbi._status.CompareTo("0") == 0)
                        {
                            //常驻人口
                            //m_cardData._name = sbi._spi._personName;
                            SCardInfo cardInfo = new SCardInfo();
                            cardInfo._idNum = sbi._spi._cardNo;
                            cardInfo._name = sbi._spi._personName;
                            cardInfo._nation = sbi._spi._nationCode;
                        }
                        else if (sbi._status.CompareTo("-1022") == 0)
                        {
                            //非常驻人口 


                        }
                        else if (sbi._status.CompareTo("-1001") == 0)
                        {
                            //系统错误  请确认此流程
                        }
                    }
                }

            }
            
        }

        public PlateHttpClient()
        {

        }
        

        /// <summary>
        /// 查找常驻人口接口 
        /// </summary>
        /// <param name="idNum"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string SearchResidentPopleInfo(string idNum,string name = "")
        {
            //if(cpHc != null)
            //{
            //    return cpHc.SearchResidentPopleInfo(idNum, name);
            //}
            return "";
        }




    }
}
