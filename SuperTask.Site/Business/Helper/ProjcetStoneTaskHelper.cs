using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

namespace Business.Helper
{

   public class ProjcetStoneTaskHelper
   {

      public static void RefreshManager(Guid projectId,Guid newMemberId, APDBDef db)
      {
         var pst = APDBDef.ProjectStoneTask;
         APQuery.update(pst).set(pst.ManagerId.SetValue(newMemberId)).where(pst.ProjectId == projectId).execute(db);
      }


   }

}