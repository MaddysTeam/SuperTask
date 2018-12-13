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


      public static Project GetCurrentProject(Guid projectid, APDBDef db=null,bool isforceClear=false)
      {
         if (_currentProject != null && _currentProject.ProjectId == projectid && !isforceClear)
            return _currentProject;

         db = db ?? new APDBDef();
         _currentProject = db.ProjectDal.PrimaryGet(projectid);

         return _currentProject;
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
            ProjectType=proj.ProjectType,
            ProjectId = proj.ProjectId,
            RateOfProgress = proj.RateOfProgress,
            PMId=proj.PMId,
            ManagerId=proj.ManagerId,
            ReviewerId = proj.ReviewerId,
            OperatorId = operatorId,
            ProjectExecutor = proj.ProjectExecutor,
            ProjectOwner=proj.ProjectOwner,
            RealCode=proj.RealCode,
            Code=proj.Code,
            ProjectName=proj.ProjectName,
            ProcessName=proj.ProcessName
         });
      }

      public static void CreateRecord(Project proj,Project orignal, Guid operatorId, APDBDef db)
      {
         if (!proj.Equals(orignal))
            CreateRecord(proj, operatorId, db);
      }

   }


}
