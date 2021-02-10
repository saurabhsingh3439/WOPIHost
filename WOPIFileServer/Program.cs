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
