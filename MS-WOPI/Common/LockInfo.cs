using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MS_WOPI.Common
{
    public class LockInfo
    {
        private static string jsonPath = @"C:\WOPIHost\MS-WOPI\Common\WOPI_Locks.json";
        //Set New Lock
        public static bool SetLock(string fileId, string lockval)
        {
            var prvList = new List<JsonLock>();
            GetprvData(ref prvList);
            prvList.Add(new JsonLock()
            {
                Lockid = fileId,
                LockVal = lockval,
                DateCreated = DateTime.UtcNow
            });
            using (StreamWriter file = File.CreateText(jsonPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, prvList);
            }
            return true;
        }
        //Get Prv Lock for fileID
        public static bool TryGetLock(string fileId, out string lockval)
        {
            lockval = "";
            var prvList = new List<JsonLock>();
            if (GetprvData(ref prvList))
            {
                if (prvList.Exists(x => x.Lockid == fileId))
                {
                    lockval = prvList.Find(x => x.Lockid.Equals(fileId)).LockVal;
                    if (prvList.Find(x => x.Lockid.Equals(fileId)).Expired)
                    {
                        var itemToRemove = prvList.SingleOrDefault(x => x.Lockid == fileId);
                        if (itemToRemove != null)
                            prvList.Remove(itemToRemove);
                        using (StreamWriter file = File.CreateText(jsonPath))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(file, prvList);
                        }
                        return false;
                    }
                    return true;
                }
            }
            return false; 
        }
        //Remove Lock for FileID
        public static bool Remove(string fileId)
        {
            var prvList = new List<JsonLock>();
            if (GetprvData(ref prvList))
            {
                if (prvList.Exists(x => x.Lockid == fileId))
                {
                    var itemToRemove = prvList.SingleOrDefault(x => x.Lockid == fileId);
                    if (itemToRemove != null)
                        prvList.Remove(itemToRemove);
                    using (StreamWriter file = File.CreateText(jsonPath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, prvList);
                    }
                    return true;
                }
            }
            return false;
        }
        //Refresh Lock for Fileid
        public static bool Refresh(string fileId)
        {
            var prvList = new List<JsonLock>();
            if (GetprvData(ref prvList))
            {
                if (prvList.Exists(x => x.Lockid == fileId))
                {
                    prvList.Find(x => x.Lockid.Equals(fileId)).DateCreated = DateTime.UtcNow;
                    using (StreamWriter file = File.CreateText(jsonPath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(file, prvList);
                    }
                    return true;
                }
            }
            return false;
        }
        //Get Prv Data from Json
        private static bool GetprvData(ref List<JsonLock> prvList)
        {
            var initialJson = File.ReadAllText(jsonPath);
            if (initialJson != "")
            {
                prvList = JsonConvert.DeserializeObject<List<JsonLock>>(initialJson);
                return true;
            }
            return false;
        }
    }

    public class JsonLock
    {
        public string Lockid { get; set; }

        public string LockVal { get; set; }

        public DateTime DateCreated { get; set; }
        public bool Expired { get { return this.DateCreated.AddMinutes(30) < DateTime.UtcNow; } }
    }
}
