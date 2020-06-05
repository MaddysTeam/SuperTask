using Business;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Helper
{

   public static class ProjectrHelper
   {
      /// <summary>
      /// 某用户参与的所有项目
      /// </summary>
      /// <param name="userId">用户id</param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static List<Project> UserJoinedProjects(Guid userId, APDBDef db)
      {
         var p = APDBDef.Project;
         var re = APDBDef.Resource;

         var projects = APQuery.select(p.ProjectId, p.ProjectName)
                .from(p,
                      re.JoinInner(re.Projectid == p.ProjectId & re.UserId == userId))
					  .order_by(p.CreateDate.Desc)
                .query(db, r => new Project
                {
                   ProjectId = p.ProjectId.GetValue(r),
                   ProjectName = p.ProjectName.GetValue(r)
                }).ToList();

         return projects;
      }

      /// <summary>
      /// 通过项目id获取当前的项目数据
      /// </summary>
      /// <param name="projectid">项目id</param>
      /// <param name="db"></param>
      /// <param name="isforceClear"></param>
      /// <returns></returns>
      public static Project GetCurrentProject(Guid projectid, APDBDef db = null, bool isforceClear = false)
      {
         db = db ?? new APDBDef();
         return db.ProjectDal.PrimaryGet(projectid);
      }


      /// <summary>
      /// 通过项目节点任务的完成率来得到项目进度
      /// </summary>
      /// <param name="projectId">项目id</param>
      /// <param name="db"></param>
      /// <returns></returns>
      public static  double GetProcessByNodeTasks(Guid projectId,APDBDef db)
      {
         var pst = APDBDef.ProjectStoneTask;
         var all = db.ProjectStoneTaskDal.ConditionQueryCount(pst.ProjectId== projectId);
         var completed = db.ProjectStoneTaskDal.ConditionQueryCount(pst.ProjectId == projectId & pst.TaskStatus == TaskKeys.CompleteStatus);

         return all <= 0 ? 0 : ((double)(completed * 100 / all)).Round(2);
      }


   }


   //public class ProjectRecordHelper
   //{

   //   static APDBDef.ProjectRecordTableDef prt = APDBDef.ProjectRecord;

   //   public static void CreateRecord(Project proj, Guid operatorId, APDBDef db)
   //   {
   //      db.ProjectRecordDal.Insert(new ProjectRecord
   //      {
   //         RecordId = Guid.NewGuid(),
   //         RecordDate = DateTime.Now,
   //         StartDate = proj.StartDate,
   //         EndDate = proj.EndDate,
   //         Progress = proj.RateOfProgress,
   //         ProjectStatus = proj.ProjectStatus,
   //         ProjectType = proj.ProjectType,
   //         ProjectId = proj.ProjectId,
   //         RateOfProgress = proj.RateOfProgress,
   //         PMId = proj.PMId,
   //         ManagerId = proj.ManagerId,
   //         ReviewerId = proj.ReviewerId,
   //         OperatorId = operatorId,
   //         ProjectExecutor = proj.ProjectExecutor,
   //         ProjectOwner = proj.ProjectOwner,
   //         RealCode = proj.RealCode,
   //         Code = proj.Code,
   //         ProjectName = proj.ProjectName,
   //         ProcessName = proj.ProcessName
   //      });
   //   }

   //   public static void CreateRecord(Project proj, Project orignal, Guid operatorId, APDBDef db)
   //   {
   //      if (!proj.Equals(orignal))
   //         CreateRecord(proj, operatorId, db);
   //   }

   //}


}
