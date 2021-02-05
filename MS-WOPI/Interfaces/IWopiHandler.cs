using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Request;

namespace MS_WOPI.Interfaces
{
    interface IWopiHandler
    {
        void ProcessRequest(IAsyncResult request);
    }
}
