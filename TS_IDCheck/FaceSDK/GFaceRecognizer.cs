using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Diagnostics;

namespace TS_IDCheck
{
    public class GFaceRecognizer
    {
        bool inited = false;
        private static GFaceRecognizer instance;
        private static readonly object locker = new object();
        // object obj = new object();

        public static GFaceRecognizer GetInstance()
        {
            if (instance == null)
            {
                instance = new GFaceRecognizer();
            }
            return instance;
        }
        private GFaceRecognizer()
        { }
        
        public bool Init()
        {
            try
            {

                if (!inited)
                {
                    lock (locker)
                    {
                        inited = GFaceSdk.GFace7_Init("TS_G7");
                    }
                }
                return inited;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
        }

        public bool Uninit()
        {
            try
            {
                lock (locker)
                {
                    if(inited)
                        GFaceSdk.GFace7_Uninit();
                }
                    inited = false;
                    return true;
                
            }
            catch
            {
                return false;
            }
        }

        public string GetVersion()
        {
            try
            {
                lock (locker)
                {
                    string ver = GFaceSdk.GFace7_GetVer();
                    return ver;
                }
            }
            catch
            { return ""; }
        }

        public bool CheckKey()
        {
            try
            {
                lock (locker)
                {
                    return GFaceSdk.GFace7_CheckKey();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
        }
        
        public int GetFeaSize()
        {

            try
            {
                lock (locker)
                {
                    return GFaceSdk.GFace7_GetFeaSize();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return 0;
            }

        }

        public int Detect(uint nThreadId,byte[] pRGB24Buf,int width,int height,out RECT rcMax,out FacePointInfo fpiMax)
        {
            int nCount = 0;
            RECT rc = new RECT();
            rcMax = rc;
            FacePointInfo fpi = new FacePointInfo();
            fpiMax = fpi;
            try
            {
                IntPtr piRc = Marshal.AllocHGlobal(1024);
                IntPtr ipFpi = Marshal.AllocHGlobal(1024);
                IntPtr iPtr = IntPtr.Zero;
                lock (locker)
                {
                    nCount = hsxGFaceSdk.GFace_Detect(pRGB24Buf, pRGB24Buf.Length, width, height, 3, piRc, ipFpi);
                }
                if(nCount != 0)
                {
                    IntPtr ii = (IntPtr)(piRc.ToInt64());
                    rcMax = (RECT)Marshal.PtrToStructure(ii, typeof(RECT));

                    IntPtr ipFpit = (IntPtr)(ipFpi.ToInt64());
                    fpiMax = (FacePointInfo)Marshal.PtrToStructure(ipFpit, typeof(FacePointInfo));
                }
            }
            catch
            {

            }
            return nCount;
        }
        private int MakePositive(int n)
        {
            return n > 0 ? n : 0;
        }

        private int FindMaxFace(int nCount, RECT[] rc)
        {
            int nMaxVPixels = 0;
            int nVpixels = 0;
            int nIndex = -1;
            for (int i = 0; i < nCount; ++i)
            {
                nVpixels = (MakePositive(rc[i].right) - MakePositive(rc[i].left)) * (MakePositive(rc[i].bottom) - MakePositive(rc[i].top));
                if (nVpixels > nMaxVPixels)
                {
                    nMaxVPixels = nVpixels;
                    nIndex = i;
                }
            }

            return nIndex;
        }
        
        /// <summary>
        /// 人脸检测
        /// </summary>
        /// <param name="nThreadID"></param>
        /// <param name="pGrayBuf"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="rect"></param>
        /// <param name="facePoint"></param>
        /// <returns></returns>
        public int Detect(uint nThreadID, byte[] pGrayBuf, int nWidth, int nHeight, out RECT[] rect, out FacePointInfo[] facePoint)
        {
            int nFaceNum = 0;
            try
            {
                IntPtr ptrRect = Marshal.AllocHGlobal(4096);
                IntPtr ptrFacePoint = Marshal.AllocHGlobal(4096);
                lock (locker)
                {
                    nFaceNum = GFaceSdk.GFace7_Detect(nThreadID, pGrayBuf, nWidth, nHeight, ptrRect, ptrFacePoint);
                }
                facePoint = new FacePointInfo[nFaceNum];
                rect = new RECT[nFaceNum];
                FaceRectInfo[] fri = new FaceRectInfo[nFaceNum];
                for (int i = 0; i < nFaceNum; i++)
                {
                    //rect[i] = new RECT();
                    //rect[i] = (RECT)Marshal.PtrToStructure((IntPtr)(ptrRect.ToInt64() + i * Marshal.SizeOf(rect[i])), typeof(RECT));

                    fri[i] = (FaceRectInfo)Marshal.PtrToStructure((IntPtr)(ptrRect.ToInt64() + i * Marshal.SizeOf(fri[i])), typeof(FaceRectInfo));
                    rect[i] = fri[i].rc;

                    facePoint[i] = new FacePointInfo();
                    facePoint[i] = (FacePointInfo)Marshal.PtrToStructure((IntPtr)(ptrFacePoint.ToInt64() + i * Marshal.SizeOf(facePoint[i])), typeof(FacePointInfo));
                }
                Marshal.FreeHGlobal(ptrRect);
                Marshal.FreeHGlobal(ptrFacePoint);


                if (nFaceNum > 1)
                {
                    int nMax = FindMaxFace(nFaceNum, rect);
                    //Trace.WriteLine("抓拍到 " + nFaceNum.ToString() + "人 ,最大脸的是第 " + nMax.ToString() + " 人");
                    if (nMax != 0)
                    {
                        rect[0] = rect[nMax];
                        facePoint[0] = facePoint[nMax];
                    }
                }

                return nFaceNum;
            }
            catch (Exception e)
            {
                rect = new RECT[1];
                facePoint = new FacePointInfo[1];
                MessageBox.Show(e.ToString());
                return 0;
            }
        }

        
        /// <summary>
        /// 特征提取
        /// </summary>
        /// <param name="pRgb24"></param>
        /// <param name="pRgb8"></param>
        /// <param name="nW"></param>
        /// <param name="nH"></param>
        /// <param name="nThreadID"></param>
        /// <param name="pFea"></param>
        /// <returns></returns>
        public bool GetFea(uint nThreadID, byte[] pRgb24, int nW, int nH, FacePointInfo facePoint, out byte[] pFea)
        {
            pFea = null;
            try
            {

                FacePointInfo[] faceTempPoint = new FacePointInfo[1];
                faceTempPoint[0] = facePoint;
                bool flag = true;
                IntPtr ptrFea = Marshal.AllocHGlobal(1024 * 8192);
                lock (locker)
                {
                    flag = GFaceSdk.GFace7_GetFea(nThreadID, pRgb24, nW, nH, 1, faceTempPoint, ptrFea);
                }
                    if (!flag)
                    {
                        return false;
                    }
                    pFea = new byte[GetFeaSize()];
                    Marshal.Copy(ptrFea, pFea, 0, pFea.Length);
                    Marshal.FreeHGlobal(ptrFea);
                    return flag;
                
            }
            catch
            {
                return false;
            }
            
        }



        public float Compare(uint nThreadID, byte[] pFea1, byte[] pFea2)
        {
            float result = 0;
            try
            {
                lock (locker)
                {
                    result = GFaceSdk.GFace7_Compare(nThreadID, pFea1, pFea2);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return result;
        }

        /// <summary>
        /// 读取照片文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public void WriteImageFile(Bitmap bmp, string path)
        {
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        /// <summary>
        /// 读取照片文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Bitmap ReadImageFile(string path)
        {
            Trace.WriteLine("read card img to Bitmap:" + path);
            if (!File.Exists(path)) return null;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); //File.OpenRead(path); //OpenRead
            int filelength = 0;
            filelength = (int)fs.Length; //获得文件长度 
            Byte[] image = new Byte[filelength]; //建立一个字节数组 
            fs.Read(image, 0, filelength); //按字节流读取 
            System.Drawing.Image result = System.Drawing.Image.FromStream(fs);
            fs.Close();
            fs.Dispose();
            Bitmap bit = new Bitmap(result);
            return bit;
        }


        /// <summary>  
        /// 获取图像rgb24buf。  
        /// </summary>  
        /// <param name="original"> 源图像。 </param>  
        /// <returns> 8位灰度图像。 </returns>  
        public byte[] GetRgb24Buf(Bitmap original)
        {
            if (original != null)
            {
                // 将源图像内存区域锁定  
                Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
                BitmapData bmpData = original.LockBits(rect, ImageLockMode.ReadOnly,
                        original.PixelFormat);

                // 获取图像参数  
                int width = bmpData.Width;
                int height = bmpData.Height;
                int stride = bmpData.Stride;  // 扫描线的宽度  
                int offset = stride - width * 3;  // 显示宽度与扫描线宽度的间隙  
                IntPtr ptr = bmpData.Scan0;   // 获取bmpData的内存起始位置  
                int scanBytes = stride * height;  // 用stride宽度，表示这是内存区域的大小  

                // 分别设置两个位置指针，指向源数组和目标数组  
                int posScan = 0, posDst = 0;
                byte[] rgbValues = new byte[scanBytes];  // 为目标数组分配内存  
                Marshal.Copy(ptr, rgbValues, 0, scanBytes);  // 将图像数据拷贝到rgbValues中  
                                                                // 分配灰度数组  
                byte[] rgb24Values = new byte[width * height * 3]; // 不含未用空间。  
                                                                    // 计算灰度数组  
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        rgb24Values[posDst] = rgbValues[posScan];
                        posDst++; posScan++;
                        rgb24Values[posDst] = rgbValues[posScan];
                        posDst++; posScan++;
                        rgb24Values[posDst] = rgbValues[posScan];
                        posDst++; posScan++;
                        posScan++;

                    }
                    // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel  
                    // posScan += offset;
                }

                // 内存解锁  
                Marshal.Copy(rgbValues, 0, ptr, scanBytes);
                original.UnlockBits(bmpData);  // 解锁内存区域  


                return rgb24Values;
            }
            else
            {
                return null;
            }  
        }
        /// <summary>  
        /// 将源图像灰度化，并转化为8位灰度图像。  
        /// </summary>  
        /// <param name="original"> 源图像。 </param>  
        /// <returns> 8位灰度图像。 </returns>  
        public byte[] GetRgb8Buf(Bitmap original)
        {
            if (original != null)
            {
                // 将源图像内存区域锁定  
                Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
                BitmapData bmpData = original.LockBits(rect, ImageLockMode.ReadOnly,
                        original.PixelFormat);

                // 获取图像参数  
                int width = bmpData.Width;
                int height = bmpData.Height;
                int stride = bmpData.Stride;  // 扫描线的宽度  
                int offset = stride - width * 3;  // 显示宽度与扫描线宽度的间隙  
                IntPtr ptr = bmpData.Scan0;   // 获取bmpData的内存起始位置  
                int scanBytes = stride * height;  // 用stride宽度，表示这是内存区域的大小  

                // 分别设置两个位置指针，指向源数组和目标数组  
                int posScan = 0, posDst = 0;
                byte[] rgbValues = new byte[scanBytes];  // 为目标数组分配内存  
                Marshal.Copy(ptr, rgbValues, 0, scanBytes);  // 将图像数据拷贝到rgbValues中  
                                                                // 分配灰度数组  
                byte[] grayValues = new byte[width * height]; // 不含未用空间。  
                                                                // 计算灰度数组  
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        double temp = rgbValues[posScan++] * 0.11 +
                            rgbValues[posScan++] * 0.59 +
                            rgbValues[posScan++] * 0.3;
                        grayValues[posDst++] = (byte)temp;
                        posScan++;

                    }
                    // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel  
                    // posScan += offset;
                }

                // 内存解锁  
                Marshal.Copy(rgbValues, 0, ptr, scanBytes);
                original.UnlockBits(bmpData);  // 解锁内存区域  


                return grayValues;
            }
            else
            {
                return null;
            } 
        }


        public bool GetRgb8And24Buf(Bitmap original, out byte[] rgb8, out byte[] rgb24)
        {
            rgb8 = null;
            rgb24 = null;
            if (original != null)
            {
                // 将源图像内存区域锁定  
                Rectangle rect = new Rectangle(0, 0, original.Width, original.Height);
                BitmapData bmpData = original.LockBits(rect, ImageLockMode.ReadOnly,
                        original.PixelFormat);

                bool bPix24 = original.PixelFormat == PixelFormat.Format24bppRgb ? true : false;

                // 获取图像参数  
                int width = bmpData.Width;
                int height = bmpData.Height;
                int stride = bmpData.Stride;  // 扫描线的宽度  
                int offset = stride - width * 3;  // 显示宽度与扫描线宽度的间隙  
                IntPtr ptr = bmpData.Scan0;   // 获取bmpData的内存起始位置  
                int scanBytes = stride * height;  // 用stride宽度，表示这是内存区域的大小  

                // 分别设置两个位置指针，指向源数组和目标数组  
                int posScan = 0, posDst = 0;
                int posRgb8 = 0;
                byte[] rgbValues = new byte[scanBytes];  // 为目标数组分配内存  
                Marshal.Copy(ptr, rgbValues, 0, scanBytes);  // 将图像数据拷贝到rgbValues中  
                                                                // 分配灰度数组  
                rgb24 = new byte[width * height * 3]; // 不含未用空间。  
                rgb8 = new byte[width * height]; // 不含未用空间。  
                                                    // 计算灰度数组  
                for (int i = 0; i < height; i++)
                {

                    for (int j = 0; j < width; j++)
                    {

                        rgb24[posDst] = rgbValues[posScan];
                        double r = rgbValues[posScan] * 0.11;
                        posDst++;
                        posScan++;
                        rgb24[posDst] = rgbValues[posScan];
                        double g = rgbValues[posScan] * 0.59;
                        posDst++;
                        posScan++;
                        rgb24[posDst] = rgbValues[posScan];
                        double b = rgbValues[posScan] * 0.3;
                        posDst++;
                        posScan++;
                        if (!bPix24)
                            posScan++;
                        rgb8[posRgb8] = (byte)(r + g + b);
                        posRgb8++;
                    }

                    // 跳过图像数据每行未用空间的字节，length = stride - width * bytePerPixel  
                    // posScan += offset;
                }

                // 内存解锁  
                Marshal.Copy(rgbValues, 0, ptr, scanBytes);
                original.UnlockBits(bmpData);  // 解锁内存区域  


                return true;
            }
            return false;
        }  
    }
}
