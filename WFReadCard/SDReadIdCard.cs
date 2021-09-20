using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WFReadCard
{
    public class SDReadIdCard
    {
        const  string dllPath = @"F:\hsx\三所侧屏20171226_15点48分\三所侧屏20171226_15点48分\三所侧屏20171226\WFReadCard\bin\Debug\sdtapi.dll";

        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall)]
        public static extern int SDT_OpenPort(int port);

        //[DllImport("sdtapi_x64.dll")]
        //static extern int SDT_StartFindIDCard(int iPort, byte[] pucManaInfo, int iIfOpen);

        //[DllImport("sdtapi_x64.dll")]
        //static extern int SDT_SelectIDCard(int iPort, byte[] pucManaMsg, int iIfOpen);

        //[DllImport("sdtapi_x64.dll")]
        //static extern int SDT_ReadBaseMsgToFile(int iPort, string pcCHMsgFileName, byte[] puiCHMsgFileLen, string pcPHMsgFileName, ref int puiPHMsgFileLen, int iIfOpen);

        ////[DllImport("sdtapi_x64.dll")]
        ////static extern int SDT_ReadBaseFPMsg(int iPort, byte[] pucCHMsg, ref int puiCHMsgLen, byte[] pucPHMsg, unsigned int* puiPHMsgLen, unsigned char* pucFPMsg, unsigned int* puiFPMsgLen, int iIfOpen);
        #region API声明
        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall)]
        public static extern int SDT_StartFindIDCard(int iPort, byte[] pucManaInfo, int iIfOpen);
        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall)]
        public static extern int SDT_SelectIDCard(int iPort, byte[] pucManaMsg, int iIfOpen);
        [DllImport(dllPath, CallingConvention = CallingConvention.StdCall)]
        public static extern int SDT_ReadBaseMsg(int iPort, byte[] pucCHMsg, ref UInt32 puiCHMsgLen, byte[] pucPHMsg, ref UInt32 puiPHMsgLen, int iIfOpen);
        #endregion

    }
}
