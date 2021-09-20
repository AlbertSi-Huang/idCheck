using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TS_IDCheck
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    /// <summary>
    /// 坐标点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    /// <summary>
    /// 人脸特征点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FacePointInfo
    {
        public POINT ptEyeLeft;//左眼
        public POINT ptEyeRight;//右眼
        public POINT ptNose;//鼻子
        public POINT ptMouthLeft;//嘴巴左
        public POINT ptMouthRight;//嘴巴右
    }

    public struct FaceRectInfo
    {
        public RECT rc;
        public double roll;
        public double pitch;
        public double yaw;
        public double score;
    }
    public class hsxGFaceSdk
    {
        public const string dllPath = @"GFaceToCSharp.dll";

        [DllImport(dllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace_Init();

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GFace_Detect(byte[] img, int imgSize, int w, int h, int depth, IntPtr ipt,IntPtr ipfpi);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace_GetFeature(byte[] imgRgb24, int w, int h,  FacePointInfo fpi, IntPtr fea);
    }


    public class GFaceSdk
    {
        public const string dllPath = @"GFace7.dll";

        //public const string dllPath = @"D:\E7-Project\src\client\ManageCenter\ManageCenter\bin\DLL\GFace6.dll";


        [DllImport(dllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace7_Init(string pKey);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static void GFace7_Uninit();
        
        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GFace7_GetFeaSize();
        
        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static int GFace7_Detect(uint nThreadID, byte[] pGrayBuf, int nWidth, int nHeight, IntPtr FaceRect, IntPtr FacePoint);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static float GFace7_Compare(uint nThreadID, byte[] pFea1, byte[] pFea2);

        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace7_GetFea(uint nThreadID, byte[] pRgb24Buf, int nWidth, int nHeight, int nCount, FacePointInfo[] FacePoint, IntPtr ptrFea);


        [DllImport(dllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static bool GFace7_CheckKey();

        [DllImport(dllPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public extern static string GFace7_GetVer();
    }
}
