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
      /// 查询项目里程碑节点
      /// </summary>
      /// <param name="projectId">项目id</param>
      /// <param name="db">APDBDef</param>
      /// <returns></returns>
      public static List<ProjectMileStone> GetProjectMileStones(Guid projectId, APDBDef db)
      {
         var ms = APDBDef.MileStone;
         var pms = APDBDef.ProjectMileStone;
         var p = APDBDef.Payments;

         var query = APQuery.select(pms.Status, pms.PmsId, pms.FolderId, pms.StoneId, pms.Content,
                                    pms.StartDate, pms.EndDate, ms.StoneName, ms.StoneType,
                                    p.PayId, p.PayName)
                             .from(pms,
                                     ms.JoinLeft(ms.StoneId == pms.StoneId),
                                     p.JoinLeft(p.PayId == pms.StoneId)
                                     )
                             .order_by(ms.Sort.Asc)
                             .where(pms.Projectid == projectId);

         var result = query.query(db, r =>
         {
            var stoneName = ms.StoneName.GetValue(r);
            stoneName = string.IsNullOrEmpty(stoneName) ? p.PayName.GetValue(r) : stoneName ;
            return new ProjectMileStone
            {
               PmsId = pms.PmsId.GetValue(r),
               FolderId = pms.FolderId.GetValue(r),
               Projectid = projectId,
               Content = pms.Content.GetValue(r),
               Status = pms.Status.GetValue(r),
               StoneId = pms.StoneId.GetValue(r),
               StoneName = stoneName,
               StoneType = ms.StoneType.GetValue(r),
               StartDate = pms.StartDate.GetValue(r),
               EndDate = pms.EndDate.GetValue(r)
            };
         }
         ).ToList();

         return result;
      }


      /// <summary>
      /// 添加项目里程碑节点
      /// </summary>
      public static void AddProjectMileStoneIfNotExits(Project project, MileStone mileStone, APDBDef db)
      {
         var f = APDBDef.Folder;
         var pm = APDBDef.ProjectMileStone;

         var isExists = db.ProjectMileStoneDal.ConditionQueryCount(pm.Projectid == project.ProjectId & pm.StoneId == mileStone.StoneId) > 0;
         if (!isExists)
         {
            var stoneId = Guid.NewGuid();

            db.ProjectMileStoneDal.Insert(new ProjectMileStone(
                  stoneId,
                  mileStone.StoneId,
                  project.ProjectId,
                  project.FolderId,
                  string.Empty,
                  project.StartDate,
                  project.EndDate,
                  MilestoneKeys.ReadyStatus,
                  DateTime.Now
               ));

            db.ProjectStoneTaskDal.Insert(new ProjectStoneTask(
               Guid.NewGuid(),
               stoneId,
               project.ProjectId,
               mileStone.StoneName,
               project.StartDate,
               project.EndDate,
               DateTime.MinValue,
               DateTime.MinValue,
               TaskKeys.PlanStatus,
               DateTime.Now,
               TaskKeys.NodeTaskType,
               project.ManagerId,
               project.ReviewerId
               ));
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
            AddProjectMileStoneIfNotExits(project, item, db);
         }
      }

   }

}
