using Business;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Helper
{

   public static class StoneTaskHelper
   {

      private static APDBDef.ProjectStoneTaskTableDef pst = APDBDef.ProjectStoneTask;
      private static APDBDef.ProjectMileStoneTableDef pm = APDBDef.ProjectMileStone;


      /// <summary>
      /// get stone task by stone id
      /// </summary>
      /// <param name="stoneId">stone id</param>
      /// <param name="db">APDBDef</param>
      /// <returns>StoneTasks</returns>
      public static List<ProjectStoneTask> GetProjectStoneTasksByStoneId(Guid stoneId, APDBDef db)
      {
         return db.ProjectStoneTaskDal.ConditionQuery(pst.PmsId == stoneId, null, null, null);
      }


      /// <summary>
      ///  get project stone tasks
      /// </summary>
      /// <param name="projectId">project id</param>
      /// <param name="db">APDBDef</param>
      /// <returns>StoneTasks</returns>
      public static List<ProjectStoneTask> GetProjectStoneTasks(Guid projectId, APDBDef db)
      {
         return APQuery.select(pst.Asterisk)
                       .from(pst, pm.JoinInner(pst.PmsId == pm.PmsId))
                       .where(pm.Projectid == projectId)
                       .query(db, r =>
                       {
                          ProjectStoneTask data = new ProjectStoneTask();
                          pst.Fullup(r, data, false);
                          return data;
                       }).ToList() ;
      }

   }

}
