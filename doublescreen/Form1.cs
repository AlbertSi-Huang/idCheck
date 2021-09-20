using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Diagnostics;

namespace doublescreen
{
    public delegate void deleNoParamFunc();

    public partial class fmainwindow : Form
    {
        public delegate void SubFormExitHandle(string mystr);
        const uint sMaxPiccnt = 10;//10001;  //debug20
        const uint sPicPerpage = 8;
        const uint sDefatPicWidth = 110;
        const uint sDefatPicHeight = 136;
        const uint sResizeInc = 30;
        public static Int32 sPerLocattionX = 0;
        public static Int32 sPerLocattionY = 0;
       
        private  PictureBox[] pics = new PictureBox[sPicPerpage];
        fileLastName myFileLastName = new fileLastName();
     
        private static bool sDoorAlwaysOpen = false;
        private static Int32 slastHour = 0;
        public static bool IsExitFromOtherForm = false;
        public static fmainwindow ref_win = null;
        SimpleLog _simLog = null;
        public fmainwindow()
        {
            InitializeComponent();

            //日志
            _simLog = new SimpleLog("doublescreen");
            _simLog.LogWriteBegin();
            Trace.Listeners.Add(_simLog);

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            SocketService.Single().ServiceStart();
            SocketService.Single().GetNoNetEvent += OnGetNoNetEvent;
            ref_win = this;
            {
                pic0.Width = 120; pic0.Height = 160; Point p0 = pic0.Location; pic0.Location = new Point(7, p0.Y - 25); pic0.SizeMode = PictureBoxSizeMode.StretchImage;
                pic1.Width = 120; pic1.Height = 160; Point p1 = pic1.Location; pic1.Location = new Point(127 * 1 + 7, p1.Y - 25); pic1.SizeMode = PictureBoxSizeMode.StretchImage;
                pic2.Width = 120; pic2.Height = 160; Point p2 = pic2.Location; pic2.Location = new Point(127 * 2 + 7, p2.Y - 25); pic2.SizeMode = PictureBoxSizeMode.StretchImage;
                pic3.Width = 120; pic3.Height = 160; Point p3 = pic3.Location; pic3.Location = new Point(127 * 3 + 7, p3.Y - 25); pic3.SizeMode = PictureBoxSizeMode.StretchImage;
                pic4.Width = 120; pic4.Height = 160; Point p4 = pic4.Location; pic4.Location = new Point(127 * 4 + 7, p4.Y - 25); pic4.SizeMode = PictureBoxSizeMode.StretchImage;
                pic5.Width = 120; pic5.Height = 160; Point p5 = pic5.Location; pic5.Location = new Point(127 * 5 + 7, p5.Y - 25); pic5.SizeMode = PictureBoxSizeMode.StretchImage;
                pic6.Width = 120; pic6.Height = 160; Point p6 = pic6.Location; pic6.Location = new Point(127 * 6 + 7, p6.Y - 25); pic6.SizeMode = PictureBoxSizeMode.StretchImage;
                pic7.Width = 120; pic7.Height = 160; Point p7 = pic7.Location; pic7.Location = new Point(127 * 7 + 7, p7.Y - 25); pic7.SizeMode = PictureBoxSizeMode.StretchImage;
            }

            pics[0] = pic0;
            pics[1] = pic1;
            pics[2] = pic2;
            pics[3] = pic3;
            pics[4] = pic4;
            pics[5] = pic5;
            pics[6] = pic6;
            pics[7] = pic7;
            
            for (Int32 i=0;i< sPicPerpage;i++)
            {
                pics[i].MouseClick += new System.Windows.Forms.MouseEventHandler(this.PicClick);
            }
            Trace.WriteLine("加载配置");
            ConfigOperator.Single().InitConfig();
            string strSavePath = ConfigOperator.Single().StrSavePath;
            SetBtnStyle(btnQuery);
            SetBtnStyle(BtnOpenMode);
            SetBtnStyle(btnReset);
            picShow.Single().mypicShow(pics, picofID, picofSite, strSavePath + @"\");
            MTWFile.Single().MTWFileSetFile(@".\log.txt");
            sys1sticket.Enabled = true;
            sys1sticket.Start();
            BtnOpenMode.BackgroundImage = Image.FromFile(@".\dbpicture\开关开.png");
            this.BackgroundImage = new Bitmap(@".\dbpicture\首页_无信息.png");
            this.btnClose.Visible = false;
            this.toolTip1.SetToolTip(this.btnReset, "关闭电脑");
            Trace.WriteLine("加载init");
        }

        void OnGetNoNet()
        {
            MTWFile.Single().WriteLine("副屏提示网络异常");
            this.labConn.ForeColor = Color.Red;
            labConn.Text = "网络异常";
            iPingLoop = 0;
        }

        private void OnGetNoNetEvent()
        {
            this.labConn.Invoke(new GetNoNetDelegate(OnGetNoNet));
        }

        private void SetBtnStyle(Button btn)  //在Form1_Load时候调用
        {
            btn.FlatStyle = FlatStyle.Flat;//样式
            btn.ForeColor = Color.Transparent;//前景
            btn.BackColor = Color.Transparent;//去背景
            btn.FlatAppearance.BorderSize = 0;//去边线
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;//鼠标经过
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;//鼠标按下
        }

        /// <summary>
        /// 图片单击显示按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PicClick(object sender, System.EventArgs e)
        {
            string ss;
            ss = ((PictureBox)sender).Name;
            ss = ss.Substring(ss.Length - 1, 1);
            picShow.Single().picShowBottom(pics);
            picShow.Single().picShowBottomHigtLine(pics, Convert.ToUInt32(ss));
            picShow.Single().ShowHitInfo(Convert.ToUInt32(ss), this.picofID, this.picofSite);
            string mystr = picShow.Single().GetClickRecordInfo((int)(Convert.ToUInt32(ss)));
            if (mystr != null)
                ShowDetectInfo(mystr, string.Empty, 1, false);
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

        private bool _isPC = false;
        
        System.Timers.Timer _timePing = null;
        ThreadCheckMain _tCheck = new ThreadCheckMain();
        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fmainwindow_Load(object sender, EventArgs e)
        {
            Trace.WriteLine("fmainwindow_Load");
            btnOpenDoor.Enabled = true;
            SocketService.Single().SocketServiceRecv += new SocketService.SocketServiceRecvHandler(SocketServiceRecUpdateForm1);
            setTextValue("");

            System.Windows.Forms.Screen[] s1 = System.Windows.Forms.Screen.AllScreens;
            System.Windows.Forms.Screen s0 = System.Windows.Forms.Screen.PrimaryScreen;
            int nPrimaryWidth = 1024;
            if (s1.Length == 1)
            {
                Trace.WriteLine("获取到一块屏幕");
                nPrimaryWidth = 0;
                this.Location = new Point(0, 0);
            }
            else if (s1.Length == 2)
            {
                for(int i = 0; i < 2; ++i)
                {
                    if (s0.WorkingArea.Width != s1[i].WorkingArea.Width || s0.WorkingArea.Height != s1[i].WorkingArea.Height)
                    {
                        System.Drawing.Rectangle r1 = s1[i].WorkingArea;
                        this.Width = r1.Width;
                        this.Height = r1.Height;
                    }
                    else
                    {
                        nPrimaryWidth = s1[i].WorkingArea.Width;
                    }
                }
            }
            //if (s0.WorkingArea.Width > 1024)
            //{
            //    _isPC = true;

            //    this.Width = s0.WorkingArea.Width;
            //    this.Height = s0.WorkingArea.Height;
            //}

            this.Location = new Point(nPrimaryWidth, 0);
            _timePing = new System.Timers.Timer(2000);
            _timePing.Elapsed += TimePingElapsed;
            _timePing.AutoReset = true;
            _timePing.Enabled = true;

            
#if !DEBUG
            _tCheck.Start();
#endif
            Trace.WriteLine("fmainwindow_Load end");
        }

        private string GetIpFromUrl(string strUrl)
        {
            string strIp = "";

            strIp = strUrl.Substring(7);
            int nPos = strIp.IndexOf(':');
            strIp = strIp.Substring(0, nPos);

            return strIp;
        }

        int iPingLoop = 0;
        private void TimePingElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int nPlateChoose = ConfigOperator.Single().PlateChoose;
            string strIp = "";
            if(nPlateChoose == 1 || nPlateChoose == 4)
            {
                strIp = GetIpFromUrl(ConfigOperator.Single().QueryBlackUrlPolice);//"10.20.50.110";
            }
            else if(nPlateChoose == 2)
            {
                strIp = GetIpFromUrl(ConfigOperator.Single().QueryBlackUrlVedio);
            }
            else if(nPlateChoose == 3)
            {
                strIp = GetIpFromUrl(ConfigOperator.Single().QueryBlackUrlNet);
            }
            bool bPing = Ping(strIp);
            if (bPing)
            {
                if(nPlateChoose == 4)
                {
                    strIp = GetIpFromUrl(ConfigOperator.Single().QueryBlackUrlArmy);//"10.84.2.174";
                    bPing = Ping(strIp);
                }
            }
            if (!bPing)
            {
                new Thread(() => { ShowNetErr("网络异常"); }).Start();
                iPingLoop = 0;
            }else
            {
                if(iPingLoop > 1)
                    this.labConn.Text = "";
                iPingLoop++;
            }
        }

        private void ShowNetErr(string msg)
        {
            Action act = delegate ()
            {
                this.labConn.ForeColor = Color.Red;
                this.labConn.Text = msg;
            };
            this.labConn.Invoke(act);
        }

        private delegate void InvokeCallback(string msg);

        /// <summary>
        /// 接收到Socket发送的消息
        /// </summary>
        /// <param name="smystr"></param>
        public void SocketServiceRecUpdateForm1(string smystr)
        {
            uint pointToSubWin;
            // string ss;
            MTWFile.Single().WriteLine("Recv message is; ");
            MTWFile.Single().WriteLine(smystr);
            if (this.InvokeRequired)//当前线程不是创建线程 
            {
                InvokeCallback mi = new InvokeCallback(SocketServiceRecUpdateForm1);
                //ss = smystr;
                this.Invoke(mi, new object[] { smystr });//回调 else//当前线程是创建线程（界面线程）        
            }
            else
            {

                #region  收到比对结果

                pointToSubWin = myFileLastName.GetCurPoint() + 1;
                picShow.Single().picNew(myFileLastName.GetCurPoint());
                NewQuest();
                lbPeopleCnt.Text = myFileLastName.GetmyCnt().ToString();

                string mystr = picShow.Single().IdInfo((int)myFileLastName.GetCurPoint());
                if (mystr != null)
                    ShowDetectInfo(mystr, smystr, pointToSubWin, true);
                #endregion
            }
        }
        private void ShowCardId(string strJson)
        {
            using(MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strJson)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SDetectRecord));
                SDetectRecord _mycord = (SDetectRecord)serializer.ReadObject(ms);
                lbName.Text = _mycord._card._name;
                lbSex.Text = _mycord._card._sex;
                lbNational.Text = _mycord._card._nation;
                lbBirthday.Text = _mycord._card._birthday;
                lbID.Text = _mycord._card._idNum;
                if (_mycord._card._address != null)
                {
                    if (_mycord._card._address.Length > 11)
                    {
                        string[] data = new string[2];
                        data[0] = _mycord._card._address.Substring(0, 11);
                        data[1] = _mycord._card._address.Substring(11);
                        lbAddr.Text = data[0];
                        lbAddr_2.Text = data[1];
                    }
                    else
                    {
                        lbAddr.Text = _mycord._card._address;
                        lbAddr_2.Text = "";
                    }
                }
                else
                {
                    lbAddr.Text = "";
                    lbAddr_2.Text = "";
                }


                lbSignAddr.Text = _mycord._card._issure;

                lbScore.Text = _mycord._detectScore;
                if (_mycord._detectResult == 1)
                {
                    // lbPass.Text = "比对失败";
                    picPass.Image = Image.FromFile(@".\dbpicture\核验成功.png");

                }
                else
                {
                    picPass.Image = Image.FromFile(@".\dbpicture\核验失败.png");
                    //lbPass.Text = "比对成功";
                }
                MTWFile.Single().WriteLine("加载图片成功！");
                lbCheckTime.Text = _mycord._createTime;
            }
        }

        private void ShowDetectInfo(string strJson,string smystr,uint pointToSubWin,bool bCurrent)
        {
            if (strJson != null)
            {
                Trace.WriteLine("收到比对结果" + strJson);
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(strJson)))
                {
                    if (ConfigOperator.Single().CardReaderBrand == "0")//身份证读卡器读取身份证信息
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SDetectRecord));
                        SDetectRecord _mycord = (SDetectRecord)serializer.ReadObject(ms);
                        lbName.Text = _mycord._card._name;
                        lbSex.Text = _mycord._card._sex;
                        lbNational.Text = _mycord._card._nation;
                        lbBirthday.Text = _mycord._card._birthday;
                        lbID.Text = _mycord._card._idNum;
                        if (_mycord._card._address != null)//
                        {
                            if (_mycord._card._address.Length > 11)
                            {
                                string[] data = new string[2];
                                data[0] = _mycord._card._address.Substring(0, 11);
                                data[1] = _mycord._card._address.Substring(11);
                                lbAddr.Text = data[0];
                                lbAddr_2.Text = data[1];
                            }
                            else
                            {
                                lbAddr.Text = _mycord._card._address;
                                lbAddr_2.Text = "";
                            }
                        }
                        else
                        {
                            lbAddr.Text = "";
                            lbAddr_2.Text = "";
                        }


                        lbSignAddr.Text = _mycord._card._issure;

                        lbScore.Text = _mycord._detectScore;
                        if (_mycord._detectResult == 1)
                        {
                            // lbPass.Text = "比对失败";
                            picPass.Image = Image.FromFile(@".\dbpicture\核验成功.png");

                        }else if(_mycord._detectResult == 6)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\车票过期.png");
                        }
                        else if (_mycord._detectResult == 7)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\车票查询失败.png");
                        }
                        else if (_mycord._detectResult == 8)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\已发班.png");
                        }else if(_mycord._detectResult == 9)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\证件过期.png");
                        }
                        else if (_mycord._detectResult == 10)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\已检票.png");
                        }
                        else /*if(_mycord._detectResult)*/
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\核验失败.png");
                            //lbPass.Text = "比对成功";
                        }
                        MTWFile.Single().WriteLine("加载图片成功！");
                        lbCheckTime.Text = _mycord._createTime;
                    }
                    
                    if (ConfigOperator.Single().CardReaderBrand == "2" || 
                        ConfigOperator.Single().CardReaderBrand == "3")//快证通设备读取护照、身份证信息，刘飞翔，20180504
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SDetectPassportRecord));
                        SDetectPassportRecord _mycord = (SDetectPassportRecord)serializer.ReadObject(ms);
                        lbName.Text = _mycord._passportCard.Name;
                        lbSex.Text = _mycord._passportCard.Sex;
                        lbNational.Text = _mycord._passportCard.Nation;
                        lbBirthday.Text = _mycord._passportCard.DateOfBirth;
                        lbID.Text = (_mycord._passportCard.PassportNo == null || _mycord._passportCard.PassportNo.Length == 0)?_mycord._passportCard.MRZ: _mycord._passportCard.PassportNo;

                        if (_mycord._passportCard.CardType == "2")//读取到护照信息
                        {
                            if (_mycord._passportCard.PlaceOfBirth != null)//出生地信息不为空
                            {
                                lbAddr.Text = _mycord._passportCard.PlaceOfBirth;
                                lbAddr_2.Text = "";
                            }
                            else
                            {
                                lbAddr.Text = "";
                                lbAddr_2.Text = "";
                                    
                            }
                        }
                        if (_mycord._passportCard.CardType == "0")//读取到身份证信息
                        {
                            Trace.WriteLine("准备显示身份证信息");
                            if (_mycord._passportCard.PlaceOfBirth != null)//出生地信息不为空
                            {
                                //出生地信息分行显示
                                if (_mycord._passportCard.PlaceOfBirth.Length > 11)
                                {
                                    string[] data = new string[2];
                                    data[0] = _mycord._passportCard.PlaceOfBirth.Substring(0, 11);
                                    data[1] = _mycord._passportCard.PlaceOfBirth.Substring(11);
                                    lbAddr.Text = data[0];
                                    lbAddr_2.Text = data[1];
                                }
                                else
                                {
                                    lbAddr.Text = _mycord._passportCard.PlaceOfBirth;
                                    lbAddr_2.Text = "";
                                }
                            }
                            else
                            {
                                lbAddr.Text = "";
                                lbAddr_2.Text = "";
                            }
                            
                        }


                        lbSignAddr.Text = _mycord._passportCard.AuthorityOCR;
                        lbScore.Text = _mycord._detectScore;
                        if (_mycord._detectResult == 1)
                        {
                            // lbPass.Text = "比对失败";
                            picPass.Image = Image.FromFile(@".\dbpicture\核验成功.png");

                        }
                        else if (_mycord._detectResult == 6)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\车票过期.png");
                        }
                        else if (_mycord._detectResult == 7)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\车票查询失败.png");
                        }
                        else if (_mycord._detectResult == 8)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\已发班.png");
                        }
                        else if (_mycord._detectResult == 9)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\证件过期.png");
                        }
                        else if (_mycord._detectResult == 10)
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\已检票.png");
                        }
                        else /*if(_mycord._detectResult)*/
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\核验失败.png");
                            //lbPass.Text = "比对成功";
                        }
                        MTWFile.Single().WriteLine("加载图片成功！");
                        lbCheckTime.Text = _mycord._createTime;
                    }
                    
                    
                    if (bCurrent)
                    {
                        string[] karray = smystr.Split(',');
                        int i = Convert.ToInt32(karray[0]);
                        int j = (Int32)SocketService.SendToScreenType.SendBlackList;
                        if (i == j)
                        {
                            string log = string.Format("get black to popo form {0}", pointToSubWin);
                            MTWFile.Single().WriteLine(log);
                            popup mypopu = new popup(popup.PopupType.BlackList, pointToSubWin-1);
                            mypopu.Show();
                        }
                    }
                    
                }
            }
        }

        /// <summary>
        /// 开门按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenDoor_Click(object sender, EventArgs e)
        {
            try
            {
                SocketService.Single().ServerSendOpenDoor();
            }
            catch(Exception ex)
            {
                MessageBoxEx.Show(ex.Message);
            }
        }

        /// <summary>
        /// 闸机模式设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpenMode_Click(object sender, EventArgs e)
        {
            if(sDoorAlwaysOpen)
            {
                sDoorAlwaysOpen = false;
                BtnOpenMode.BackgroundImage = Image.FromFile(@".\dbpicture\关.png");
                btnOpenDoor.Enabled = true;
            } 
            else
            {
                sDoorAlwaysOpen = true;
                BtnOpenMode.BackgroundImage = Image.FromFile(@".\dbpicture\开关开.png");
                btnOpenDoor.Enabled = false;
            }
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);

            if (message.Msg == WM_NCHITTEST && (int)message.Result == HTCLIENT)
            {
                message.Result = (IntPtr)HTCAPTION;
            }
        }

        private void NewQuest()
        {
            UInt32 i = 0;
            UInt32 k = 0;
            myFileLastName.AddNew();
            uint nTotal = myFileLastName.GetmyCnt();
            uint nCurrentNum = myFileLastName.GetCurPoint();

            UInt32[] myArray = new UInt32[sPicPerpage];

            if (nTotal <= sPicPerpage)
            {
                for (k = 0; k < sPicPerpage; k++)
                {
                    myArray[k] = k+1;
                }
            }
            else
            {
                if (nTotal <= sMaxPiccnt)
                {
                    i = nTotal - sPicPerpage + 1;
                    for (k = 0; k < sPicPerpage; k++)
                    {
                        myArray[k] = i++;
                    }
                }
                else
                {
                    uint yushu = nTotal % sMaxPiccnt;          //nCurrentNum + sMaxPiccnt - sPicPerpage + 1;
                    //uint start = sMaxPiccnt - yushu;
                    if (yushu >= sPicPerpage) i = yushu - sPicPerpage + 1;
                    else { i = sMaxPiccnt - sPicPerpage + yushu + 1; }
                    for (k = 0; k < sPicPerpage; k++)
                    {
                        myArray[k] = i++;
                        if (i > sMaxPiccnt) i = 1;
                    }
                }
            }
            picShow.Single().picShowBottom(pics, myArray);
            i = myFileLastName.GetCurPoint();
            picShow.Single().picShowCompPic(nCurrentNum, picofID, picofSite);
            //this.btnLeft.Enabled = myFileLastName.HasLeftPic();
            //this.btnRight.Enabled = myFileLastName.HasRightPic();
        }

        public struct SCardInfo
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
        }
        public struct SDetectRecord
        {
            public SCardInfo _card { set; get; }
            public string _cardNum { set; get; }
            public string _createTime { set; get; }
            public string _detectScore { set; get; }
            public int _detectResult { set; get; }
            public string _siteImage { set; get; }
            public int _updateState { set; get; }
        }

        public struct SDetectPassportRecord
        {
            public SCardInfo _card;
            public string _cardNum;
            public string _createTime;
            public int _detectResult;
            public string _detectScore;
            public PassportInfo _passportCard;
            public string _siteImage;
            public int _updateState;
        }

        public struct PassportInfo
        {
            public string CardType;
            public string Type;
            public string MRZ;
            public string Name;
            public string EnglishName;
            public string Sex;
            public string DateOfBirth;
            public string DateOfExpiry;
            public string CountryCode;
            public string EnglishFamilyName;
            public string EnglishGivienName;
            public string MRZ1;
            public string MRZ2;
            public string Nationality;
            public string PassportNo;
            public string PlaceOfBirth;
            public string PlaceOfIssue;
            public string DateOfIssue;
            public string RFIDMRZ;
            public string OCRMRZ;
            public string PlaceOfBirthPinyin;
            public string PlaceOfIssuePinyin;
            public string PersonalIdNo;
            public string NamePinyinOCR;
            public string SexOCR;
            public string NationalityOCR;
            public string PersonalIdNoOCR;
            public string PlaceOfBirthOCR;
            public string DateOfExpiryOCR;
            public string AuthorityOCR;
            public string FamilyName;
            public string GivienName;
            public string Photo;
            public string PhotoHead;
            public string Nation;
    }

    /// <summary>
    /// 向右选择
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnLeft_Click(object sender, EventArgs e)
        {
            UInt32 i, j=0;
            UInt32[] myArray = new UInt32[sPicPerpage];
            bool x, y;
            myFileLastName.LeftKey();

            i = myFileLastName.GetShowPoint()+1;
            for (j = 0; j < sPicPerpage; j++)
            {
                myArray[j] = i++;
            }
            picShow.Single().picShowBottom(pics, myArray);
            x  = myFileLastName.HasLeftPic();
            y = myFileLastName.HasRightPic();
            //this.btnLeft.Enabled = x;
            //this.btnRight.Enabled = y;
        }

        /// <summary>
        /// 向左选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRight_Click(object sender, EventArgs e)
        {
            UInt32 i, j = 0;
            UInt32[] myArray = new UInt32[sPicPerpage];
            bool x, y;
            myFileLastName.RightKey();

            i = myFileLastName.GetShowPoint()+1;
            for (j = 0; j < sPicPerpage; j++)
            {
                myArray[j] = i++;
                if (i > sMaxPiccnt) i = 1;
                
            }
            
            picShow.Single().picShowBottom(pics, myArray);

            x = myFileLastName.HasLeftPic();
            y = myFileLastName.HasRightPic();
            //this.btnLeft.Enabled = x;
            //this.btnRight.Enabled = y;
        }

        /// <summary>
        /// 查询按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQuery_Click(object sender, EventArgs e)
        {
           IdInput myIdInput = new IdInput(SocketService.Single());
            myIdInput.Show();
        }

        /// <summary>
        /// 重启按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBoxEx.Show("确定要关机吗?", "关机", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)//如果点击“确定”按钮
            {
                System.Diagnostics.Process bootProcess = new System.Diagnostics.Process();
                bootProcess.StartInfo.FileName = "shutdown";
                bootProcess.StartInfo.Arguments = "/s";
                bootProcess.Start();
            }
            //DialogResult dr = MessageBoxEx.Show("确定要重启系统吗?", "重启系统", MessageBoxButtons.OKCancel);
            //if (dr == DialogResult.OK)//如果点击“确定”按钮
            //{
            //    Process restartProcess = null;
            //    restartProcess = new Process();
            //    string strPath = Directory.GetCurrentDirectory();
            //    restartProcess.StartInfo.FileName = strPath + @"\RestartApplication.exe";
            //    restartProcess.Start();
            //}
            //else//如果点击“取消”按钮
            //{
            //}            
        }

        /// <summary>
        /// 时间显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sys1sticket_Tick(object sender, EventArgs e)
        {
            string ss = string.Empty;
            ss = DateTime.Now.ToString();
            
            lbDataTime.Text = ss;
        }

        /// <summary>
        /// 设置比对结果
        /// </summary>
        /// <param name="value"></param>
        public void setTextValue(string value)
        {
            label1.Text = value;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private void btnShutdown_Click(object sender, EventArgs e)
        {
            
        }
    }
}
