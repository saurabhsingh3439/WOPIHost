using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MS_WOPI.Response
{
    class CheckFileInfoResponse
    {
        public string BaseFileName { get; set; }
        public string OwnerId { get; set; }
        public int Size { get; set; }
        public string UserId { get; set; }
        public string Version { get; set; }

        public string BreadcrumbBrandName { get; set; }
        public string BreadcrumbBrandUrl { get; set; }
        public string BreadcrumbFolderName { get; set; }
        public string BreadcrumbFolderUrl { get; set; }
        public string BreadcrumbDocName { get; set; }

        public bool UserCanWrite { get; set; }
        public bool ReadOnly { get; set; }
        public bool SupportsLocks { get; set; }
        public bool SupportsUpdate { get; set; }
        public bool UserCanNotWriteRelative { get; set; }

        public string UserFriendlyName { get; set; }
    }

    [DataContract]
    class PutRelativeFileResponse
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string HostViewUrl { get; set; }
        [DataMember]
        public string HostEditUrl { get; set; }
    }

    public class WopiResponse
    {
        public int status;

        public string message;
    }
}
