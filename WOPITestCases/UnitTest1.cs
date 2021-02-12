/*
Copyright Mitratech Holdings Inc, 2021
This software is provided under the terms of a License Agreement and may
only be used and/or copied in accordance with the terms of such agreement.
Neither this software nor any copy thereof may be provided or otherwise
made available to any other person. No title or ownership of this software
is hereby transferred.
*/

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