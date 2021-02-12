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
using System.Text;
using MS_WOPI.Response;
using MS_WOPI.Interfaces;
using MS_WOPI.Headers;
using System.Net;
using MS_WOPI.Common;

namespace MS_WOPI.Handlers
{
    public class ErrorHandler : IErrorHandler
    {
        private IStatusValidator _statusValidator;

        public ErrorHandler()
        {
            _statusValidator = new StatusValidator();
        }
        public void ReturnSuccess(HttpListenerResponse response)
        {
            _statusValidator.ReturnStatus(response, 200, "Success");
        }

        public void ReturnInvalidToken(HttpListenerResponse response)
        {
            _statusValidator.ReturnStatus(response, 401, "Invalid Token");
        }

        public void ReturnBadRequest(HttpListenerResponse response)
        {
            _statusValidator.ReturnStatus(response, 400, "Bad Request");
        }

        public void ReturnFileUnknown(HttpListenerResponse response)
        {
            _statusValidator.ReturnStatus(response, 404, "File Unknown/User Unauthorized");
        }

        public void ReturnLockMismatch(HttpListenerResponse response, string existingLock = null, string reason = null)
        {
            response.Headers[WopiHeaders.Lock] = existingLock ?? String.Empty;
            if (!String.IsNullOrEmpty(reason))
            {
                response.Headers[WopiHeaders.LockFailureReason] = reason;
                _statusValidator.ReturnStatus(response, 409, reason);
                return;
            }

            _statusValidator.ReturnStatus(response, 409, "Lock mismatch/Locked by another interface");
        }

        public void ReturnConflict(HttpListenerResponse response, string reason = null)
        {
            _statusValidator.ReturnStatus(response, 409, "Conflict");
        }

        public void ReturnServerError(HttpListenerResponse response)
        {
            _statusValidator.ReturnStatus(response, 500, "Server Error");
        }

        public void ReturnUnsupported(HttpListenerResponse response)
        {
            _statusValidator.ReturnStatus(response, 501, "Unsupported");
        }
    }
}
