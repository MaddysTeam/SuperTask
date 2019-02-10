using Business;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TheSite.ModelBinder;


namespace XzNursery.Admin
{
   public class MvcApplication : System.Web.HttpApplication
   {
      protected void Application_Start()
      {
          Symber.Web.Compilation.APGenManager.SyncAndInitData();

         AreaRegistration.RegisterAllAreas();
         FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
         RouteConfig.RegisterRoutes(RouteTable.Routes);
         BundleConfig.RegisterBundles(BundleTable.Bundles);

         ModelBinders.Binders.DefaultBinder = new EmptyStringModelBinder();
         ModelBinders.Binders.Add(typeof(AjaxOrder), new AjaxOrderModelBinder());


         JobManager.Initialize(new WorkJournalGenerator());
      }
   }


}
