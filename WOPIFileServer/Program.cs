using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS_WOPI;

namespace WOPIFileServer
{
    class Program
    {
        static void Main(string[] args)
        {
            WopiHost host = new WopiHost(@"C:\\wopi-docs");
            host.Start();
            Console.WriteLine("A simple wopi webserver. Press any key to quit.");
            Console.ReadKey();
            host.Stop();
        }
    }
}
