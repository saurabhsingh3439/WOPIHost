using System;
using System.Collections.Generic;
using System.Text;

namespace MS_WOPI.Common
{
    public class LockInfo
    {
        public string Lock { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Expired { get { return this.DateCreated.AddMinutes(30) < DateTime.UtcNow; } }

        public static readonly Dictionary<string, LockInfo> Locks = new Dictionary<string, LockInfo>();
        public static bool TryGetLock(string fileId, out LockInfo lockInfo)
        {
            // TODO: This lock implementation is not thread safe and not persisted and all in all just an example.
            if (Locks.TryGetValue(fileId, out lockInfo))
            {
                if (lockInfo.Expired)
                {
                    Locks.Remove(fileId);
                    return false;
                }
                return true;
            }

            return false;
        }
    }
}
