using Business.Security;

namespace System.Web.Mvc
{

   public class SecurityAttribute : FilterAttribute, IActionFilter
   {

      public void OnActionExecuted(ActionExecutedContext filterContext)
      {

      }

      public void OnActionExecuting(ActionExecutingContext filterContext)
      {
         var parms = filterContext.ActionDescriptor.GetParameters();

         //avoid sql injection
         foreach (var item in parms)
         {
            if (item.ParameterType == typeof(string))
            {
               if (filterContext.ActionParameters.ContainsKey(item.ParameterName))
               {
                  var parm = filterContext.ActionParameters[item.ParameterName] ?? string.Empty;
                  filterContext.ActionParameters[item.ParameterName]
                     = SecurityScenario.SQLInjection.FilterSqlString(parm.ToString());
               }
            }
         }
      }

   }

}