using Business;
using Business.Config;
using Business.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace System.Web.Mvc
{

   public class AuthAttribute : AuthorizeAttribute
   {

      protected override bool AuthorizeCore(HttpContextBase httpContext)
      {
         var userInfo = httpContext.Session[ThisApp.UserInfo] as UserInfo;

         if (userInfo == null)
         {
            userInfo = httpContext.GetFromCookie<UserInfo>();
         }


         if (userInfo == null || !(userInfo is UserInfo))
         {
            httpContext.Response.StatusCode = 403;

            return false;
         }

         return true;
      }

      public override void OnAuthorization(AuthorizationContext filterContext)
      {
         base.OnAuthorization(filterContext);

         if (filterContext.HttpContext.Response.StatusCode == 403)
         {
            filterContext.Result = new RedirectResult("/account/login");
         }
      }

   }

}