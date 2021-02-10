using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MS_WOPI.Interfaces;
using NUnit.Framework;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace NUnitTestWOPI
{
    public class Tests
    {
        private IAuthorization _authorization;
        //SecurityToken securityToken = null;
        public Tests()
        {
            var services = new ServiceCollection();
            services.AddTransient<IAuthorization, MS_WOPI.Handlers.Authorization>();

            var serviceProvider = services.BuildServiceProvider();

            _authorization = serviceProvider.GetService<IAuthorization>();
        }
        [SetUp]
        public void Setup()
        {

        }

        #region Lock
        [Test]
        public void Test_Lock_ReturnSuccess()
        {
            string expectedValue = "OK";
            SecurityToken accessToken = _authorization.GenerateAccessToken("user@policyhub", "CorrectPath.txt");
            var strToken = ((JwtSecurityToken)accessToken).RawData.ToString();
            var url = "http://localhost:8080/wopi/files/CorrectPath.txt?access_token=";
            var postUrl = url + strToken;
            var client = new RestClient(postUrl);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            //request.AddParameter("access_token", accessToken);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_Lock_ReturnConflict()
        {
            string expectedValue = "Conflict";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcde");
            IRestResponse response = client.Execute(request);

            client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_Lock_FileUnKnown()
        {
            var client = new RestClient("http://localhost:8080/wopi/files/InCorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            string expectedValue = "NotFound";
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            var response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

            //Assert.Pass();
        }

        [Test]
        public void Test_Lock_ReturnUnAuthorized()
        {
            string expectedValue = "Unauthorized";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        #endregion

        #region UNLOCK

        [Test]
        public void Test_UnLock_ReturnSuccess()
        {
            //passing the same value of X-WOPI-Lock as passed by last LOCK method
            string expectedValue = "OK";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "UNLOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_UnLock_MisMatch()
        {
            //passing the same value of X-WOPI-Lock as passed by last LOCK method
            string expectedValue = "Conflict";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "UNLOCK");
            request.AddHeader("X-WOPI-Lock", "abcdef");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_UnLock_FileUnKnown()
        {
            var client = new RestClient("http://localhost:8080/wopi/files/InCorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            string expectedValue = "NotFound";
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "UNLOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            var response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

            //Assert.Pass();
        }

        [Test]
        public void Test_UnLock_ReturnUnAuthorized()
        {
            //passing the same value of X-WOPI-Lock as passed by last LOCK method
            string expectedValue = "Unauthorized";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "UNLOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        #endregion

        #region Refresh Lock

        [Test]
        public void Test_RefreshLock_ReturnSuccess()
        {
            string expectedValue = "OK";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "REFRESH_LOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_RefreshLock_ReturnConflict()
        {
            string expectedValue = "Conflict";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "REFRESH_LOCK");
            request.AddHeader("X-WOPI-Lock", "abcdef");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_RefreshLock_FileUnKnown()
        {
            var client = new RestClient("http://localhost:8080/wopi/files/InCorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            string expectedValue = "NotFound";
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "REFRESH_LOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            var response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

            //Assert.Pass();
        }

        [Test]
        public void Test_RefreshLock_ReturnUnAuthoized()
        {
            string expectedValue = "Unauthorized";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "REFRESH_LOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }


        #endregion

        #region CheckFileInfo

        [Test]
        public void Test_CheckFileInfo_ReturnSuccess()
        {
            string expectedValue = "OK";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-WOPI-SessionContext", "afssgdsgdgdsgsdg");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_CheckFileInfo_WithoutToken_ReturnUnAuthorized()
        {
            string expectedValue = "Unauthorized";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-WOPI-SessionContext", "afssgdsgdgdsgsdg");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_CheckFileInfo_FileUnKnown()
        {
            var client = new RestClient("http://localhost:8080/wopi/files/InCorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            string expectedValue = "NotFound";
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-WOPI-SessionContext", "afssgdsgdgdsgsdg");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

            //Assert.Pass();
        }

        #endregion

        #region GetFile

        [Test]
        public void Test_GetFileInfo_ReturnSuccess()
        {
            string expectedValue = "OK";
            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-WOPI-MaxExpectedSize", "11");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());
        }

        [Test]
        public void Test_GetFileInfo_ReturnUnAuthorized()
        {
            string expectedValue = "Unauthorized";
            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-WOPI-MaxExpectedSize", "11");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());
        }

        [Test]
        public void Test_GetFileInfo_FileUnKnown()
        {
            var client = new RestClient("http://localhost:8080/wopi/files/InCorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            string expectedValue = "NotFound";
            var request = new RestRequest(Method.GET);
            request.AddHeader("X-WOPI-MaxExpectedSize", "11");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

            //Assert.Pass();
        }
        #endregion

        #region Unlock and Relock

        [Test]
        public void Test_UnLockReLock_ReturnSuccess()
        {
            string expectedValue = "OK";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcde");
            request.AddHeader("X-WOPI-OldLock", "abcd");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_UnLockReLock_ReturnConflict()
        {
            string expectedValue = "Conflict";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-OldLock", "abcd");
            request.AddHeader("X-WOPI-Lock", "abcde");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        [Test]
        public void Test_UnLockReLock_FileUnKnown()
        {
            var client = new RestClient("http://localhost:8080/wopi/files/InCorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            string expectedValue = "NotFound";
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            request.AddHeader("X-WOPI-OldLock", "abcde");
            var response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

            //Assert.Pass();
        }

        [Test]
        public void Test_UnLockReLock_ReturnUnAuthorized()
        {
            string expectedValue = "Unauthorized";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcde");
            request.AddHeader("X-WOPI-OldLock", "abcd");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        #endregion

        #region PUT FILE 

        [Test]
        public void Test_PUTFile_ReturnSuccess()
        {
            string expectedValue = "OK";

            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "PUT");
            request.AddHeader("X-WOPI-Lock", "abcd");
            request.AddHeader("X-WOPI-Editors", "Test");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

        }

        #endregion


    }
}