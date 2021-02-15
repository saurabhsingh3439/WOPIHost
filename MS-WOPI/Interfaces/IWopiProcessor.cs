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
        
        void GetFileLockId(WopiRequest requestData);

    }
}
