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
            string strSerialNum = GetDefaultValue("serial_number", "szttop");
            string strSerialName = GetDefaultValue("serial_name", "taisau");
            string strManagerIp = GetDefaultValue("manager_ip", "127.0.0.1");
            int nPort = GetDefaultValue("manager_port", 8899);
            int nInOut = GetDefaultValue("in_or_out", 1);
            bool bPlateEnable = GetDefaultValue("enable_plate", false);
            string strPlateIp = GetDefaultValue("plate_ip", "192.168.1.108");
            int nPlatePort = GetDefaultValue("plate_port", 554);
            string strPlateName = GetDefaultValue("plate_name", "admin");
            string strPlatePwd = GetDefaultValue("plate_password", "admin");
            float fDetectScore = GetDefaultValue("face_detect_score", 58f);
            int nCameraType = GetDefaultValue("camera_type", 1);
            string strCameraIp = GetDefaultValue("camera_ip", "192.168.0.108");
            int nCameraPort = GetDefaultValue("camera_port", 554);
            string strCameraName = GetDefaultValue("camera_name", "admin");
            string strCameraPwd = GetDefaultValue("camera_pwassword", "admin");
            string strLocalMan = GetDefaultValue("local_card", "441481");
            string strFaceFrame = GetDefaultValue("face_frame", "60,30,60,30");

            SetConfigValue("serial_number", strSerialNum);
            SetConfigValue("serial_name", strSerialName);
            SetConfigValue("manager_ip", strManagerIp);
            SetConfigValue("manager_port", nPort.ToString());
            SetConfigValue("in_or_out", nInOut.ToString());
            SetConfigValue("enable_plate", bPlateEnable.ToString());
            SetConfigValue("plate_ip", strPlateIp);
            SetConfigValue("plate_port", nPlatePort.ToString());
            SetConfigValue("plate_name", strPlateName);
            SetConfigValue("plate_password", strPlatePwd);
            SetConfigValue("face_detect_score", fDetectScore.ToString());
            SetConfigValue("camera_type", nCameraType.ToString());
            SetConfigValue("camera_ip", strCameraIp);
            SetConfigValue("camera_port", nCameraPort.ToString());
            SetConfigValue("camera_name", strCameraName);
            SetConfigValue("camera_pwassword", strCameraPwd);
            SetConfigValue("local_card", strLocalMan);
            SetConfigValue("face_frame", strFaceFrame);

            this.txt_serialNo.Text = strSerialNum;
            this.txt_serialName.Text = strSerialName;
            this.txt_managerIp.Text = strManagerIp;
            this.txt_managerPort.Text = nPort.ToString();
            this.txt_inOrOut.Text = nInOut.ToString();
            this.txt_plateIp.Text = strPlateIp;
            this.txt_platePort.Text = nPlatePort.ToString();
            this.txt_plateUserName.Text = strPlateName;
            this.txt_platePwd.Text = strPlatePwd;
            this.txt_detectScore.Text = fDetectScore.ToString();
            this.txt_cameraType.Text = nCameraType.ToString();
            this.txt_cameraIp.Text = strCameraIp;
            this.txt_cameraPort.Text = nCameraPort.ToString();
            this.txt_cameraName.Text = strCameraName;
            this.txt_cameraPwd.Text = strCameraPwd;
            this.txt_localMan.Text = strLocalMan;
            this.txt_faceFrame.Text = strFaceFrame;

            if (bPlateEnable)
            {
                chb_enablePlate.CheckState = CheckState.Checked;
            }else
            {
                chb_enablePlate.CheckState = CheckState.Unchecked;
            }
            
            tip_txt.SetToolTip(this.txt_inOrOut, "1 为入口 2 为出口");
            tip_txt.SetToolTip(this.chb_enablePlate, "1 为开启 0 为关闭 默认关闭");
            tip_txt.SetToolTip(this.txt_plateIp, "车牌摄像头ip  车牌识别开启后填写");
            tip_txt.SetToolTip(this.txt_platePort, "车牌摄像头端口  车牌识别开启后填写");
            tip_txt.SetToolTip(this.txt_plateUserName, "车牌摄像头用户名  车牌识别开启后填写");
            tip_txt.SetToolTip(this.txt_platePwd, "车牌摄像头密码  车牌识别开启后填写");

            tip_txt.SetToolTip(this.txt_localMan, "本地身份证号");
            tip_txt.SetToolTip(this.txt_cameraType, "1 是usb摄像头 2 是天视通");
            tip_txt.SetToolTip(this.txt_cameraIp, "像头类型为网络摄像头时填写");
            tip_txt.SetToolTip(this.txt_cameraPort, "摄像头类型为网络摄像头时填写");
            tip_txt.SetToolTip(this.txt_cameraName, "摄像头类型为网络摄像头时填写");
            tip_txt.SetToolTip(this.txt_cameraPwd, " 摄像头类型为网络摄像头时填写");
            tip_txt.SetToolTip(this.txt_detectScore, "人脸识别比对分值");
            tip_txt.SetToolTip(this.txt_faceFrame, "人脸矩形,四个值分别用逗号隔开");

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
            string strSerialNum = this.txt_serialNo.Text;
            string strSerialName = this.txt_serialName.Text;
            string strManagerIp = this.txt_managerIp.Text;
            int nPort = Convert.ToInt16(this.txt_managerPort.Text);
            int nInOut = Convert.ToInt16(this.txt_inOrOut.Text);
            bool bPlateEnable = false;
            if(this.chb_enablePlate.CheckState == CheckState.Checked)
            {
                bPlateEnable = true;
            }
            string strPlateIp = this.txt_plateIp.Text;
            int nPlatePort = Convert.ToInt16(this.txt_platePort.Text);
            string strPlateName = this.txt_plateUserName.Text;
            string strPlatePwd = this.txt_platePwd.Text;
            float fDetectScore = (float)Convert.ToDouble(this.txt_detectScore.Text);
            int nCameraType = Convert.ToInt16(this.txt_cameraType.Text);
            string strCameraIp = this.txt_cameraIp.Text;
            int nCameraPort = Convert.ToInt16(this.txt_cameraPort.Text);
            string strCameraName = this.txt_cameraName.Text;
            string strCameraPwd = this.txt_cameraPwd.Text;
            string strLocalCard = this.txt_localMan.Text;
            string strFaceFrame = this.txt_faceFrame.Text;

            SetConfigValue("serial_number", strSerialNum);
            SetConfigValue("serial_name", strSerialName);
            SetConfigValue("manager_ip", strManagerIp);
            SetConfigValue("manager_port", nPort.ToString());
            SetConfigValue("in_or_out", nInOut.ToString());
            SetConfigValue("enable_plate", bPlateEnable.ToString());
            SetConfigValue("plate_ip", strPlateIp);
            SetConfigValue("plate_port", nPlatePort.ToString());
            SetConfigValue("plate_name", strPlateName);
            SetConfigValue("plate_password", strPlatePwd);
            SetConfigValue("face_detect_score", fDetectScore.ToString());
            SetConfigValue("camera_type", nCameraType.ToString());
            SetConfigValue("camera_ip", strCameraIp);
            SetConfigValue("camera_port", nCameraPort.ToString());
            SetConfigValue("camera_name", strCameraName);
            SetConfigValue("camera_pwassword", strCameraPwd);
            SetConfigValue("local_card", strLocalCard);
            SetConfigValue("face_frame", strFaceFrame);

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
