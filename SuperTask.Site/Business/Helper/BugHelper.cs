using System;
using System.Collections.Generic;

namespace Business.Helper
{

   public class BugHelper
   {

      static APDBDef.BugTableDef b = APDBDef.Bug;

      public static List<Bug> GetBugsByProjectId(Guid projectId, APDBDef db)
      => db.BugDal.ConditionQuery(b.Projectid == projectId, null, null, null);


   }

}