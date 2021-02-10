using MS_WOPI.Handlers;
using System;
using System.IO;

namespace MS_WOPI.Request
{
    public enum RequestType
    {
        None,

        CheckFileInfo,
        PutRelativeFile,

        Lock,
        Unlock,
        RefreshLock,
        UnlockAndRelock,

        ExecuteCobaltRequest,

        DeleteFile,
        ReadSecureStore,
        GetRestrictedLink,
        RevokeRestrictedLink,

        CheckFolderInfo,

        GetFile,
        PutFile,
        GetLock,

        EnumerateChildren,
    }

    static class WopiHeaders
    {
        public const string RequestType = "X-WOPI-Override";
        public const string ItemVersion = "X-WOPI-ItemVersion";

        public const string Lock = "X-WOPI-Lock";
        public const string OldLock = "X-WOPI-OldLock";
        public const string LockFailureReason = "X-WOPI-LockFailureReason";
        public const string LockedByOtherInterface = "X-WOPI-LockedByOtherInterface";

        public const string SuggestedTarget = "X-WOPI-SuggestedTarget";
        public const string RelativeTarget = "X-WOPI-RelativeTarget";
        public const string OverwriteRelativeTarget = "X-WOPI-OverwriteRelativeTarget";
        public const string ValidRelativeTarget = "X-WOPI-ValidRelativeTarget";
    }

    public class WopiRequest
    {
        public RequestType Type { get; set; }

        public string AccessToken { get; set; }

        public string Id { get; set; }

        public string FullPath
        {
            get { return Path.Combine(WopiHandler.LocalStoragePath, Id); }
        }

        public byte[] FileData { get; set; }

        public string LockId { get; set; }
        public string OldLockId { get; set; }
        public string RelativeTarget { get; set; }
        public string SuggestedTarget { get; set; }
        public bool OverwriteTarget { get; set; }
        

    }
    }
