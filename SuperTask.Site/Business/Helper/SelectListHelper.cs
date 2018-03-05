using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Business.Helper
{

   public class SelectListHelper
   {

      public static SelectListItem Default = new SelectListItem { Text="-- 请选择 --" , Value="-1" };

      public static List<SelectListItem> GetSelectItems<T>(IEnumerable<T> source,string textField, string valueField,Func<object,bool> isChecked=null, SelectListItem defaultItem=null) where T:class
      {
         var list = new List<SelectListItem>();
         if (source == null)
            return list;

         var type = typeof(T);
         var textProp = type.GetProperty(textField);
         var valueProp = type.GetProperty(valueField);

         foreach (var item in source)
         {
            var textValue = textProp.GetValue(item);
            var value = valueProp.GetValue(item);
            list.Add(new SelectListItem
            {
               Text = textValue == null ? string.Empty : textValue.ToString(),
               Value = value == null ? string.Empty : value.ToString(),
               Selected= isChecked==null?false: isChecked(value)
            });

         }

         if (defaultItem!=null)
            list.Insert(0, defaultItem);

         return list;
      }

   }

}