using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Response;
using System.Net;

namespace MS_WOPI.Interfaces
{
    public interface IErrorHandler
    {
        void ReturnSuccess(HttpListenerResponse response);
        void ReturnInvalidToken(HttpListenerResponse response);
        void ReturnFileUnknown(HttpListenerResponse response);
        void ReturnLockMismatch(HttpListenerResponse response, string existingLock = null, string reason = null);
        void ReturnServerError(HttpListenerResponse response);
        void ReturnUnsupported(HttpListenerResponse response);
        void ReturnBadRequest(HttpListenerResponse response);
        void ReturnConflict(HttpListenerResponse response, string reason = null);
    }
}
