using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Business
{

   public static class DictionaryExtension
   {

      public static List<SelectListItem> ToSelectList(this List<Dictionary> dictionary)
      {
         return new List<SelectListItem>();
      }

   }

}
