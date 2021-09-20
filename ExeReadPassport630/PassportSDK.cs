using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExeReadPassport630
{
    public class PassportSDK
    {
        #region DLL引用
        [DllImport("kernel32")]
        public static extern int LoadLibrary(string strDllName);

        const string idcardPath = @".\IDCard.dll";
        const string sdtapiPath = @".\sdtapi.dll";
        const string wslPath = @".\WltRS.dll";

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int InitIDCard(char[] cArrUserID, int nType, char[] cArrDirectory);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetRecogResult(int nIndex, char[] cArrBuffer, ref int nBufferLen);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogIDCard();

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetFieldName(int nIndex, char[] cArrBuffer, ref int nBufferLen);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int AcquireImage(int nCardType);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SaveImage(char[] cArrFileName);
        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SaveHeadImage(char[] cArrFileName);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int CheckUVDull(bool bForceAcquire, int nReserve);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetCurrentDevice(char[] cArrDeviceName, int nLength);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern void GetVersionInfo(char[] cArrVersion, int nLength);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool CheckDeviceOnline();

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool SetAcquireImageType(int nLightType, int nImageType);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool SetUserDefinedImageSize(int nWidth, int nHeight);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern bool SetAcquireImageResolution(int nResolutionX, int nResolutionY);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SetIDCardID(int nMainID, int[] nSubID, int nSubIdCount);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int AddIDCardID(int nMainID, int[] nSubID, int nSubIdCount);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogIDCardEX(int nMainID, int nSubID);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetButtonDownType();

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetGrabSignalType();

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SetSpecialAttribute(int nType, int nSet);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern void FreeIDCard();
        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetDeviceSN(char[] cArrSn, int nLength);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBusinessCardResult(int nID, int nIndex, char[] cArrBuffer, ref int nBufferLen);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogBusinessCard(int nType);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBusinessCardFieldName(int nID, char[] cArrBuffer, ref int nBufferLen);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBusinessCardResultCount(int nID);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int LoadImageToMemory(string path, int nType);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int ClassifyIDCard(ref int nCardType);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogChipCard(int nDGGroup, bool bRecogVIZ, int nSaveImageType);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogGeneralMRZCard(bool bRecogVIZ, int nSaveImageType);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int RecogCommonCard(int nSaveImageType);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int SaveImageEx(char[] lpFileName, int nType);

        [DllImport(idcardPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetDataGroupContent(int nDGIndex, bool bRawData, byte[] lpBuffer, ref int len);

        [DllImport(sdtapiPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_OpenPort(int iPort);
        [DllImport(sdtapiPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_ClosePort(int iPort);

        [DllImport(sdtapiPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_StartFindIDCard(int iPort, ref byte pRAPDU, int iIfOpen);

        [DllImport(sdtapiPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_SelectIDCard(int iPort, ref byte pRAPDU, int iIfOpen);

        [DllImport(sdtapiPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_ReadBaseMsg(int iPort, ref byte pucCHMsg, ref int puiCHMsgLen, ref byte pucPHMsg, ref int puiPHMsgLen, int iIfOpen);

        [DllImport(sdtapiPath, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern int SDT_ReadNewAppMsg(int iPort, ref byte pucAppMsg, ref int puiAppMsgLen, int iIfOpen);

        [DllImport(wslPath, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern int GetBmp(string filename, int nType);

        #endregion
    }
}
