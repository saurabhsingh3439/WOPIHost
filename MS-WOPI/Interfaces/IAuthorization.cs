using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using MS_WOPI.Request;

namespace MS_WOPI.Interfaces
{
    public interface IAuthorization
    {
        bool ValidateWopiProofKey(HttpListenerRequest request);
        bool ValidateAccess(WopiRequest requestData, bool writeAccessRequired);
    }
}
