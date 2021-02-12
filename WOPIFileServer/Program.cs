/*
Copyright Mitratech Holdings Inc, 2021
This software is provided under the terms of a License Agreement and may
only be used and/or copied in accordance with the terms of such agreement.
Neither this software nor any copy thereof may be provided or otherwise
made available to any other person. No title or ownership of this software
is hereby transferred.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS_WOPI;
using MS_WOPI.Request;

namespace WOPIFileServer
{
    class Program
    {
        static void Main(string[] args)
        {
            WopiUserRequest request = new WopiUserRequest()
            {
                userId = "user@policyhub",
                resourceId = "file1.txt",
                Action = ActionType.VIEW,
                docsPath = @"C:\\wopi-docs"
            };
            WopiHost host = new WopiHost(request);
            host.Start();
            Console.WriteLine("A simple wopi webserver. Press any key to quit.");
            Console.ReadKey();
            host.Stop();
        }
    }
}
