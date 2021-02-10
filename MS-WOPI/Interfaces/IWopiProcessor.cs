using System;
using System.Collections.Generic;
using System.Text;
using MS_WOPI.Request;

namespace MS_WOPI.Interfaces
{
    public interface IWopiProcessor
    {
        void HandleCheckFileInfoRequest(WopiRequest requestData);
        void HandleGetFileRequest(WopiRequest requestData);
        void HandlePutFileRequest(WopiRequest requestData);
        void HandleLockRequest(WopiRequest requestData);
        void HandleRefreshLockRequest(WopiRequest requestData);
        void HandleUnlockRequest(WopiRequest requestData);
        void HandleUnlockAndRelockRequest(WopiRequest requestData);
        void HandlePutRelativeFileRequest(WopiRequest requestData);
    }
}
