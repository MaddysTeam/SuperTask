using Business;
using Business.Config;
using Business.Helper;

namespace System.Web.Mvc
{

   public static class HtmlExtensions
   {

      public static UserInfo GetUserProfile(this HtmlHelper helper)
      {
         var userInfo=HttpContext.Current.Session["UserInfo"] as UserInfo;

         if (userInfo == null)
            userInfo = HttpContext.Current.GetFromCookie<UserInfo>();

         return userInfo;

      } 

      public static bool HasInRole(this HtmlHelper helper, string roleName)
      {
         var roles = helper
            .GetUserProfile()
            .Roles;

         return roles==null? false : roles.Exists(x =>
                  string.Equals(roleName, x.RoleName, StringComparison.InvariantCultureIgnoreCase));
      }


      public static bool HasPermission(this HtmlHelper helper, string permissionCode)
      {
         return true;
      }


      public static bool HasPermission(this HtmlHelper helper, string projectId,string permissionCode)
      {
         var userId = helper.GetUserProfile().UserId;

         var result=ResourceHelper.HasPermission(userId, projectId.ToGuid(Guid.Empty), permissionCode, new APDBDef());

         return result;
      }

   }

}