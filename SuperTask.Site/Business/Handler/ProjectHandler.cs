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
               ProjectRecordHelper.CreateRecord(project, option.Orignal, user.UserId, db);

            }

            db.Commit();
         }
         catch(Exception e)
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

         db.ResourceDal.Insert(currentResource);

         //创建项目下的资源角色
         ResourceHelper.AddDefaultResoureRoles(project, db);

         //创建项目下的第一个默认任务   
         TaskHelper.CreateAndSaveRootTaskInDB(user, project, db);

         //创建项目文件夹
         var folder=ShareFolderHelper.CreateFolder(project.FolderId, project.ProjectName, ShareFolderKeys.RootProjectFolderId, user.UserId, db);

         //创建默认里程碑
         MilestoneHelper.AddDefaultMileStones(project, db);

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


   public class ProjectTemplateEditHandler : IHandler<Project, PorjectTemplateEditOption>
   {

      public void Handle(Project p, PorjectTemplateEditOption v)
      {
         //创建项目
         p = CreateProject(p, v);


         //创建任务
         var taskOptions = new TaskEditOption { Project = p, db = v.db };
         CreateTasks(taskOptions);
      }


      protected virtual Project CreateProject(Project p, PorjectTemplateEditOption v)
      {
         v.ProjectEditHandler.Handle(p, v);

         return p;
      }


      protected virtual void CreateTasks(TaskEditOption option)
      {

      }

   }


   public class DevelopmentProjectTemplateEditHandler : ProjectTemplateEditHandler
   {
      protected override Project CreateProject(Project p, PorjectTemplateEditOption v)
      {
         var start = DateTime.Now;
         var end = DateTime.Now.AddDays(10);
         p = p ?? Project.Create("软件开发类项目", v.CurrentUser.UserId, start, end, ProjectKeys.PlanStatus, ProjectKeys.DefaultProjectType);

         return base.CreateProject(p, v);
      }

      protected override void CreateTasks(TaskEditOption option)
      {
         var projectId = option.Project.ProjectId;
         var createrId = option.Project.CreatorId;

         var db = option.db ?? new APDBDef();
         var r = APDBDef.WorkTask;
         var start = DateTime.Now;
         var end = DateTime.Now.AddDays(10);
         var list = new List<WorkTask>();

         db.WorkTaskDal.ConditionDelete(r.Projectid == projectId & r.ParentId == Guid.Empty & r.SortId == 1);

         var rootTask = WorkTask.Create(createrId, projectId, "软件开发类项目", start, end, TaskKeys.PlanStatus, TaskKeys.ProjectTaskType, 1, 1, true, Guid.Empty);
         var task1 = WorkTask.Create(createrId, projectId, "功能模块", start, end, TaskKeys.PlanStatus, TaskKeys.ProjectTaskType, 1, 2, true, rootTask.TaskId);
         var task2 = WorkTask.Create(createrId, projectId, "功能子模块1-1", start, end, TaskKeys.PlanStatus, TaskKeys.ProjectTaskType, 3, 3, false, task1.TaskId);
         var task3 = WorkTask.Create(createrId, projectId, "功能子模块1-2", start, end, TaskKeys.PlanStatus, TaskKeys.ProjectTaskType, 3, 4, false, task1.TaskId);
         var task4 = WorkTask.Create(createrId, projectId, "功能子模块1-4", start, end, TaskKeys.PlanStatus, TaskKeys.ProjectTaskType, 3, 5, false, task1.TaskId);
         var task5 = WorkTask.Create(createrId, projectId, "测试模块1", start, end, TaskKeys.PlanStatus, TaskKeys.ProjectTaskType, 2, 6, true, task1.TaskId);
         var task6 = WorkTask.Create(createrId, projectId, "测试任务1", start, end, TaskKeys.PlanStatus, TaskKeys.ProjectTaskType, 3, 7, false, task5.TaskId);
         var task7 = WorkTask.Create(createrId, projectId, "测试任务2", start, end, TaskKeys.PlanStatus, TaskKeys.ProjectTaskType, 3, 8, false, task5.TaskId);
         list = new WorkTask[] { task1, task2, task3, task4, task5, task6, task7 }.ToList();
         foreach (var item in list)
         {
            db.WorkTaskDal.Insert(item);
         }
      }
   }


   public class MaintanceProjectTemplateEditHandler : ProjectTemplateEditHandler
   {

      protected override Project CreateProject(Project p, PorjectTemplateEditOption v)
      {
         return base.CreateProject(p, v);
      }


      protected override void CreateTasks(TaskEditOption option)
      {
         base.CreateTasks(option);
      }

   }


   public class PorjectTemplateEditOption : ProjectEditOption
   {
      public ProjectEditHandler ProjectEditHandler { get; set; }
   }

}
