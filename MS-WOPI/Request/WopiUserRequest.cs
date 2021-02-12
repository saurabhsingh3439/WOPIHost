using System;
using System.Collections.Generic;
using System.Text;

namespace MS_WOPI.Request
{
   public class WopiUserRequest
   {
      public ActionType Action;
      public string userId;
      public string resourceId;
      public string docsPath;

   }
   public enum ActionType
   {
      VIEW = 1,
      EDIT = 2,
      DELETE = 3,
      SAVE = 4
   }
}
