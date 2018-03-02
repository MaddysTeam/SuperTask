using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace System.Web.Mvc
{

   public class ExceptionAttribute: HandleErrorAttribute
   {

      public override void OnException(ExceptionContext filterContext)
      {
         if (!filterContext.ExceptionHandled)
         {
            var controller = filterContext.RouteData.Values["controller"];
            var action = filterContext.RouteData.Values["action"];
            string msgTemplate = "在执行 controller[{0}] 的 action[{1}] 时产生异常";
         }

         if (filterContext.Result is JsonResult)
         {
            // 当结果为json时，设置异常已处理

            filterContext.ExceptionHandled = true;
         }
         else
         {
            // 否则调用原始设置

            base.OnException(filterContext);
         }

      }

   }

}