using System;
using System.Collections.Generic;

namespace Business.Helper
{

   public class RequireHelper
   {

      static APDBDef.RequireTableDef rq = APDBDef.Require;
   

      public static List<Require> GetRequiresByProjectId(Guid projectId, APDBDef db)
      => db.RequireDal.ConditionQuery(rq.Projectid == projectId, null, null, null);


      public static List<Require> GetRequiresByManager(Guid managerid, APDBDef db)
     => db.RequireDal.ConditionQuery(rq.ManagerId == managerid | rq.CreatorId==managerid, null, null, null);

      public static List<Require> GetRequiresByReviewer(Guid reviewerId, APDBDef db)
    => db.RequireDal.ConditionQuery(rq.ReviewerId == reviewerId, null, null, null);

   }

}