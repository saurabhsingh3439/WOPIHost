using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Response;
using MS_WOPI.Interfaces;
using System.Net;
using System.IO;

namespace MS_WOPI.Common
{
    public class StatusValidator : IStatusValidator
    {
        public void ReturnStatus(HttpListenerResponse response, int code, string description)
        {
            response.StatusCode = code;
            response.StatusDescription = description;
        }
    }
}
