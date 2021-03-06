/* soapCheckTicketWSPortBindingProxy.h
   Generated by gSOAP 2.8.49 for ./natappfree.h

gSOAP XML Web services tools
Copyright (C) 2000-2017, Robert van Engelen, Genivia Inc. All Rights Reserved.
The soapcpp2 tool and its generated software are released under the GPL.
This program is released under the GPL with the additional exemption that
compiling, linking, and/or using OpenSSL is allowed.
--------------------------------------------------------------------------------
A commercial use license is available from Genivia Inc., contact@genivia.com
--------------------------------------------------------------------------------
*/

#ifndef soapCheckTicketWSPortBindingProxy_H
#define soapCheckTicketWSPortBindingProxy_H
#include "soapH.h"

    class SOAP_CMAC CheckTicketWSPortBindingProxy : public soap {
      public:
        /// Endpoint URL of service 'CheckTicketWSPortBindingProxy' (change as needed)
        const char *soap_endpoint;
        /// Variables globally declared in ./natappfree.h, if any
        /// Construct a proxy with new managing context
        CheckTicketWSPortBindingProxy();
        /// Copy constructor
        CheckTicketWSPortBindingProxy(const CheckTicketWSPortBindingProxy& rhs);
        /// Construct proxy given a managing context
        CheckTicketWSPortBindingProxy(const struct soap&);
        /// Constructor taking an endpoint URL
        CheckTicketWSPortBindingProxy(const char *endpoint);
        /// Constructor taking input and output mode flags for the new managing context
        CheckTicketWSPortBindingProxy(soap_mode iomode);
        /// Constructor taking endpoint URL and input and output mode flags for the new managing context
        CheckTicketWSPortBindingProxy(const char *endpoint, soap_mode iomode);
        /// Constructor taking input and output mode flags for the new managing context
        CheckTicketWSPortBindingProxy(soap_mode imode, soap_mode omode);
        /// Destructor deletes deserialized data and managing context
        virtual ~CheckTicketWSPortBindingProxy();
        /// Initializer used by constructors
        virtual void CheckTicketWSPortBindingProxy_init(soap_mode imode, soap_mode omode);
        /// Return a copy that has a new managing context with the same engine state
        virtual CheckTicketWSPortBindingProxy *copy() SOAP_PURE_VIRTUAL;
        /// Copy assignment
        CheckTicketWSPortBindingProxy& operator=(const CheckTicketWSPortBindingProxy&);
        /// Delete all deserialized data (uses soap_destroy() and soap_end())
        virtual void destroy();
        /// Delete all deserialized data and reset to default
        virtual void reset();
        /// Disables and removes SOAP Header from message by setting soap->header = NULL
        virtual void soap_noheader();
        /// Get SOAP Header structure (i.e. soap->header, which is NULL when absent)
        virtual ::SOAP_ENV__Header *soap_header();
        /// Get SOAP Fault structure (i.e. soap->fault, which is NULL when absent)
        virtual ::SOAP_ENV__Fault *soap_fault();
        /// Get SOAP Fault string (NULL when absent)
        virtual const char *soap_fault_string();
        /// Get SOAP Fault detail as string (NULL when absent)
        virtual const char *soap_fault_detail();
        /// Close connection (normally automatic, except for send_X ops)
        virtual int soap_close_socket();
        /// Force close connection (can kill a thread blocked on IO)
        virtual int soap_force_close_socket();
        /// Print fault
        virtual void soap_print_fault(FILE*);
    #ifndef WITH_LEAN
    #ifndef WITH_COMPAT
        /// Print fault to stream
        virtual void soap_stream_fault(std::ostream&);
    #endif
        /// Write fault to buffer
        virtual char *soap_sprint_fault(char *buf, size_t len);
    #endif
        /// Web service operation 'getTicketInfo' (returns SOAP_OK or error code)
        virtual int getTicketInfo(const std::string& arg0, struct ns1__getTicketInfoResponse &_param_1)
        { return this->getTicketInfo(NULL, NULL, arg0, _param_1); }
        virtual int getTicketInfo(const char *soap_endpoint, const char *soap_action, const std::string& arg0, struct ns1__getTicketInfoResponse &_param_1);
    };
#endif
