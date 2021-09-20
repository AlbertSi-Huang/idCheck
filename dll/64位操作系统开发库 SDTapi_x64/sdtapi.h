#ifdef  _WIN32
#define STDCALL  __stdcall
#else
#define STDCALL
#endif
#ifndef SDTAPI_
#define SDTAPI_
#ifdef __cplusplus
extern "C"{
#endif 
/**********************************************************
 ********************** 端口类API *************************
 **********************************************************/
int STDCALL SDT_GetCOMBaud(int iPort, unsigned int *puiBaudRate);
int STDCALL SDT_SetCOMBaud(int iPort, unsigned int uiCurrBaud, unsigned int uiSetBaud);
int STDCALL SDT_OpenPort(int iPort);
int STDCALL SDT_ClosePort(int iPort);

/**********************************************************
 ********************** SAM类API **************************
 **********************************************************/
int STDCALL SDT_ResetSAM(int iPort, int iIfOpen);
int STDCALL SDT_SetMaxRFByte(int iPort, unsigned char ucByte, int bIfOpen);
int STDCALL SDT_GetSAMStatus(int iPort, int iIfOpen);
int STDCALL SDT_GetSAMID(int iPort, unsigned char * pucSAMID, int iIfOpen);
int STDCALL SDT_GetSAMIDToStr(int iPort, char *pcSAMID, int iIfOpen);

/**********************************************************
 ******************* 身份证卡类API ************************
 **********************************************************/
int	STDCALL SDT_StartFindIDCard(int iPort, unsigned char *pucManaInfo, int iIfOpen);
int	STDCALL SDT_SelectIDCard(int iPort, unsigned char *pucManaMsg, int iIfOpen);
int	STDCALL SDT_ReadBaseMsg(int iPort, unsigned char *pucCHMsg, unsigned int *puiCHMsgLen, unsigned char *pucPHMsg, unsigned int *puiPHMsgLen, int iIfOpen);
int	STDCALL SDT_ReadBaseMsgToFile(int iPort, char *pcCHMsgFileName, unsigned int *puiCHMsgFileLen, char *pcPHMsgFileName, unsigned int *puiPHMsgFileLen, int iIfOpen);
int	STDCALL SDT_ReadBaseFPMsg(int iPort, unsigned char *pucCHMsg, unsigned int *puiCHMsgLen, unsigned char *pucPHMsg, unsigned int *puiPHMsgLen, unsigned char *pucFPMsg, unsigned int *puiFPMsgLen, int iIfOpen);
int	STDCALL SDT_ReadBaseFPMsgToFile(int iPort, char *pcCHMsgFileName, unsigned int *puiCHMsgFileLen, char *pcPHMsgFileName, unsigned int *puiPHMsgFileLen, char *pcFPMsgFileName, unsigned int *puiFPMsgFileLen, int iIfOpen);
int	STDCALL SDT_ReadNewAppMsg(int iPort, unsigned char *pucAppMsg, unsigned int *puiAppMsgLen, int iIfOpen);

#ifdef __cplusplus
}
#endif 
#endif