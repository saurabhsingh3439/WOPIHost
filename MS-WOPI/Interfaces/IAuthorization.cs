using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using MS_WOPI.Request;
using Microsoft.IdentityModel.Tokens;

namespace MS_WOPI.Interfaces
{
    public interface IAuthorization
    {
        bool ValidateWopiProofKey(HttpListenerRequest request);
        bool ValidateAccess(WopiRequest requestData, bool writeAccessRequired);
        SecurityToken GenerateAccessToken(string UserID, string DocID);
        string GetWopiUrl(string wopiSource, string accessToken = null);
        bool ValidateToken(string tokenString, string userId, string docId);
    }
}
