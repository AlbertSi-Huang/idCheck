using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TS_IDCheck
{
    public class SaveRecordFile
    {
        private string _strScenePath,_strCardPath,_strSitePath,_strRecordPath, _strQuerryPath;
        #region  定义通知副屏软件 事件
        //定义事件
        //public delegate void SendMagToScreenHandler(string mystr);
        //public event SendMagToScreenHandler SendMsgToScreen;
        #endregion
        public SaveRecordFile()
        {
            string savePath = ConfigOperator.Single().StrSavePath;

            _strCardPath = savePath + @"\cardImgd\";
            _strSitePath = savePath + @"\siteImgd\";
            _strScenePath = savePath + @"\sceneImgd\";
            _strRecordPath = savePath + @"\jsonRecord\";
            _strQuerryPath = savePath + @"\Querrypic\";

        }

        public static void DelectDir(string srcPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void CreateDirAndFile()
        {
            //fly20171103  ++
            fileUploadCnt = ConfigOperator.Single().StrfileUploadCnt;
            if(fileUploadCnt==null)
            {
                fileUploadCnt = "0000000000";
            }
              try
              {
                string savePath = ConfigOperator.Single().StrSavePath;
                if (Directory.Exists(savePath))
                  {
                      DelectDir(savePath);
                  }
              }
              catch (Exception ex)
              {
                Console.WriteLine("CreateDirAndFile" + ex.Message);
                  //i++;
              }

            if (!Directory.Exists(_strCardPath))
            {
                Directory.CreateDirectory(_strCardPath);
            }

            if (!Directory.Exists(_strSitePath))
            {
                Directory.CreateDirectory(_strSitePath);
            }

            if (!Directory.Exists(_strScenePath))
            {
                Directory.CreateDirectory(_strScenePath);
            }

            if (!Directory.Exists(_strRecordPath))
            {
                Directory.CreateDirectory(_strRecordPath);
            }
            if (!Directory.Exists(_strQuerryPath))
            {
                Directory.CreateDirectory(_strQuerryPath);
            }
            
        }

        private int GetFileNum(string dirPath)
        {
            DirectoryInfo theFolder = new DirectoryInfo(dirPath);
            return theFolder.GetFiles().Length;  
        }

        private int _nCurrentChangeNum = 0;
        private const int _SAVENUM = 10;
      

        public bool SaveQuerryImg(Bitmap myMap)
        {
            bool bRet = false;
            string fileName = _strQuerryPath + "tmp.jpg";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            myMap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            bRet = true;
            return bRet;
        }

        string  fileUploadCnt = "0";
        string  fileUploadSubCnt;
        public string GetCurScenePath()
        {         
            return _strScenePath + fileUploadSubCnt + ".jpg";
        }
        public string GetCurSitePath()
        {
            
            return _strSitePath + fileUploadSubCnt + ".jpg";
        }
        public string GetCurCardPath()
        {
            return _strCardPath + fileUploadSubCnt + ".jpg";
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="bmScene"></param>
        /// <param name="bmCut"></param>
        /// <param name="bmCard"></param>
        /// <param name="detectRecord"></param>
        /// <returns></returns>
        public bool SaveFile(Bitmap bmScene,Bitmap bmCut,Bitmap bmCard, SDetectRecord detectRecord,int nManType)
        {
            bool bRet = false;
            int nImgNum =  GetFileNum(_strScenePath);
            
            if (_nCurrentChangeNum >= _SAVENUM)
            {
                _nCurrentChangeNum = 0;
            }
            _nCurrentChangeNum++;
            
            fileUploadSubCnt = _nCurrentChangeNum.ToString();
            string fileName = (_nCurrentChangeNum).ToString() + ".jpg";
            
            
            if(File.Exists(_strScenePath + fileName))
            {
                File.Delete(_strScenePath + fileName);
            }
            bmScene.Save(_strScenePath + fileName,ImageFormat.Jpeg);
            
            if (File.Exists(_strSitePath + fileName))
            {
                File.Delete(_strSitePath + fileName);
            }
            bmCut.Save(_strSitePath + fileName, ImageFormat.Jpeg);

            Trace.WriteLine("写入的文件名：" + _strCardPath + fileName);
            if (File.Exists(_strCardPath + fileName))
            {
                File.Delete(_strCardPath + fileName);
            }
            bmCard.Save(_strCardPath + fileName, ImageFormat.Jpeg);
            bmCard.Dispose();
            Trace.WriteLine("写入完成：" + _strCardPath + fileName);
            DataContractJsonSerializer deseralizer = null;

            string txtFileName = _strRecordPath + (_nCurrentChangeNum).ToString() + ".txt";
            if (File.Exists(txtFileName)){
                File.Delete(txtFileName);
            }
            //wfc 此处用FileMode.OpenOrCreate方式打开时 必须先删除原文件 否则可能出现json错误
            using (FileStream fs = new FileStream(txtFileName, FileMode.OpenOrCreate))
            {
                deseralizer = new DataContractJsonSerializer(typeof(SDetectRecord));
                MemoryStream msObj = new MemoryStream();
                //将序列化之后的Json格式数据写入流中
                deseralizer.WriteObject(msObj, detectRecord);
                msObj.Position = 0;
                StreamReader sr = new StreamReader(msObj);
                string json = sr.ReadToEnd();
                sr.Close();
                msObj.Close();
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(json);
                fs.Write(byteArray, 0, byteArray.Length);
            }
            #region  //向副屏发送消息  
            if(ConfigOperator.Single().nIsDoubleScreen == 1)
            {
                try
                {

                    if (nManType == 0)
                    {
                        string mystr = ((int)dbscclient.SendToScreenType.SendGetCopareResult).ToString();
                        dbscclient.Single().ClientSendMsg(mystr + "," + _nCurrentChangeNum.ToString());
                    }
                    else if (nManType == 1)
                    {
                        Trace.WriteLine("发送黑名单消息");
                        string mystr = ((int)(dbscclient.SendToScreenType.SendBlackList)).ToString();
                        dbscclient.Single().ClientSendMsg(mystr + "," + _nCurrentChangeNum.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }    
            
            #endregion
            return bRet;
        }

    }
}
