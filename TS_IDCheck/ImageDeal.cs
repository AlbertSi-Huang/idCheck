using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS_IDCheck
{
    public  class ImageDeal
    {
        /// <summary>
        /// 截取图片方法
        /// </summary>
        /// <param name="url">图片地址</param>
        /// <param name="beginX">开始位置-X</param>
        /// <param name="beginY">开始位置-Y</param>
        /// <param name="getX">截取宽度</param>
        /// <param name="getY">截取长度</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="fileExt">后缀名</param>
        public static string CutImage(string url, int beginX, int beginY, int getX, int getY, string fileName, string savePath, string fileExt)
        {
            #region
            //if ((beginX < getX) && (beginY < getY))
            //{
            //    Bitmap bitmap = new Bitmap(url);//原图
            //    if (((beginX + getX) <= bitmap.Width) && ((beginY + getY) <= bitmap.Height))
            //    {
            //        Bitmap destBitmap = new Bitmap(getX, getY);//目标图
            //        Rectangle destRect = new Rectangle(0, 0, getX, getY);//矩形容器
            //        Rectangle srcRect = new Rectangle(beginX, beginY, getX, getY);


            //        Graphics.FromImage(destBitmap);
            //        Graphics.DrawImage(bitmap, destRect, srcRect, GraphicsUnit.Pixel);

            //        ImageFormat format = ImageFormat.Png;
            //        switch (fileExt.ToLower())
            //        {
            //            case "png":
            //                format = ImageFormat.Png;
            //                break;
            //            case "bmp":
            //                format = ImageFormat.Bmp;
            //                break;
            //            case "gif":
            //                format = ImageFormat.Gif;
            //                break;
            //        }
            //        destBitmap.Save(savePath + "//" + fileName, format);
            //        return savePath + "\\" + "*" + fileName.Split('.')[0] + "." + fileExt;
            //    }
            //    else
            //    {
            //        return "截取范围超出图片范围";
            //    }
            //}
            //else
            //{
            //    return "请确认(beginX < getX)&&(beginY < getY)";
            //}
            #endregion
           
            Bitmap bmpBase = new Bitmap(url);

            // 画像を切り抜く
            Rectangle rect = new Rectangle(beginX, beginY, getX, getY);
            Bitmap bmpNew = bmpBase.Clone(rect, bmpBase.PixelFormat);

            ImageFormat format = ImageFormat.Png;
            switch (fileExt.ToLower())
            {
                case "png":
                    format = ImageFormat.Png;
                    break;
                case "bmp":
                    format = ImageFormat.Bmp;
                    break;
                case "gif":
                    format = ImageFormat.Gif;
                    break;
            }

            // 画像をGIF形式で保存
            bmpNew.Save(savePath + "//" + fileName, format);

            // 画像リソースを解放
            bmpBase.Dispose();
            bmpNew.Dispose();
            return "ok";
        }

        /// <summary>
        /// 把img 对象转成byte[]
        /// </summary>
        /// <param name="imgIn"></param>
        /// <returns>返回对应的byte[]</returns>
        public static byte[] ImageToByteArray(Image imgIn)
        {
            MemoryStream ms = new MemoryStream();
            imgIn.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }


        public static Image ByteArrayToImage(byte[] byteArr,int count)
        {
            MemoryStream ms = new MemoryStream(byteArr, 0, count);
            ms.Position = 0;
            Image Img = Image.FromStream(ms);
            return Img;
        }

        public static Bitmap CutImageByMem(Bitmap bm, int beginX, int beginY, int getX, int getY)
        {
            Rectangle rect = new Rectangle(beginX, beginY, getX, getY);
            Bitmap bmpNew = bm.Clone(rect, bm.PixelFormat);

            return bmpNew;
        }

    }
}
