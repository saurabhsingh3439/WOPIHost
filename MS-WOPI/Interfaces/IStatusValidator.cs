using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Response;
using System.Net;

namespace MS_WOPI.Interfaces
{
    public interface IStatusValidator
    {
        void ReturnStatus(HttpListenerResponse response, int code, string description);
    }
}
