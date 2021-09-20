using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TS_IDCheck
{
    public enum EDetectResult
    {
        EDR_UNKNOW = 0,         //未知
        EDR_SUCCESS,            //比对成功
        EDR_FAILED,             //比对失败
        EDR_REGISTER,           //未注册
        EDR_WARNING,            //预警人员
        EDR_REREADCARD,         //重刷身份证
        EDR_TICKETINFOERR,      //车票信息过期(包括多种情况，需要车票查询返回信息确认具体情况)
        EDR_TICKETNETFAIL,      //车票查询网络错误
        EDR_TICKETON,           //车辆发出
        EDR_DATEINVALID,        //证件过期
        EDR_TICKETCHECKED,      //已检票(放行)
    }
    

    public class DetectThreadHelper
    {
        [DllImport("GetTicketLib.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static void GetTicketInfo(string url ,string signNum, StringBuilder strOut);
        StringBuilder outInfoData = new StringBuilder(1024);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strNum"></param>
        /// <param name="msg"></param>
        /// <returns>0表示成功，1表示失败 2表示网络错误 3车辆已发班 4表示已检票(放行)</returns>
        public int IsCheckTicketSuccess(string strNum,out string msg,out string strRetMsg)
        {
            
            int nRet = 1;
            msg = ""; strRetMsg = "";
            if (ConfigOperator.Single().GetTicketInfo != 1)
            {
                msg = "不检测车票";
                return 0;
            }
            try
            {
                string strUrl = ConfigOperator.Single().GetTicketUrl;
                Trace.WriteLine("strUrl = " + strUrl);
                GetTicketInfo(strUrl,strNum, outInfoData);
                string strRet = outInfoData.ToString();
                strRetMsg = strRet;
                if(strRet.IndexOf("error") != -1)
                {
                    msg = "网络错误";
                    return 2;
                }
                Trace.WriteLine(strNum + " : " + strRet);
                string[] ss = strRet.Split('|');
                if (ss != null && ss.Length > 0 )
                {
                    if(ss[0].CompareTo("0") == 0)
                    {
                        nRet = 0;
                        msg = "0";

                        if(ss.Length > 13)
                        {
                            if(ss[12].CompareTo("1") == 0)
                            {
                                nRet = 4;
                            }
                            if(ss[13].CompareTo("1") == 0)
                            {
                                nRet = 3;
                            }
                        }
                    }else
                    {
                        if (ss.Length > 1)
                        {
                            msg = ss[1];
                        }
                    }
                }
                
                return nRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("IsCheckTicketSuccess error: " + ex.Message);
                return nRet;
            }
        }
        private string GetIpFromUrl(string strUrl)
        {
            string strIp = "";

            strIp = strUrl.Substring(7);
            int nPos = strIp.IndexOf(':');
            strIp = strIp.Substring(0, nPos);

            return strIp;
        }
        public string QueryBlack(string queryNum)
        {
            ThirdBlackQurryMsg msg = new ThirdBlackQurryMsg();
            msg.token = ConfigOperator.Single().StrThreeToken;
            msg.cardNo = queryNum;
            msg.deviceCode = ConfigOperator.Single().StrThreeDeviceCode;
            int nPlateChoose = ConfigOperator.Single().PlateChoose;

            string strBlackUrl = "";
            string strIp = "";
            if (nPlateChoose == 1 || nPlateChoose == 4)
            {
                //公安网
                strBlackUrl = ConfigOperator.Single().QueryBlackUrlPolice; //"http://10.20.50.110:9092/verificationInterface/passlog/blackList";
                
            }
            else if (nPlateChoose == 2)
            {
                //视频网
                strBlackUrl = ConfigOperator.Single().QueryBlackUrlVedio;
            }
            else if (nPlateChoose == 3)
            {
                //互联网
                strBlackUrl = ConfigOperator.Single().QueryBlackUrlNet;
            }
            strIp = GetIpFromUrl(strBlackUrl);
            bool bPing = Ping(strIp);
#if !DEBUG
            string strQueryBack = SanTranProto.Single().ThirdQueryBlack(msg, strBlackUrl);
#else
                string strQueryBack = "";
#endif
            return strQueryBack;
        }

        public ThirdBlackQurryReturn QueryArmPlateBlack(string queryNum)
        {
            ThirdBlackQurryReturn mdc = new ThirdBlackQurryReturn();
            mdc.isBlackList = 0;
            string strToken = ConfigOperator.Single().StrThreeToken;

            ThirdBlackQurryMsg msg = new ThirdBlackQurryMsg();
            msg.token = strToken;
            msg.cardNo = queryNum;
            msg.deviceCode = ConfigOperator.Single().StrThreeDeviceCode;
            string strQueryBack = "";
            string strBlackUrl = ConfigOperator.Single().QueryBlackUrlArmy;
            strQueryBack = SanTranProto.Single().ThirdQueryBlack(msg, strBlackUrl);
            Trace.WriteLine("兵团黑名单返回：" + strQueryBack);

            if((strQueryBack != null) && (strQueryBack.IndexOf("isBlackList") != -1))
            {
                mdc = JsonConvert.DeserializeObject<ThirdBlackQurryReturn>(strQueryBack);
            }
            return mdc;
        }


        /// <summary>
        /// 是否能 Ping 通指定的主机
        /// </summary>
        /// <param name="ip">ip 地址或主机名或域名</param>
        /// <returns>true 通，false 不通</returns>
        public bool Ping(string ip)
        {
            System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
            options.DontFragment = true;
            string data = "Test Data!";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 300; // Timeout 时间，单位：毫秒
            try
            {
                System.Net.NetworkInformation.PingReply reply = p.Send(ip, timeout, buffer, options);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return false;
            }

        }

        public EDetectResult GetDetectResult(float fScore,int nManType,int bGetTickValid = 0,string dateEnd = "")
        {
            EDetectResult nRet = EDetectResult.EDR_FAILED;
            float fConfigSocre = (float)Convert.ToDouble(ConfigOperator.Single().FDetectScore);
            do
            {
                if(nManType == 1)
                {
                    nRet = EDetectResult.EDR_WARNING;
                    break;
                }


                if (dateEnd.Length != 0)
                {
                    DateTime dtEnd;
                    bool bChangeDate = DateTime.TryParse(dateEnd, out dtEnd);
                    if (bChangeDate)
                    {
                        int compNum = DateTime.Compare(dtEnd, DateTime.Now);
                        if(compNum < 0)
                        {
                            nRet = EDetectResult.EDR_DATEINVALID;
                            break;
                        }
                    }else
                    {
                        Trace.WriteLine("日期数据转换失败：" + dateEnd);
                    }
                }
                if(fScore >= (float)Convert.ToDouble(ConfigOperator.Single().FDetectScore))
                {
                    nRet = EDetectResult.EDR_SUCCESS;
                }
                if (nRet == EDetectResult.EDR_SUCCESS && (bGetTickValid != 0))
                {
                    if(bGetTickValid == 1)
                    {
                        nRet = EDetectResult.EDR_TICKETINFOERR;     //车票过期
                    }else if(bGetTickValid == 2)
                    {
                        nRet = EDetectResult.EDR_TICKETNETFAIL;     //车票查询网络错误
                    }else if(bGetTickValid == 3)
                    {
                        nRet = EDetectResult.EDR_TICKETON;
                    }else if(bGetTickValid == 4)
                    {
                        nRet = EDetectResult.EDR_TICKETCHECKED;
                    }
                }
            } while (false);
            return nRet;
        }


    }
}
