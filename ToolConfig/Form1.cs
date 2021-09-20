using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Common;

namespace ToolConfig
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            m_szConfigFilePath = string.Format("{0}\\{1}", SystemConfig.GetWorkDirectory(), CONFIG_FILE);
        }

        private static string CONFIG_FILE = @"config\SystemConfig.xml";
        private string m_szConfigFilePath = string.Empty;

        private string GetDefaultValue(string szKeyName,string defauleValue)
        {
            return SystemConfig.GetConfigData(m_szConfigFilePath, szKeyName, defauleValue);
        }

        private int GetDefaultValue(string szKeyName,int defauleValue)
        {
            return SystemConfig.GetConfigData(m_szConfigFilePath, szKeyName, defauleValue);
        }

        private float GetDefaultValue(string szKeyName, float defauleValue)
        {
            return SystemConfig.GetConfigData(m_szConfigFilePath, szKeyName, defauleValue);
        }

        private bool GetDefaultValue(string szKeyName, bool defauleValue)
        {
            return SystemConfig.GetConfigData(m_szConfigFilePath, szKeyName, defauleValue);
        }

        private bool SetConfigValue(string szKeyName, string szValue)
        {
            return SystemConfig.WriteConfigData(m_szConfigFilePath, szKeyName, szValue);
        }

        private bool CheckIp(string strIp)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(strIp);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region 终端配置
            string StrRelaySerial = GetDefaultValue("relay_serial", "COM1");
            string StrSavePath = GetDefaultValue("save_path", @"d:\saves");
            string StrSaveDays = GetDefaultValue("save_days", "30");
            string LocalCard = GetDefaultValue("local_card", "");
            string StrThreeDeviceCode = GetDefaultValue("devicecode", "660000FL00100098");
            string StrThreeLicense = GetDefaultValue("threeLicense", "20170621660000FL001660000FL00100098");
            string StrThreeToken = GetDefaultValue("threetoken", "");
            string StrThreeAvilid = GetDefaultValue("threeavilid", "");
            string StrThreeDeviceCodeT = GetDefaultValue("devicecodeT", "");
            string StrThreeLicenseT = GetDefaultValue("threelicenseT", "");
            string StrThreeTokenT = GetDefaultValue("threetokenT", "");
            string StrThreeAvilidT = GetDefaultValue("threeavilidT", "");
            string StrThreeLatitude = GetDefaultValue("threeLatitude", "111.0");
            string StrThreeLongitude = GetDefaultValue("threeLongitude", "112.0");
            string BAutoStart = GetDefaultValue("auto_start", "1");
            string FDetectScore = GetDefaultValue("face_detect_score", "58.0");
            string NInOut = GetDefaultValue("in_or_out", "1");
            string StrSerialNum = GetDefaultValue("serial_num", "ts0001");
            string StrfileUploadCnt = GetDefaultValue("upload_file", "d:\\saves");
            int PlateChoose = Convert.ToInt32(GetDefaultValue("plate_choose", "3"));
            int nDoubleScreenX = Convert.ToInt32(GetDefaultValue("double_width", "768"));
            int nDoubleScreenY = Convert.ToInt32(GetDefaultValue("double_height", "1024"));
            int DoorCloseDelay = Convert.ToInt32(GetDefaultValue("DoorCloseDelay", "3000"));

            SetConfigValue("relay_serial", StrRelaySerial);
            SetConfigValue("save_path", StrSavePath);
            SetConfigValue("save_days", StrSaveDays);
            SetConfigValue("local_card", LocalCard);
            SetConfigValue("devicecode", StrThreeDeviceCode);
            SetConfigValue("threeLicense", StrThreeLicense);
            SetConfigValue("threetoken", StrThreeToken);
            SetConfigValue("StrThreeAvilid", StrThreeAvilid);
            SetConfigValue("devicecodeT", StrThreeDeviceCodeT);
            SetConfigValue("threetokenT", StrThreeTokenT);
            SetConfigValue("threeavilidT", StrThreeAvilidT);
            SetConfigValue("threelicenseT", StrThreeLicenseT);
            SetConfigValue("threeLatitude", StrThreeLatitude);
            SetConfigValue("threeLongitude", StrThreeLongitude);
            SetConfigValue("auto_start", BAutoStart);
            SetConfigValue("face_detect_score", FDetectScore);
            SetConfigValue("in_or_out", NInOut);
            SetConfigValue("serial_num", StrSerialNum);
            SetConfigValue("upload_file", StrfileUploadCnt);
            SetConfigValue("plate_choose", PlateChoose.ToString());
            SetConfigValue("double_width", nDoubleScreenX.ToString());
            SetConfigValue("double_height", nDoubleScreenY.ToString());
            SetConfigValue("DoorCloseDelay", DoorCloseDelay.ToString());

            this.txtComPort.Text = StrRelaySerial;
            this.txtSavePath.Text = StrSavePath;
            this.txtDeviceCode.Text = StrThreeDeviceCode;
            this.txtLicense.Text = StrThreeLicense;
            this.txtToken.Text = StrThreeToken;
            this.txtAvilid.Text = StrThreeAvilid;
            this.txtLatitude.Text = StrThreeLatitude;
            this.txtLongitude.Text = StrThreeLongitude;
            this.txtSerialNum.Text = StrSerialNum;
            this.txt_detectScore.Text = FDetectScore;
            this.txtInOrOut.Text = NInOut;
            
            this.txtPlateChoose.Text = PlateChoose.ToString();

            this.tip_txt.SetToolTip(this.txtInOrOut, "0:出口，1：入口");
            this.tip_txt.SetToolTip(this.txtPlateChoose, "平台选择，1：地方平台，2：视频网平台，3：互联网平台，4：兵团网平台");
            
            tip_txt.SetToolTip(this.txt_detectScore, "人脸识别比对分值");
            
            #endregion 

            Point pBtnSure = new Point(btn_sure.Location.X, this.Height - btn_sure.Height- 50);
            Point pBtnCancle = new Point(btn_cancel.Location.X, this.Height -  btn_cancel.Height - 50);
            btn_sure.Location = pBtnSure;
            btn_cancel.Location = pBtnCancle;
            
        }

        /// <summary>
        /// 保存终端配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_sure_Click(object sender, EventArgs e)
        {
            string strInOut = this.txtInOrOut.Text;
            string strPlateChoose = this.txtPlateChoose.Text;
            string fDetectScore = (this.txt_detectScore.Text);
            
            SetConfigValue("in_or_out", strInOut);
            SetConfigValue("plate_choose", strPlateChoose);
            SetConfigValue("face_detect_score", fDetectScore);
            
            Close();
            this.Dispose();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            Close();
            this.Dispose();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            Pen p1 = new Pen(Color.Blue, 2);
            p1.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            p1.DashPattern = new float[] { 4f, 1f };

            g.DrawLine(p1, 650, 0, 650, this.Height);
        }
    }
}
