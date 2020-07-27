using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Business.Helper
{

   public static class UserHelper
   {

      public static List<UserInfo> GetAvailableUser(APDBDef db)
      {
         APDBDef.UserInfoTableDef u = APDBDef.UserInfo;

         return db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
      }

   }

}
