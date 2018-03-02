using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace Business.Helper
{

   public static class DapperHelper
   {

      public static List<T> QueryBySQL<T>(string sql,object paras=null)
      {
         if (string.IsNullOrEmpty(sql)) return null;

         var _conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
         var result=_conn.Query<T>(sql, paras);

         return result==null ? null: result.ToList();
      }

   }

}
