using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using TheSite.Models;

namespace Business
{

   public class ProjectEditHandler : IHandler<Project, ProjectEditOption>
   {
      protected Dictionary<Guid, Action<EditContext>> EditStrategy;

      public ProjectEditHandler()
      {
         EditStrategy = new Dictionary<Guid, Action<EditContext>>();
         EditStrategy.Add(ProjectKeys.PlanStatus, WhenPlan);
         EditStrategy.Add(ProjectKeys.ProcessStatus, WhenStart);
         EditStrategy.Add(ProjectKeys.CompleteStatus, WhenComplete);
         EditStrategy.Add(ProjectKeys.ForceCloseStatus, WhenForceClose);
      }


      public virtual void Handle(Project project, ProjectEditOption option)
      {
         var result = Validate(project, option);
         option.Result = result;
         if (!result.IsSuccess)
            return;

         var db = option.db;
         var isNotExists = Project.PrimaryGet(project.ProjectId) == null;

         db.BeginTrans();

         try
         {
            if (isNotExists)
            {
               InitAndCreateProject(project, option);
            }
            else
            {
               var user = option.CurrentUser;

               option.ProjectTasks = option.ProjectTasks ?? TaskHelper.GetProjectTasks(project.ProjectId, option.db);

               //更改资源角色中的负责人和项目经理
               ResourceHelper.ReplaceLeader(project.ProjectId, project.ManagerId, project.PMId, db);

               if (EditStrategy.ContainsKey(project.ProjectStatus))
                  EditStrategy[project.ProjectStatus].Invoke(new EditContext { db = db, OperatorId = user.UserId, Project = project, Tasks = option.ProjectTasks });

               //最后更新项目数据
               db.ProjectDal.Update(project);

               //如果修改过项目属性则创建记录
              // ProjectRecordHelper.CreateRecord(project, option.Orignal, user.UserId, db);

            }

            db.Commit();
         }
         catch
         {
            db.Rollback();
         }
      }

      protected virtual void InitAndCreateProject(Project project, ProjectEditOption option)
      {
         var db = option.db;
         var user = option.CurrentUser;

         Project.Initial(project);

         project.CreatorId = user.UserId;
         db.ProjectDal.Insert(project);

         //创建项目下的第一个默认资源,默认资源类型为项目负责人
         var currentResource = Resource.Create(
            user.UserName,
            user.UserId,
            project.ProjectId,
            Resource.DefaultLeaderTypes);

         //创建当前用户资源
         db.ResourceDal.Insert(currentResource);

         //创建项目下的资源角色
         ResourceHelper.AddDefaultResoureRoles(project, db);

         //创建项目下的第一个默认任务   
         TaskHelper.CreateAndSaveRootTaskInDB(user, project, db);

         //创建项目文件夹
         var folder=ShareFolderHelper.CreateFolder(project.FolderId, project.ProjectName, ShareFolderKeys.RootProjectFolderId, user.UserId, db);

         //创建默认里程碑节点和节点任务
         MilestoneHelper.AddDefaultMileStones(project, db);

         //创建默认款项管理
          PaymentsHelper.AddDefaultPayments(project, db);

         //创建项目记录
         //ProjectRecordHelper.CreateRecord(project, user.UserId, db);
      }

      protected virtual Result Validate(Project project, ProjectEditOption option)
      {
         var result = project.Validate();//基本检查，用于项目新增或修改
         if (!result.IsSuccess)
         {
            return result;
         }

         if (option.CurrentUser == null)
         {
            result.IsSuccess = false;
            result.Msg = Errors.Project.NOT_ALLOWED_OPERATOR_NULL;
            return result;
         }

         if (option.Orignal != null && option.Orignal.ProjectStatus == ProjectKeys.CompleteStatus)
         {
            result.IsSuccess = false;
            result.Msg = Errors.Project.HAS_COMPLETE;
            return result;
         }

         return result;
      }

      protected virtual void WhenPlan(EditContext ctx)
      {
         ctx.Project.SetStatus(ProjectKeys.PlanStatus);
      }

      protected virtual void WhenStart(EditContext ctx)
      {
         var project = ctx.Project;
         var tasks = ctx.Tasks;
         var operatorId = ctx.OperatorId;
         var db = ctx.db;

         project.Start();

         //启动节点任务
         //var pst = APDBDef.ProjectStoneTask;
         //APQuery.update(pst)
         //        .set(
         //             pst.TaskStatus.SetValue(TaskKeys.ProcessStatus),
         //             pst.RealStartDate.SetValue(DateTime.Now))
         //        .where(pst.ProjectId == project.ProjectId)
         //        .execute(ctx.db);

         //启动未启动的任务，将任务设置为启动状态
         foreach (var tk in tasks)
         {
            if (tk.IsPlanStatus && tk.HasPlan)
            {
               tk.Start();

               db.WorkTaskDal.UpdatePartial(tk.TaskId, new { TaskStatus = tk.TaskStatus, RealStartDate = tk.RealStartDate });

               WorkJournalHelper.CreateOrUpdateJournalByTask(tk, db);

               TaskLogHelper.CreateLog(tk, operatorId, db);
            }
         }
      }

      protected virtual void WhenComplete(EditContext ctx)
      {
         ctx.Project.Complete();
      }

      protected virtual void WhenForceClose(EditContext ctx)
      {
         if (ctx != null
            && ctx.db != null
            && ctx.Project != null
            && !ctx.Project.ProjectId.IsEmpty())
         {
            var t = APDBDef.WorkTask;
            APQuery.update(t)
                   .set(
                        t.TaskStatus.SetValue(TaskKeys.CompleteStatus),
                        t.RateOfProgress.SetValue(100),
                        t.RealEndDate.SetValue(DateTime.Now))
                   .where(t.Projectid == ctx.Project.ProjectId)
                   .execute(ctx.db);

            WhenComplete(ctx);
         }
      }


      protected sealed class EditContext
      {
         public Project Project { get; set; }
         public List<WorkTask> Tasks { get; set; }
         public Guid OperatorId { get; set; }
         public APDBDef db { get; set; }
      }
   }


   public class ProjectEditOption
   {

      public UserInfo CurrentUser { get; set; }

      public APDBDef db { get; set; } = new APDBDef();

      public Result Result { get; set; } = Result.Initial();

      public WorkTask RootTask { get; set; }

      public List<WorkTask> ProjectTasks { get; set; }

      public Guid Status { get; set; } = ProjectKeys.PlanStatus; // 表示当前项目状态，计划中，执行中，已完成

      public Project Orignal { get; set; }

   }

}
