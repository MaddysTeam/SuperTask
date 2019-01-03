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

         var query = APQuery.select(pms.Status, pms.PmsId, pms.FolderId, pms.StoneId, pms.Content, ms.StoneName,ms.StoneType)
                             .from(ms, pms.JoinInner(ms.StoneId == pms.StoneId & pms.Projectid == projectId));

         var result = query.query(db, r => new ProjectMileStone
         {
            PmsId = pms.PmsId.GetValue(r),
            FolderId = pms.FolderId.GetValue(r),
            Projectid = projectId,
            Content = pms.Content.GetValue(r),
            Status = pms.Status.GetValue(r),
            StoneId = pms.StoneId.GetValue(r),
            StoneName = ms.StoneName.GetValue(r),
            StoneType=ms.StoneType.GetValue(r)
         }).ToList();

         return result;
      }


      /// <summary>
      /// 添加项目里程碑节点
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
            var projectStone = new ProjectMileStone(
                  Guid.NewGuid(),
                  stoneId,
                  projectId,
                  folder.FolderId,
                  string.Empty,
                  project.StartDate,
                  project.EndDate,
                  MilestoneKeys.ReadyStatus
               );

            // step2 add milestone
            db.ProjectMileStoneDal.Insert(projectStone);

            //step3 create stoneTask template
            db.ProjectStoneTaskDal.Insert(new ProjectStoneTask(Guid.NewGuid(), projectStone.PmsId,mileStone.StoneName, project.StartDate,project.EndDate, DateTime.MinValue, DateTime.MinValue, TaskKeys.PlanStatus));
         }
      }

      /// <summary>
      /// 为每个项目创建基本的里程碑节点
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
