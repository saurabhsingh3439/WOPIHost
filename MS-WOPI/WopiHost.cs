﻿/*
Copyright Mitratech Holdings Inc, 2021
This software is provided under the terms of a License Agreement and may
only be used and/or copied in accordance with the terms of such agreement.
Neither this software nor any copy thereof may be provided or otherwise
made available to any other person. No title or ownership of this software
is hereby transferred.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using MS_WOPI.Handlers;
using MS_WOPI.Request;
using Authorization = MS_WOPI.Handlers.Authorization;

namespace MS_WOPI
{
    public class WopiHost
    {
        private const int MaximumRequestCount = 50; //to be read from config file
        private HttpListener m_listener;
        private WopiUserRequest _userRequest;
        private int m_port = 8080;
        private WopiHandler _handler;

        //this class will be called from the application
        public WopiHost(WopiUserRequest wopiUser)
        {
            _userRequest = wopiUser;          
            //Start();
        }

        public void Start()
        {
            m_listener = new HttpListener();
            _handler = new WopiHandler(_userRequest);
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
