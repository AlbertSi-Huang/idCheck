using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
namespace doublescreen
{
    public partial class popup : Form
    {
        public enum PopupType
        {
            querryNoExist = 0,
            BlackList,
            NoTicket,
            Maxval
        }
       
        public  popup(PopupType mytype, uint i)
        {
           // PopupType myPopupType;
            InitializeComponent();
            if(mytype > PopupType.Maxval)
            {
                mytype = PopupType.Maxval;
            }
            if(mytype == PopupType.NoTicket)
            {

                foreach (Label lab in this.Controls.OfType<Label>())
                {
                    lab.ForeColor = Color.Black;
                }
               
                lbScore.ForeColor = Color.Black;
             }
            else if(mytype == PopupType.BlackList)
            {
                this.BackgroundImage = Image.FromFile(@".\dbpicture\黑名单_无信息.png");
                string mystr = picShow.Single().IdInfo((int)i+1);
                if (mystr != null)
                {
                    if (i == 10) i = 0;
                    picShow.Single().picShowCompPic(i, picofid, picOfSite, picOfSence);
                    if (ConfigOperator.Single().CardReaderBrand == "0")//身份证读卡器读取身份证信息
                    {
                        fmainwindow.SDetectRecord _mycord = JsonConvert.DeserializeObject<fmainwindow.SDetectRecord>(mystr);
                        lbName.Text = _mycord._card._name;
                        lbSex.Text = _mycord._card._sex;
                        lbNational.Text = _mycord._card._nation;
                        lbIdCard.Text = _mycord._card._idNum;

                        lbScore.Text = _mycord._detectScore;
                        if (_mycord._detectResult != 1)
                        {
                            // lbPass.Text = "比对失败";
                            picPass.Image = Image.FromFile(@".\dbpicture\核验失败.png");
                            // myPopupForm.Show();
                        }
                        else
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\核验成功.png");
                            //lbPass.Text = "比对成功";
                        }
                        lbTime.Text = _mycord._createTime;
                    }
                    if (ConfigOperator.Single().CardReaderBrand == "2" || ConfigOperator.Single().CardReaderBrand == "3")//快证通设备读取护照、身份证信息，刘飞翔，20180504
                    {
                        fmainwindow.SDetectPassportRecord _mycord = JsonConvert.DeserializeObject<fmainwindow.SDetectPassportRecord>(mystr);
                        lbName.Text = _mycord._passportCard.Name;
                        lbSex.Text = _mycord._passportCard.Sex;
                        lbNational.Text = _mycord._passportCard.Nation;
                        lbIdCard.Text = _mycord._passportCard.PassportNo;

                        lbScore.Text = _mycord._detectScore;
                        if (_mycord._detectResult != 1)
                        {
                            // lbPass.Text = "比对失败";
                            picPass.Image = Image.FromFile(@".\dbpicture\核验失败.png");
                            // myPopupForm.Show();
                        }
                        else
                        {
                            picPass.Image = Image.FromFile(@".\dbpicture\核验成功.png");
                            //lbPass.Text = "比对成功";
                        }
                        lbTime.Text = _mycord._createTime;
                    }

                }

                //lbblackTip.Text = "公安机关可以对涉嫌吸毒的人员进行必要的检测被检测人员应当予以配合对拒" + "\n" + "绝接受检测的经县级以上人民政府公安机关或者其派出机构负责人批准可以强" + "\n" + "制公安机关应当对吸毒人员进行登记对吸毒成瘾人员公安机关可以责罚";
            //    this.btnDealed.Location = new Point(x, y);
            }
            else if(mytype == PopupType.querryNoExist)
            {
            }
          
        }

        private void btnDealed_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPass_Click(object sender, EventArgs e)
        {
            SocketService.Single().ServerSendOpenDoor();
        }

        private void popup_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Screen[] s1 = System.Windows.Forms.Screen.AllScreens;
            System.Windows.Forms.Screen s0 = System.Windows.Forms.Screen.PrimaryScreen;
            int nPrimaryWidth = 1024;

            //System.Windows.Forms.Screen[] s1 = System.Windows.Forms.Screen.AllScreens;
            if (s1.Length == 1)
            {
                this.Location = new Point(0, 0);
            }
            else if (s1.Length == 2)
            {
                for (int i = 0; i < 2; ++i)
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
            this.Location = new Point(nPrimaryWidth, 0);
            if (timeClose == null)
            {
                timeClose = new System.Timers.Timer(60 * 1000);
            }
            timeClose.Elapsed += TimeCloseElapsed;
            timeClose.Start();
        }

        private void TimeCloseElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Close();
        }

        System.Timers.Timer timeClose = null;

    }
}
