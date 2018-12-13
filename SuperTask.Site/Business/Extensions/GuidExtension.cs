using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business
{
   public static class GuidExtension
   {

      public static bool IsEmpty(this Guid gid)
      {
         return gid == null || gid == Guid.Empty;
      }

   }
}