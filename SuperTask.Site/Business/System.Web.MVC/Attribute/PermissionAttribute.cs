using Business;
using Business.Config;
using Business.Helper;

namespace System.Web.Mvc
{

   public class PermissionAttribute : AuthorizeAttribute
   {

      protected override bool AuthorizeCore(HttpContextBase httpContext)
      {
         return true;
      }


      public override void OnAuthorization(AuthorizationContext filterContext)
      {
         var auth = true;
         var userInfo = filterContext.HttpContext.Session[ThisApp.UserInfo] as UserInfo;

         if (userInfo == null)
         {
            userInfo = filterContext.HttpContext.GetFromCookie<UserInfo>();
         }

         var result = userInfo.Validate();
         if (!result.IsSuccess)
            auth = false;
         else
         {
            var actionDescriptor = filterContext.ActionDescriptor;
            var controllerDescriptor = actionDescriptor.ControllerDescriptor;
            var controller = controllerDescriptor.ControllerName;
            var action = actionDescriptor.ActionName;
            var appAddress = string.Format("/{0}/{1}", controller, action);

            if (!userInfo.HasPermission(appAddress))
            {
               auth = false;
            }
         }


         if (!auth)
         {
            throw new ApplicationException(Errors.Permission.PERMISSION_DENY);
         }
         else
            base.OnAuthorization(filterContext);
      }

   }

}