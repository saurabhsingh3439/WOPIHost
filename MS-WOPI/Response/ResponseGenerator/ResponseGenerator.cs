using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MS_WOPI.Response.ResponseGenerator
{
    public class ResponseGenerator
    {
        private FileInfo _info;
        public ResponseGenerator(FileInfo info)
        {
            _info = info;
        }
        public WopiCheckFileInfo GetFileInfoResponse()
        {
            WopiCheckFileInfo cfi = new WopiCheckFileInfo();

            cfi.BaseFileName = _info.Name;
            cfi.OwnerId = "";
            cfi.UserFriendlyName = "";

            lock (_info)
            {
                if (_info.Exists)
                {
                    cfi.Size = _info.Length;
                }
                else
                {
                    cfi.Size = 0;
                }
            }

            cfi.Version = DateTime.Now.ToString("s");
            cfi.SupportsCoauth = false;
            cfi.SupportsCobalt = false;
            cfi.SupportsFolders = true;
            cfi.SupportsLocks = true;
            cfi.SupportsScenarioLinks = false;
            cfi.SupportsSecureStore = false;
            cfi.SupportsUpdate = true;
            cfi.UserCanWrite = true;

            return cfi;
        }

        public byte[] GetFileContent()
        {
            MemoryStream ms = new MemoryStream();
            lock (_info)
            {
                using (FileStream fileStream = _info.OpenRead())
                {
                    fileStream.CopyTo(ms);
                }
            }
            return ms.ToArray();
        }

        public void Save(byte[] new_content)
        {
            lock (_info)
            {
                using (FileStream fileStream = _info.Open(FileMode.Truncate))
                {
                    fileStream.Write(new_content, 0, new_content.Length);
                }
            }
        }
    }
}
