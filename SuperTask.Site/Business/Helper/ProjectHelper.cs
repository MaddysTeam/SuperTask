using Business;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Helper
{

   public static class ProjectrHelper
   {

      private static Project _currentProject;


      public static List<Project> UserJoinedProjects(Guid userId, APDBDef db)
      {
         var p = APDBDef.Project;
         var re = APDBDef.Resource;

         var projects = APQuery.select(p.ProjectId, p.ProjectName)
                .from(p,
                      re.JoinInner(re.Projectid == p.ProjectId & re.UserId == userId))
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
      /// <param name="projectid"></param>
      /// <param name="db"></param>
      /// <param name="isforceClear"></param>
      /// <returns></returns>
      public static Project GetCurrentProject(Guid projectid, APDBDef db = null, bool isforceClear = false)
      {
         if (_currentProject != null && _currentProject.ProjectId == projectid && !isforceClear)
            return _currentProject;

         db = db ?? new APDBDef();
         _currentProject = db.ProjectDal.PrimaryGet(projectid);

         return _currentProject;
      }


      public static List<ProjectMileStone> GetProjectMileStones(Guid projectId, APDBDef db)
      {
         var ms = APDBDef.MileStone;
         var pms = APDBDef.ProjectMileStone;

         var query = APQuery.select(ms.StoneId, ms.StoneName,
                                    pms.Status, pms.PmsId, pms.FolderId, pms.FolderId, pms.Content)
                             .from(ms, pms.JoinLeft(ms.StoneId == pms.StoneId & pms.Projectid == projectId));

         var result = query.query(db, r => new ProjectMileStone
         {
            PmsId = pms.PmsId.GetValue(r),
            FolderId = pms.FolderId.GetValue(r),
            Projectid = projectId,
            Content = pms.Content.GetValue(r),
            Status = pms.Status.GetValue(r),
            StoneId = ms.StoneId.GetValue(r),
            StoneName = ms.StoneName.GetValue(r)
         }).ToList();

         return result;
      }


      public static void AddMileStone(Guid userId, Guid projectId, Guid mileStoneId, APDBDef db)
      {
         var f = APDBDef.Folder;
         var pm = APDBDef.ProjectMileStone;

         var mileStone = MileStone.PrimaryGet(mileStoneId);
         var project = Project.PrimaryGet(projectId);
         var isExists = ProjectMileStone.ConditionQueryCount(pm.Projectid == projectId & pm.StoneId == mileStoneId) > 0;
         if (!isExists)
         {
            // step1 if folder is not exists, then add folder
            var mileStonFolder = new Folder
            {
               FolderId = Guid.NewGuid(),
               FolderName = mileStone.StoneName,
               ParentId = project.FolderId,
               OperatorId = userId,
            };
            APBplDef.FolderBpl.Insert(mileStonFolder);

            // step2 add milestone
            db.ProjectMileStoneDal.Insert(
               new ProjectMileStone(
                  Guid.NewGuid(),
                  mileStoneId,
                  projectId,
                  mileStonFolder.FolderId,
                  MilestoneKeys.ReadyStatus,
                  string.Empty
               ));

            //step3 create planTask template
            var tasks = TaskHelper.GetProjectTasks(projectId);
            var rootTask = tasks.Find(t => t.IsRoot);
            var planTask = WorkTask.Create(userId, projectId, mileStone.StoneName, project.StartDate, project.EndDate, TaskKeys.PlanStatus, TaskKeys.PlanTaskTaskType, 1, tasks.Count+1, true, rootTask.TaskId);

            db.WorkTaskDal.Insert(planTask);

         }

      }

   }


   public class ProjectRecordHelper
   {

      static APDBDef.ProjectRecordTableDef prt = APDBDef.ProjectRecord;

      public static void CreateRecord(Project proj, Guid operatorId, APDBDef db)
      {
         db.ProjectRecordDal.Insert(new ProjectRecord
         {
            RecordId = Guid.NewGuid(),
            RecordDate = DateTime.Now,
            StartDate = proj.StartDate,
            EndDate = proj.EndDate,
            Progress = proj.RateOfProgress,
            ProjectStatus = proj.ProjectStatus,
            ProjectType = proj.ProjectType,
            ProjectId = proj.ProjectId,
            RateOfProgress = proj.RateOfProgress,
            PMId = proj.PMId,
            ManagerId = proj.ManagerId,
            ReviewerId = proj.ReviewerId,
            OperatorId = operatorId,
            ProjectExecutor = proj.ProjectExecutor,
            ProjectOwner = proj.ProjectOwner,
            RealCode = proj.RealCode,
            Code = proj.Code,
            ProjectName = proj.ProjectName,
            ProcessName = proj.ProcessName
         });
      }

      public static void CreateRecord(Project proj, Project orignal, Guid operatorId, APDBDef db)
      {
         if (!proj.Equals(orignal))
            CreateRecord(proj, operatorId, db);
      }

   }


}
