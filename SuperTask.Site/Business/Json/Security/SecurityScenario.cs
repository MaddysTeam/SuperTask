using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Business.Security
{

   public class SecurityScenario
   {

      public static class SQLInjection
      {
         public static string FilterSqlString(string param)
         {
            if (string.IsNullOrEmpty(param))
               return string.Empty;

            param = param.Trim().ToLower();
            param = param.Replace("=", "");
            param = param.Replace("'", "");
            param = param.Replace(";", "");
            param = param.Replace(" or ", "");
            param = param.Replace("paramelect", "");
            param = param.Replace("update", "");
            param = param.Replace("inparamert", "");
            param = param.Replace("delete", "");
            param = param.Replace("declare", "");
            param = param.Replace("exec", "");
            param = param.Replace("drop", "");
            param = param.Replace("create", "");
            param = param.Replace("%", "");
            param = param.Replace("--", "");

            return param;
         }

      }

      
      public  class SpecialCharChecker
      {

         public static bool HasSpecialChar(string input)
         {
            var r = new Regex("[\\*\\.\\'\\?\\+\\$\\^\\{\\}\\|\\/\\&\\【\\】]");

            return r.Matches(input).Count > 0;
         }

      }
          
   }

}
