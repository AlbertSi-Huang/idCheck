// GetTicketLib.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "GetTicketLib.h"
#include "soapCheckTicketWSPortBindingProxy.h"
#include "CheckticketWsPortBinding.nsmap"
#include <iostream>
using namespace std;




std::string UTF8_To_string(const std::string & str)
{
	int nwLen = MultiByteToWideChar(CP_UTF8, 0, str.c_str(), -1, NULL, 0);

	wchar_t * pwBuf = new wchar_t[nwLen + 1];
	memset(pwBuf, 0, nwLen * 2 + 2);

	MultiByteToWideChar(CP_UTF8, 0, str.c_str(), str.length(), pwBuf, nwLen);

	int nLen = WideCharToMultiByte(CP_ACP, 0, pwBuf, -1, NULL, NULL, NULL, NULL);

	char * pBuf = new char[nLen + 1];
	memset(pBuf, 0, nLen + 1);

	WideCharToMultiByte(CP_ACP, 0, pwBuf, nwLen, pBuf, nLen, NULL, NULL);

	std::string retStr = pBuf;

	delete[]pBuf;
	delete[]pwBuf;

	pBuf = NULL;
	pwBuf = NULL;

	return retStr;
}

GETTICKETLIB_API void GetTicketInfo(const char* url, const char* signNum, char* outInfo)
{
	if (signNum == NULL || outInfo == NULL) {
		return;
	}
	ZeroMemory(outInfo, 1024);
	string strSignNum(signNum);

	CheckTicketWSPortBindingProxy proxy(/*url, */SOAP_C_UTFSTRING);

	//CheckTicketWSPortBindingProxy proxy("");
	ns1__getTicketInfoResponse response;
	// SOAP_OK
	//int ret0 = proxy.getTicketInfo()
	int ret = proxy.getTicketInfo(url,NULL,strSignNum, response);
	if (ret != SOAP_OK)
	{
		memcpy(outInfo, "net error", strlen("net error"));
		printf("soap error, ret=%d\n", ret);
		return ;
	}
	else
	{
		char buf[1024] = { 0 };
		memcpy(buf, (void*)response.return_.c_str(), (size_t)response.return_.length());
		string str = UTF8_To_string(string(buf));
		printf("request success, msg=%s\n", str.c_str());
		memcpy(outInfo, str.c_str(), str.length());
		return;
		//return (char*)str.c_str();
	}
}


