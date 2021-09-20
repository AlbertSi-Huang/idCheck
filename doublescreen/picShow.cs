using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace doublescreen
{
    class picShow
    {
        const UInt32 CntPerPage = 10;
        private Point[] myPoint = new Point[CntPerPage];
        private Int32[] myWidth = new Int32[CntPerPage];
        private Int32[] myHeight = new Int32[CntPerPage];
        private const Int32 sResizeInc = 30;
        private string myPath = string.Empty;

        private static byte[][] IdImgArrayByte = new byte[CntPerPage][];
        private static byte[][] siteImgArrayByte = new byte[CntPerPage][];
        private static byte[][] sceneImgArrayByte = new byte[CntPerPage][];
        private static  string[] myjson = new string[CntPerPage];
        private uint[] nArrImgList = { 100, 100, 100, 100, 100, 100, 100, 100 };
        private static picShow instance = null;
        public static picShow Single()
        {
            if (instance == null)
            {
                instance = new picShow();
            }
            return instance;
        }

        private byte[] converByte(string imgFile)
        {
            if (File.Exists(imgFile))
            {
                using (FileStream fs = new FileStream(imgFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    int nFileLen = (int)fs.Length;
                    byte[] bytes = new byte[nFileLen];
                    fs.Read(bytes, 0, nFileLen);
                    return bytes;
                }
            }
            return null;
        }

        private byte[] convertByte(Image img)
        {
            if (img != null)
            {
                MemoryStream ms = new MemoryStream();
                img.Save(ms, img.RawFormat);
                //以上两句改成下面两句
                byte[] bytes = ms.ToArray();
                ms.Close();
                ms.Dispose();
                ms = null;
                return bytes;
            }
            else
            {
                return null;
            }
        }

        ///二进制生成图片
        private Image convertImg(byte[] datas)
        {
            if (datas != null)
            {
                MemoryStream ms = new MemoryStream(datas);
                Image img = Image.FromStream(ms, true);
                ms.Close();
                ms.Dispose();
                ms = null;
                return img;
            }
            else
            {
                return null;
            }
        }
        public void mypicShow()
        {
        }

        public void mypicShow(PictureBox[] pics, PictureBox Idimg, PictureBox SiteImg, string path)
        {
            UInt32[] bArry = new UInt32[8];
            myPath = path;
            for (UInt32 i = 0; i < 8; i++)
            {
                myPoint[i].X = pics[i].Location.X;
                myPoint[i].Y = pics[i].Location.Y;
                myWidth[i] = pics[i].Width;
                myHeight[i] = pics[i].Height;
                bArry[i] = i + 1;
            }
        }



        public void picNew(UInt32 ptr)
        {
            ptr += 1;
            if (ptr == 11) ptr = 1;
            string fileName = myPath + @"cardImgd/" + ptr.ToString() + ".jpg";
            
            if (File.Exists(fileName))
            {
                //Image myimage = Image.FromFile(fileName);
                IdImgArrayByte[ptr - 1] = converByte(fileName);//convertByte(myimage);
                //myimage.Dispose();
                //myimage = null;
            }
            
            fileName = myPath + @"siteImgd/" + ptr.ToString() + ".jpg";

            if (File.Exists(fileName))
            {
                //Image myimage = Image.FromFile(fileName);
                siteImgArrayByte[ptr-1] = converByte(fileName);//convertByte(myimage);
                //myimage.Dispose();
                //myimage = null;
            }
            
            fileName = myPath + @"sceneImgd/" + ptr.ToString() + ".jpg";

            if (File.Exists(fileName))
            {
//                Image myimage = Image.FromFile(fileName);
                sceneImgArrayByte[ptr-1] = converByte(fileName);//convertByte(myimage);
 //               myimage.Dispose();
 //               myimage = null;
            }
            fileName = myPath + @"jsonRecord/"+ ptr.ToString() + ".txt";
            if (File.Exists(fileName))
            {
                string mystr = string.Empty;
                StreamReader sr = new StreamReader(fileName, Encoding.Default);
                mystr = sr.ReadLine();
                myjson[ptr-1] = mystr;
                sr.Close();
                sr.Dispose();
                sr = null;
            }
           // GC.Collect();

        }

        private void picShowGetDefault(PictureBox[] pics)
        {
            for (int i = 0; i < 8; i++)
            {
                pics[i].Location = new Point(myPoint[i].X, myPoint[i].Y);
                pics[i].Width = myWidth[i];
                pics[i].Height = myHeight[i];
            }
        }
        public void picShowBottomHigtLine(PictureBox[] pics, UInt32 index)
        {
            pics[index].Location = new Point(myPoint[index].X - sResizeInc / 2, myPoint[index].Y - sResizeInc); //
            pics[index].Width = myWidth[index] + sResizeInc;
            pics[index].Height = myHeight[index] + sResizeInc;
            pics[index].SetBounds(myPoint[index].X - sResizeInc / 2,
                myPoint[index].Y - sResizeInc, myWidth[index] + sResizeInc,myHeight[index] + sResizeInc);
            
        }

        public void ShowHitInfo(uint index,PictureBox cardImg,PictureBox siteImg) 
        {
            uint nPos = index;// > 0 ? index - 1 : 0;
            if (IdImgArrayByte[nPos] != null)
            {
                Image myimage = convertImg(IdImgArrayByte[nArrImgList[nPos] - 1]);
                cardImg.Image = myimage;

                myimage = convertImg(siteImgArrayByte[nArrImgList[nPos] - 1]);
                siteImg.Image = myimage;
            }
        }
        
        public void picShowBottom(PictureBox[] pics, UInt32[] curp)
        {

            picShowGetDefault(pics);

            for (int i = 0; i < 8; i++)
            {
                if (IdImgArrayByte[curp[i] - 1] != null)
                {
                    Image myimage = convertImg(sceneImgArrayByte[curp[i] - 1]);
                    pics[i].Image = myimage;
                }
            }

            nArrImgList = curp;
        }

        public void picShowBottom(PictureBox[] pics)
        {
            picShowGetDefault(pics);
            for (int i = 0; i < 8; i++)
            {
                pics[i].Refresh();
            }
        }

        public void picShowCompPic(UInt32 curp, PictureBox myPic1, PictureBox myPic2)
        {
            uint nPos = curp > 0 ? curp - 1 : 0;
            Image myimage = convertImg(IdImgArrayByte[nPos]);
            myPic1.Image = myimage;
            myimage = convertImg(siteImgArrayByte[nPos]);
            myPic2.Image = myimage; 
        }
        public void picShowCompPic(UInt32 curp, PictureBox myPic1, PictureBox myPic2,PictureBox myPic3)
        {
            Image myimage = convertImg(IdImgArrayByte[curp]);
            myPic1.Image = myimage;
            myimage = convertImg(siteImgArrayByte[curp]);
            myPic2.Image = myimage;
            myimage = convertImg(sceneImgArrayByte[curp]);
            myPic3.Image = myimage;

        }

        public string GetClickRecordInfo(int clickId)
        {
            return IdInfo((int)nArrImgList[clickId]);
        }

        public  string  IdInfo(Int32 curp)
        {
            int nPos = curp > 0 ? curp - 1 : 0;
            if (nPos == 10) nPos = 0;
            string ss = string.Empty;
            Console.WriteLine("Json point is {0}", curp);
            ss = myjson[nPos];
            Console.WriteLine(ss);
            return ss;
        }
        public void PicFromNetUpData(PictureBox myPic)
        {
            string fileName = myPath + @"Querrypic/" + "tmp.jpg";

            if (File.Exists(fileName))
            {
                Image myimage = Image.FromFile(fileName);
                byte[] ImgArrayByte = convertByte(myimage);
                myPic.Image = convertImg(ImgArrayByte);
                myimage.Dispose();
                myimage = null;
                GC.Collect();
            }
        }
    }
}
