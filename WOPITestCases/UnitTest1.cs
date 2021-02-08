using NUnit.Framework;
using RestSharp;
using System.Net;

namespace WOPITestCases
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test_Lock_ReturnSuccess()
        {
            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            string expectedValue = "OK";
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcdqwe");
            IRestResponse response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());

            //Assert.Pass();
        }

        [Test]
        public void Test_Lock_ReturnConflict()
        {
            var client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcdqwe");
            IRestResponse response = client.Execute(request);
            client = new RestClient("http://localhost:8080/wopi/files/CorrectPath.txt?access_token=afdasfas");
            client.Timeout = -1;
            string expectedValue = "Conflict";
            request = new RestRequest(Method.POST);
            request.AddHeader("X-WOPI-Override", "LOCK");
            request.AddHeader("X-WOPI-Lock", "abcd");
            response = client.Execute(request);
            Assert.AreEqual(expectedValue, response.StatusCode.ToString());
            
            //Assert.Pass();
        }
        
    }
}