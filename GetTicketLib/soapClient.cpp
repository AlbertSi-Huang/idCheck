/* soapClient.cpp
   Generated by gSOAP 2.8.49 for .\natappfree.h

gSOAP XML Web services tools
Copyright (C) 2000-2017, Robert van Engelen, Genivia Inc. All Rights Reserved.
The soapcpp2 tool and its generated software are released under the GPL.
This program is released under the GPL with the additional exemption that
compiling, linking, and/or using OpenSSL is allowed.
--------------------------------------------------------------------------------
A commercial use license is available from Genivia Inc., contact@genivia.com
--------------------------------------------------------------------------------
*/

#if defined(__BORLANDC__)
#pragma option push -w-8060
#pragma option push -w-8004
#endif
#include "soapH.h"

SOAP_SOURCE_STAMP("@(#) soapClient.cpp ver 2.8.49 2019-06-11 05:01:41 GMT")


SOAP_FMAC5 int SOAP_FMAC6 soap_call_ns1__getTicketInfo(struct soap *soap, const char *soap_endpoint, const char *soap_action, const std::string& arg0, struct ns1__getTicketInfoResponse &_param_1)
{	struct ns1__getTicketInfo soap_tmp_ns1__getTicketInfo;
	if (soap_endpoint == NULL)
		soap_endpoint = "http://rrtt5s.natappfree.cc/klmy";
	if (soap_action == NULL)
		soap_action = "";
	soap_tmp_ns1__getTicketInfo.arg0 = arg0;
	soap_begin(soap);
	soap->encodingStyle = NULL;
	soap_serializeheader(soap);
	soap_serialize_ns1__getTicketInfo(soap, &soap_tmp_ns1__getTicketInfo);
	if (soap_begin_count(soap))
		return soap->error;
	if (soap->mode & SOAP_IO_LENGTH)
	{	if (soap_envelope_begin_out(soap)
		 || soap_putheader(soap)
		 || soap_body_begin_out(soap)
		 || soap_put_ns1__getTicketInfo(soap, &soap_tmp_ns1__getTicketInfo, "ns1:getTicketInfo", "")
		 || soap_body_end_out(soap)
		 || soap_envelope_end_out(soap))
			 return soap->error;
	}
	if (soap_end_count(soap))
		return soap->error;
	if (soap_connect(soap, soap_endpoint, soap_action)
	 || soap_envelope_begin_out(soap)
	 || soap_putheader(soap)
	 || soap_body_begin_out(soap)
	 || soap_put_ns1__getTicketInfo(soap, &soap_tmp_ns1__getTicketInfo, "ns1:getTicketInfo", "")
	 || soap_body_end_out(soap)
	 || soap_envelope_end_out(soap)
	 || soap_end_send(soap))
		return soap_closesock(soap);
	if (!static_cast<struct ns1__getTicketInfoResponse*>(&_param_1)) // NULL ref?
		return soap_closesock(soap);
	soap_default_ns1__getTicketInfoResponse(soap, &_param_1);
	if (soap_begin_recv(soap)
	 || soap_envelope_begin_in(soap)
	 || soap_recv_header(soap)
	 || soap_body_begin_in(soap))
		return soap_closesock(soap);
	soap_get_ns1__getTicketInfoResponse(soap, &_param_1, "ns1:getTicketInfoResponse", NULL);
	if (soap->error)
		return soap_recv_fault(soap, 0);
	if (soap_body_end_in(soap)
	 || soap_envelope_end_in(soap)
	 || soap_end_recv(soap))
		return soap_closesock(soap);
	return soap_closesock(soap);
}

#if defined(__BORLANDC__)
#pragma option pop
#pragma option pop
#endif

/* End of soapClient.cpp */
