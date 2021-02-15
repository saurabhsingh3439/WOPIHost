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
