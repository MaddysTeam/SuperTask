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
      /// 新增节点任务
      /// </summary>
      /// <param name="projectId">项目id</param>
      /// <param name="taskName">任务名称</param>
      /// <param name="start">开始时间</param>
      /// <param name="end">结束时间</param>
      /// <param name="addUserId"></param>
      /// <param name="db">APDBDef</param>
      public static void CreateStoneTask(Guid projectId,string taskName,DateTime start,DateTime end,APDBDef db)
      {
         db.ProjectStoneTaskDal.Insert(new ProjectStoneTask
         {
            PstId = Guid.NewGuid(),
            ProjectId = projectId,
            StartDate = start,
            EndDate =end,
            TaskName = taskName,
            TaskStatus = TaskKeys.PlanStatus,
            CreateDate=DateTime.Now,
         });
      }

   }

}
