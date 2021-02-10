using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using MS_WOPI.Handlers;
using Authorization = MS_WOPI.Handlers.Authorization;

namespace MS_WOPI
{
    public class WopiHost
    {
        private HttpListener m_listener;
        private string m_docsPath;
        private int m_port;
        private WopiHandler _handler;

        private const int MaximumRequestCount = 50; //to be read from config file

        //this class will be called from the application
        public WopiHost(string docsPath, int port = 8080)
        {
            m_docsPath = docsPath;
            m_port = port;
            //Start();
        }

        public void Start()
        {
            m_listener = new HttpListener();
            _handler = new WopiHandler(m_docsPath);
            // localhost may change to the real hostname or IP
            m_listener.Prefixes.Add(String.Format("http://localhost:{0}/wopi/", m_port));
            m_listener.Start();
            for (int i = 0; i < MaximumRequestCount; i++)
            {
                m_listener.BeginGetContext(new AsyncCallback(_handler.ProcessRequest), m_listener);
            }
            Console.WriteLine(@"WopiServer Started");
        }

        public void Stop()
        {
            m_listener.Stop();
        }
    }
}
