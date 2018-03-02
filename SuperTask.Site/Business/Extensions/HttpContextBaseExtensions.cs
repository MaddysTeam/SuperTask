using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace System.Web.Mvc
{

   public static class HttpContextBaseExtensions
   {

      public static T GetFromCookie<T>(this HttpContextBase ctx)
      {
         var t = default(T);
         var authCookie = ctx.Request.Cookies[FormsAuthentication.FormsCookieName];//获取cookie
         if (authCookie != null)
         {
            var Ticket = FormsAuthentication.Decrypt(authCookie.Value);//解密
            t = JsonConvert.DeserializeObject<T>(Ticket.UserData);
         }

         return t;
      }


      public static T GetFromCookie<T>(this HttpContext ctx)
      {
         var t = default(T);
         var authCookie = ctx.Request.Cookies[FormsAuthentication.FormsCookieName];//获取cookie
         if (authCookie != null)
         {
            var Ticket = FormsAuthentication.Decrypt(authCookie.Value);//解密
            t = JsonConvert.DeserializeObject<T>(Ticket.UserData);
         }

         return t;
      }


      public static void SetValueToCookie<T>(this HttpContext ctx,T t,string name)
      {
         var UserData = JsonConvert.SerializeObject(t);
         var ticket = new FormsAuthenticationTicket(1, name, DateTime.Now, DateTime.Now.AddHours(10), false, UserData);
         var Cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
         HttpContext.Current.Response.Cookies.Add(Cookie);
      }

   }

}