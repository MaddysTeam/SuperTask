using Business;
using Business.Config;
using System;
using System.Web.Mvc;

namespace TheSite.Controllers
{

   [Auth]
   [Security]
   public class BaseController : Controller
   {

      #region [ DB ]


      private APDBDef _db;


      public APDBDef db
      {
         get
         {
            if (_db == null)
               _db = new APDBDef();
            return _db;
         }
         private set
         {
            _db = value;
         }
      }


      #endregion


      #region [ Ajax ]


      protected void ThrowNotAjax()
      {
         if (!Request.IsAjaxRequest())
            throw new NotSupportedException("Action must be Ajax call.");
      }


      #endregion


      public UserInfo GetUserInfo()
      {
         var userInfo = Session[ThisApp.UserInfo] as UserInfo;

         if (userInfo == null)
         {
            userInfo = HttpContext.GetFromCookie<UserInfo>();

            if (userInfo == null)
               RedirectToAction("Login", "Account");
         }

         return userInfo;
      }


      public bool HasInRole(string roleName)
      {
         return GetUserInfo().Roles.Exists(x =>
                  string.Equals(roleName, x.RoleName, StringComparison.InvariantCultureIgnoreCase));
      }

   }

}