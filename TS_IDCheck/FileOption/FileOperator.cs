using System;
using System.Collections.Generic;
using System.IO;
using Common;
using System.Runtime.InteropServices;
using System.Threading;
using TS_IDCheck.Info;
using System.Diagnostics;

namespace TS_IDCheck
{


    public class FileOperator
    {
        private string strSavePath;
        private string strCardImgPath,strSitePath,strScenecsPath,strHistoryPath;
        private string strCardInfoFile,strRecordFile,strJsonRecord;
        private string strPassportInfoFile;//护照
        private Base64Crypt crypt;
        private bool bInited = false; 
        
        public bool BInited
        {
            set { bInited = value; }
            get { return bInited; }
        }

        public FileOperator()
        {
            if (!ConfigOperator.Single().BInited)
            {
                ConfigOperator.Single().InitConfig();
            }
             strSavePath = ConfigOperator.Single().StrSavePath;
            crypt = new Base64Crypt();
            strCardInfoFile = strSavePath + @"\" + "cardInfo.txt";
            strPassportInfoFile = strSavePath + @"\" + "passportInfo.txt";
            strRecordFile = strSavePath + @"\" + "record.txt";
            strCardImgPath = strSavePath + @"\" + "cardImg";
            strHistoryPath = strSavePath + @"\history";
            strSitePath = strSavePath + @"\" + "siteImg";

            strScenecsPath = strSavePath + @"\" + "sceceImg";
            strJsonRecord = strSavePath + @"\" + "jsonRecord.txt";
        }

        /// <summary>
        /// 创建文件保存路径和相应的文件
        /// </summary>
        /// <returns></returns>
        public bool InitFileOper()
        {
            //Int32 i =0;
     
            if (Directory.Exists(strSavePath) == false)//如果不存在就创建file文件夹
            {
                try
                {
                    Directory.CreateDirectory(strSavePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            if(Directory.Exists(strCardImgPath) == false)
            {
                try
                {
                    Directory.CreateDirectory(strCardImgPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            if (Directory.Exists(strSitePath) == false)
            {
                try
                {
                    Directory.CreateDirectory(strSitePath);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    return false;
                }
            }

            if(Directory.Exists(strHistoryPath) == false)
            {
                try
                {
                    Directory.CreateDirectory(strHistoryPath);
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            if(Directory.Exists(strScenecsPath) == false)
            {
                try
                {
                    Directory.CreateDirectory(strScenecsPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            if (File.Exists(strCardInfoFile) == false)
            {
                FileStream fs1 = new FileStream(strCardInfoFile, FileMode.Create, FileAccess.Write);//创建写入文件 
                fs1.Close();
            }

            if (File.Exists(strPassportInfoFile) == false)//护照
            {
                FileStream fs1 = new FileStream(strPassportInfoFile, FileMode.Create, FileAccess.Write);//创建写入文件 
                fs1.Close();
            }

            if (!File.Exists(strJsonRecord))
            {
                FileStream fs1 = new FileStream(strJsonRecord, FileMode.Create, FileAccess.Write);//创建写入文件 
                fs1.Close();
            }

            bInited = true;
            return bInited;
        }

        public bool SaveDetectRecord(SDetectRecord sdr)
        {
            if (FileIsUsing())
            {
                return false;
            }

            string strWrite = "";
            string strTmp = sdr._createTime;
            string strEnc = crypt.Encode(strTmp);
            strWrite += strEnc;
            strWrite += ",";

            strTmp = sdr._cardNum;
            strEnc = crypt.Encode(strTmp);
            strWrite += strEnc;
            strWrite += ",";

            strTmp = sdr._detectScore;
            strEnc = crypt.Encode(strTmp);
            strWrite += strEnc;
            strWrite += ",";

            strTmp = sdr._detectResult.ToString();
            strEnc = crypt.Encode(strTmp);
            strWrite += strEnc;
            strWrite += ",";

            strTmp = sdr._siteImage;
            strEnc = crypt.Encode(strTmp);
            strWrite += strEnc;
            strWrite += ",";

            strTmp = sdr._updateState.ToString();
            strEnc = crypt.Encode(strTmp);
            strWrite += strEnc;
            strWrite += "\r\n";

            if (!File.Exists(strRecordFile))
            {
                File.Create(strRecordFile);
            }
            using (FileStream fs = File.Open(strRecordFile, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    byte[] data = System.Text.Encoding.Default.GetBytes(strWrite);
                    fs.Write(data, 0, data.Length);
                }
            }

            return true;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        public readonly IntPtr HFILE_ERROR = new IntPtr(-1);

        /// <summary>
        /// 检测文件是否被占用
        /// </summary>
        /// <returns></returns>
        private bool FileIsUsing()
        {
            IntPtr vHandle = _lopen(strRecordFile, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (vHandle == HFILE_ERROR)
            {
                return true;
            }
            CloseHandle(vHandle);
            return false;
        }

        /// <summary>
        /// 获取记录表第一行
        /// </summary>
        public string ReadDetectRecord()
        {
            if (!File.Exists(strRecordFile) || FileIsUsing())
            {
                return "";
            }
            string strRet = "";
            using (FileStream fs = File.Open(strRecordFile, FileMode.Open))
            {
                StreamReader sr = new StreamReader(fs);
                string line = null;
                if ((line = sr.ReadLine()) == null)
                    return strRet;

                string[] ss = line.Split(',');
                
                string strTmp = "";
                for (int i = 0; i < ss.Length; ++i)
                {
                    string str = crypt.Decode(ss[i]);
                    if (i == (ss.Length - 1))
                    {
                        strTmp += str;
                    }
                    else
                    {
                        strTmp += str + ",";
                    }
                }
                strRet = strTmp;
            }
            return strRet;
        }

        public bool RemoveRecordFirst()
        {
            if (FileIsUsing())
            {
                return false;
            }
            List<string> lines = new List<string>(File.ReadAllLines(strRecordFile));
            lines.RemoveAt(0);//删除第1行
            File.WriteAllLines(strRecordFile, lines.ToArray());
            return false;
        }

        public bool SaveCardInfo(object obj,string sType)
        {
            if (sType == "0")//身份证读卡器设备
            {
                SCardInfo info = (SCardInfo)obj;
                string strWrite = "";
                string strTmp = info._idNum;
                string strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info._name;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info._sex;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info._nation;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info._birthday;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info._address;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info._issure;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info._dateStart;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info._dateEnd;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info._photo;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += "\r\n";

                using (FileStream fs = File.Open(strCardInfoFile, FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        byte[] data = System.Text.Encoding.Default.GetBytes(strWrite);
                        fs.Write(data, 0, data.Length);
                    }
                }
            }

            if (sType == "2")//快证通设备
            {
                PassportInfo info = (PassportInfo)obj;
                string strWrite = "";
                string strTmp = info.PassportNo.Length == 0?info.MRZ:info.PassportNo;
                string strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info.Name;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info.Sex;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info.Nation;//民族
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info.DateOfBirth;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info.PlaceOfBirth;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info.AuthorityOCR;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info.DateOfIssue;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info.DateOfExpiry;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += ",";

                strTmp = info.PhotoHead;
                strEnc = crypt.Encode(strTmp);
                strWrite += strEnc;
                strWrite += "\r\n";

                using (FileStream fs = File.Open(strPassportInfoFile, FileMode.Append))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        byte[] data = System.Text.Encoding.Default.GetBytes(strWrite);
                        fs.Write(data, 0, data.Length);
                    }
                }
            }

            return true;
        }



        /// <summary>
        /// 获取身份证号码列表
        /// </summary>
        /// <param name="cardNumLists"></param>
        public void ReadCardInfo(out List<string> cardNumLists)
        {
            List<string> cNumLists = new List<string>();
            if (!File.Exists(strCardInfoFile))
            {
                File.Create(strCardInfoFile);
                cardNumLists = cNumLists;
                return;
            }

            using (FileStream fs = File.Open(strCardInfoFile, FileMode.Open))
            {
                StreamReader sr = new StreamReader(fs);
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] ss = line.Split(',');
                    if (ss.Length != 10)
                    {
                        continue;
                    }
                    string strTmp = "";
                    for (int i = 0; i < ss.Length; ++i)
                    {
                        string str = crypt.Decode(ss[i]);
                        if (i == (ss.Length - 1))
                        {
                            strTmp += str;
                        }
                        else
                        {
                            strTmp += str + ",";
                        }
                    }
                    cNumLists.Add(strTmp);
                }
            }
            cardNumLists = cNumLists;
            return;
        }

        /// <summary>
        /// 获取护照号码列表
        /// </summary>
        /// <param name="cardNumLists"></param>
        public void ReadPassportInfo(out List<string> cardNumLists)
        {
            List<string> cNumLists = new List<string>();
            if (!File.Exists(strPassportInfoFile))
            {
                File.Create(strPassportInfoFile);
                cardNumLists = cNumLists;
                return;
            }

            using (FileStream fs = File.Open(strPassportInfoFile, FileMode.Open))
            {
                StreamReader sr = new StreamReader(fs);
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] ss = line.Split(',');
                    if (ss.Length != 10)
                    {
                        continue;
                    }
                    string strTmp = "";
                    for (int i = 0; i < ss.Length; ++i)
                    {
                        string str = crypt.Decode(ss[i]);
                        if (i == (ss.Length - 1))
                        {
                            strTmp += str;
                        }
                        else
                        {
                            strTmp += str + ",";
                        }
                    }
                    cNumLists.Add(strTmp);
                }
            }
            cardNumLists = cNumLists;
            return;
        }
    }
}
