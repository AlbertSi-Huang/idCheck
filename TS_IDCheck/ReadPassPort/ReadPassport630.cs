using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TS_IDCheck
{
    class ReadPassport630 : IReadCard
    {
        public bool IsNeedCapture
        {
            get;

            set;
        }

        public bool IsOpened
        {
            get;

            set;
        }

        public string Name
        {
            get;

            set;
        }

        private static ReadCardDelegate readCardComplete;
        public event ReadCardDelegate OnReadCardEvent
        {
            add { readCardComplete += new ReadCardDelegate(value); }
            remove { readCardComplete -= new ReadCardDelegate(value); }
        }

        public void Close()
        {
            return;
        }

        public void StartExeFile(string myPathName, string myArgu, int myOpenType)
        {
            if (!File.Exists(myPathName))
            {
                Trace.WriteLine("文件不存在");
                return;
            }
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = myPathName;
            info.Arguments = myArgu;
            //info.UseShellExecute = false;
            if (myOpenType > 3)
            {
                return;
            }
            switch (myOpenType)
            {
                case 0:
                    info.WindowStyle = ProcessWindowStyle.Normal;
                    break;
                case 1:
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    break;
                case 2:
                    info.WindowStyle = ProcessWindowStyle.Minimized;
                    break;
                case 3:
                    info.WindowStyle = ProcessWindowStyle.Maximized;
                    break;
                default:
                    info.WindowStyle = ProcessWindowStyle.Maximized;
                    break;
            }

            Process pro = Process.Start(info);
        }

        bool CheckRun(string strName)
        {
            Process[] procs = Process.GetProcesses();
            bool bHave = false;
            for (int i = 0; i < procs.Length; ++i)
            {
                if (procs[i].ProcessName.ToLower().CompareTo(strName) == 0)
                {
                    bHave = true;
                    break;
                }
            }
            return bHave;
        }

        public bool Init()
        {
            bool bRet = false;

            string strPath = Directory.GetCurrentDirectory();
            strPath += "\\device\\passport_630\\exereadpassport630.exe";
            StartExeFile(strPath, "", 1);

            Thread.Sleep(1000);
            bRet = CheckRun("exereadpassport630");

            return bRet;

        }

        Thread _threadRead = null;

        public void ReadCard()
        {
            _threadRead = new Thread(OnRun);
            _threadRead.IsBackground = true;
            _threadRead.Start();
        }

        private FileOperator fileOper = new FileOperator();

        public bool InitFileData()
        {
            try
            {
                Trace.WriteLine("文件数据初始化...");
                bool bRet = false;
                if (!fileOper.BInited)
                {
                    bRet = fileOper.InitFileOper();
                    if (!bRet)
                        return bRet;
                }
                //GetPassportLists();
                Trace.WriteLine("文件数据初始化完成");
                return bRet;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("文件数据初始化异常:" + ex.Message);
                return false;
            }
        }

        void OnRun()
        {
            Thread.Sleep(3000);
            string strFilePath = Directory.GetCurrentDirectory();
            strFilePath += "\\device\\passport_630";
            string strIDCardInfoFile = strFilePath + "\\readcard.txt";
            string strPassportInfoFile = strFilePath + "\\readpassport.txt";
            DateTime dtLastTick = DateTime.Now;
            SCardInfo cardInfo = new SCardInfo();
            PassportInfo passportInfo = new PassportInfo();
            while (_threadRead.IsAlive)
            {
                TimeSpan ts = DateTime.Now.Subtract(dtLastTick);
                if(ts.TotalMilliseconds < 300)
                {
                    Thread.Sleep(50);
                }

                if (!IsNeedCapture)
                {
                    Thread.Sleep(100);
                }

                bool bReadIDCard = false;
                try
                {
                    #region 读身份证信息文件

                    cardInfo._idNum = "";
                    //
                    if (File.Exists(strIDCardInfoFile))
                    {
                        FileInfo fi = new FileInfo(strIDCardInfoFile);
                        DateTime dtLastWriteTime = fi.LastWriteTime;
                        TimeSpan ts1 = DateTime.Now.Subtract(dtLastWriteTime);
                        if (ts1.TotalMilliseconds < 3000)
                        {
                            //有效信息
                            string strContext = File.ReadAllText(strIDCardInfoFile);
                            Trace.WriteLine("630护照读到身份证数据:" + strContext);
                            string[] ssContext = strContext.Split(',');
                            if (ssContext.Length == 10)
                            {
                                int nIndex = 0;
                                cardInfo._name = ssContext[nIndex++];
                                cardInfo._sex = ssContext[nIndex++];
                                cardInfo._birthday = ssContext[nIndex++];
                                cardInfo._nation = ssContext[nIndex++];
                                cardInfo._address = ssContext[nIndex++];
                                cardInfo._idNum = ssContext[nIndex++];
                                cardInfo._issure = ssContext[nIndex++];
                                cardInfo._dateStart = ssContext[nIndex++];
                                cardInfo._dateEnd = ssContext[nIndex++];
                                cardInfo._photo = ssContext[nIndex++];

                                if (cardInfo._idNum.Length == 18)
                                {
                                    bReadIDCard = true;

                                    passportInfo.CardType = "0";
                                    passportInfo.Name = cardInfo._name;
                                    passportInfo.Sex = cardInfo._sex;
                                    passportInfo.Nation = cardInfo._nation;
                                    passportInfo.DateOfBirth = cardInfo._birthday;
                                    passportInfo.PlaceOfBirth = cardInfo._address;
                                    passportInfo.PersonalIdNo = cardInfo._idNum;
                                    passportInfo.AuthorityOCR = cardInfo._issure;
                                    passportInfo.DateOfIssue = cardInfo._dateStart;
                                    passportInfo.DateOfExpiry = cardInfo._dateEnd;
                                    fileOper.SaveCardInfo(passportInfo, "2");

                                    readCardComplete(cardInfo, 0);
                                }
                            }
                        }

                        File.Delete(strIDCardInfoFile);
                    }
                    #endregion
                    //passportInfo.PassportNo = "";
                    #region 读护照信息文件
                    if (File.Exists(strPassportInfoFile))
                    {
                        FileInfo fi = new FileInfo(strPassportInfoFile);
                        DateTime dtLastWriteTime = fi.LastWriteTime;
                        TimeSpan ts1 = DateTime.Now.Subtract(dtLastWriteTime);
                        if (ts1.TotalMilliseconds < 3000)
                        {
                            string strContext = File.ReadAllText(strPassportInfoFile);
                            Trace.WriteLine("630护照读到护照数据:" + strContext);
                            string[] ssContext = strContext.Split('~');
                            if (ssContext[0].Length > 0)
                            {
                                int nIndex = 0;
                                passportInfo.CardType = "2";
                                passportInfo.PassportNo = ssContext[nIndex++];
                                passportInfo.Nation = ssContext[nIndex++];
                                passportInfo.Sex = ssContext[nIndex++];
                                passportInfo.MRZ = ssContext[nIndex++];
                                passportInfo.Name = ssContext[nIndex++];
                                passportInfo.DateOfBirth = ssContext[nIndex++];
                                passportInfo.EnglishName = ssContext[nIndex++];
                                passportInfo.EnglishFamilyName = ssContext[nIndex++];
                                passportInfo.EnglishGivienName = ssContext[nIndex++];
                                passportInfo.MRZ1 = ssContext[nIndex++];
                                passportInfo.MRZ2 = ssContext[nIndex++];
                                passportInfo.Nationality = ssContext[nIndex++];
                                passportInfo.PlaceOfBirth = ssContext[nIndex++];
                                passportInfo.PlaceOfIssue = ssContext[nIndex++];
                                passportInfo.DateOfIssue = ssContext[nIndex++];
                                passportInfo.RFIDMRZ = ssContext[nIndex++];
                                passportInfo.OCRMRZ = ssContext[nIndex++];
                                passportInfo.PlaceOfBirthPinyin = ssContext[nIndex++];
                                passportInfo.PlaceOfIssuePinyin = ssContext[nIndex++];
                                passportInfo.PersonalIdNo = ssContext[nIndex++];
                                passportInfo.NamePinyinOCR = ssContext[nIndex++];
                                passportInfo.SexOCR = ssContext[nIndex++];
                                passportInfo.NationalityOCR = ssContext[nIndex++];
                                passportInfo.PersonalIdNoOCR = ssContext[nIndex++];
                                passportInfo.PlaceOfBirthOCR = ssContext[nIndex++];
                                passportInfo.DateOfExpiryOCR = ssContext[nIndex++];
                                passportInfo.AuthorityOCR = ssContext[nIndex++];
                                passportInfo.FamilyName = ssContext[nIndex++];
                                passportInfo.GivienName = ssContext[nIndex++];
                                passportInfo.Photo = ssContext[nIndex++];
                                passportInfo.PhotoHead = ssContext[nIndex++];

                                readCardComplete(passportInfo, 2);
                                fileOper.SaveCardInfo(passportInfo, "2");
                                bReadIDCard = true;
                            }
                        }
                        //Trace.WriteLine("准备删除文件" + strPassportInfoFile);
                        File.Delete(strPassportInfoFile);
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("passport_630 error: " + ex.Message);
                }
                
                if (bReadIDCard)
                {

                }

                Thread.Sleep(50);
            }
        }
    }
}
