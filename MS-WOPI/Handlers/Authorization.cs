using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Request;
using MS_WOPI.Interfaces;
using System.Net;

namespace MS_WOPI.Handlers
{
    public class Authorization : IAuthorization
    {
        public bool ValidateWopiProofKey(HttpListenerRequest request)
        {
            return true;
        }

        public bool ValidateAccess(WopiRequest requestData, bool writeAccessRequired)
        {
            return !String.IsNullOrWhiteSpace(requestData.AccessToken) && (requestData.AccessToken != "INVALID");
        }
    }
}
