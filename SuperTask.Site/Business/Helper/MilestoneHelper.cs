using Business;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Helper
{

   public static class MilestoneHelper
   {

      /// <summary>
      /// 查询项目里程碑
      /// </summary>
      /// <param name="projectId">项目id</param>
      /// <param name="db">APDBDef</param>
      /// <returns></returns>
      public static List<ProjectMileStone> GetProjectMileStones(Guid projectId, APDBDef db)
      {
         var ms = APDBDef.MileStone;
         var pms = APDBDef.ProjectMileStone;

         var query = APQuery.select(pms.Status, pms.PmsId, pms.FolderId, pms.StoneId, pms.Content, ms.StoneName)
                             .from(ms, pms.JoinInner(ms.StoneId == pms.StoneId & pms.Projectid == projectId));

         var result = query.query(db, r => new ProjectMileStone
         {
            PmsId = pms.PmsId.GetValue(r),
            FolderId = pms.FolderId.GetValue(r),
            Projectid = projectId,
            Content = pms.Content.GetValue(r),
            Status = pms.Status.GetValue(r),
            StoneId = pms.StoneId.GetValue(r),
            StoneName = ms.StoneName.GetValue(r)
         }).ToList();

         return result;
      }


      /// <summary>
      /// 添加项目里程碑
      /// </summary>
      public static void AddProjectMileStone(Project project, MileStone mileStone, APDBDef db)
      {
         var f = APDBDef.Folder;
         var pm = APDBDef.ProjectMileStone;

         var addUserId = project.CreatorId;
         var projectId = project.ProjectId;
         var stoneId = mileStone.StoneId;

         var isExists = db.ProjectMileStoneDal.ConditionQueryCount(pm.Projectid == projectId & pm.StoneId == stoneId) > 0;
         if (!isExists)
         {
            // step1 if folder is not exists, then add folder
            var folder = ShareFolderHelper.CreateFolder(Guid.NewGuid(), mileStone.StoneName, project.FolderId, addUserId, db);

            // step2 add milestone
            db.ProjectMileStoneDal.Insert(
               new ProjectMileStone(
                  Guid.NewGuid(),
                  stoneId,
                  projectId,
                  folder.FolderId,
                  MilestoneKeys.ReadyStatus,
                  string.Empty
               ));

            //step3 create planTask template
            var tasks = TaskHelper.GetProjectTasks(projectId, db);
            var rootTask = tasks.Find(t => t.IsRoot);
            var planTask = WorkTask.Create(
               addUserId, projectId, mileStone.StoneName, project.StartDate,
               project.EndDate, TaskKeys.PlanStatus, TaskKeys.PlanTaskTaskType,
               2, tasks.Count + 1, false, rootTask.TaskId);

            db.WorkTaskDal.Insert(planTask);
         }
      }

      /// <summary>
      /// 为每个项目创建基本的里程碑
      /// </summary>
      public static void AddDefaultMileStones(Project project, APDBDef db)
      {
         var m = APDBDef.MileStone;

         var defaultStones = db.MileStoneDal.ConditionQuery(m.StoneType == MilestoneKeys.DefaultType, null, null, null);
         foreach (var item in defaultStones)
         {
            AddProjectMileStone(project, item, db);
         }
      }

   }

}
