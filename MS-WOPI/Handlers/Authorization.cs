using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Request;
using MS_WOPI.Interfaces;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using MS_WOPI.Common;

namespace MS_WOPI.Handlers
{
   public class Authorization : IAuthorization
   {
      string BaseURL = "https://wopi-app-server.contoso.com/wv/wordviewerframe.aspx";
      private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
      private SymmetricSecurityKey _key = null;

      public bool ValidateWopiProofKey(HttpListenerRequest request)
      {
         return true;
      }

      public bool ValidateAccess(WopiRequest requestData, bool writeAccessRequired)
      {
         return !String.IsNullOrWhiteSpace(requestData.AccessToken) && (requestData.AccessToken != "INVALID");
      }

      private SymmetricSecurityKey Key
      {
         get
         {
            if (_key is null)
            {
               var key = Encoding.ASCII.GetBytes("secretKeysecretKeysecretKey123" + new Random(DateTime.Now.Millisecond).Next(1,999));
               _key = new SymmetricSecurityKey(key);
            }

            return _key;
         }
      }
      public SecurityToken GenerateAccessToken(string userId, string resourceId)
      {
         var tokenDescriptor = new SecurityTokenDescriptor
         {
            Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, userId),
                        new Claim("docid", resourceId)
                }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)
         };

         return _tokenHandler.CreateToken(tokenDescriptor);
      }
 
      public string GetWopiUrl(string wopiSource,string accessToken = null)
      {
         accessToken = Uri.EscapeDataString(accessToken);
         return $"{BaseURL}?WOPISrc={wopiSource}&access_token={accessToken}";
      }
  
      public bool ValidateToken(string tokenString, string userId, string docId)
      {
         // Initialize the token handler and validation parameters
         var tokenHandler = new JwtSecurityTokenHandler();
         var tokenValidation = new TokenValidationParameters
         {
            ValidAudience = "https://officewopi.azurewebsites.net",
            ValidIssuer = "https://officewopi.azurewebsites.net",
            IssuerSigningKey = Key
         };

         try
         {
            SecurityToken token = null;
            var principal = tokenHandler.ValidateToken(tokenString, tokenValidation, out token);
            return principal.HasClaim(ClaimTypes.Name, userId) && principal.HasClaim("docid", docId);
         }
         catch (Exception)
         {
            return false;
         }
      }
   }
}
