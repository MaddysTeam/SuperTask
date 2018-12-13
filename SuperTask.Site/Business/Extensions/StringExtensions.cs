using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{

   public static class StringExtensions
   {

      public static Guid ToGuid(this string str,Guid defaultValue)
      {
         var guid = Guid.Empty;
         var isSuccess=Guid.TryParse(str, out guid);

         if (guid == Guid.Empty | !isSuccess)
         {
            guid = defaultValue;
         }

         return guid;
      }


      public static string Ellipsis(this string str,int length=20)
      {
         if (string.IsNullOrEmpty(str))
            return str;

         length = str.Length < length ? str.Length : length;

         return str.Substring(0, length) + "...";
      }

      public static bool InlcudeAny<T>(this string str,T[] includes,char split=',')
      {
         if (str.IsNullOrEmpty()) return false;

         foreach(var item in includes)
         {
            if (string.Equals(item.ToString(), str, StringComparison.InvariantCultureIgnoreCase)
                  || str.Split(split).Contains(item.ToString(),StringComparison.InvariantCultureIgnoreCase))
               return true;
         }

         return false;
      }

   }

}
