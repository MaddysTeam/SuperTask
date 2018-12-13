using Symber.Web.Data;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Business.Helper
{

   public class ComplexityHelper
   {
      static APDBDef.WorkTaskComplextiyTableDef wtc= APDBDef.WorkTaskComplextiy;
      static APDBDef.TaskCompelxtiyRoleTableDef tcr = APDBDef.TaskCompelxtiyRole;

      public static List<WorkTaskComplextiy> GetFloatComplexity(APDBDef db)
      {
         var result = APQuery.select(wtc.TaskId, wtc.CreatorRoleId, wtc.Complexity.Avg().As("AvgComplexity"), tcr.Propertion)
                            .from(wtc, tcr.JoinInner(wtc.CreatorRoleId == tcr.RoleId))
                            .group_by(wtc.TaskId, tcr.Propertion, wtc.CreatorRoleId)
                            .query(db, r => new WorkTaskComplextiy
                            {
                               TaskId = wtc.TaskId.GetValue(r),
                               CreatorRoleId = wtc.CreatorRoleId.GetValue(r),
                               //角色设置的平均复杂度*角色占比%
                               Complexity = wtc.Complexity.GetValue(r, "AvgComplexity") * (tcr.Propertion.GetValue(r)/100),
                            }).ToList();

         return result;
      }

   }

}