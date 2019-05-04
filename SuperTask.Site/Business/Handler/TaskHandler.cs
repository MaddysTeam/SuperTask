using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using TheSite.Models;

namespace Business
{

   public class DefaultTaskSearchHandler : IHandler<APSqlSelectCommand, TaskSearchOption>
   {
      public virtual void Handle(APSqlSelectCommand t, TaskSearchOption v) { }
   }

   public class TaskSearchHandler : DefaultTaskSearchHandler
   {

      protected APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
      protected APDBDef.ProjectTableDef p = APDBDef.Project;
      protected APDBDef.UserInfoTableDef u = APDBDef.UserInfo;
      protected APDBDef.WorkJournalTableDef wj = APDBDef.WorkJournal;

      public override void Handle(APSqlSelectCommand query, TaskSearchOption option)
      {
         if (option == null || query == null)
            return;

         if (option.SearchType == TaskKeys.SearchByDetaultType)
         {
            if (option.RangeType == TaskKeys.SendByMeRangeType)
               query = query.where_and(t.CreatorId == option.UserId);
            else if (option.RangeType == TaskKeys.ReviewByMeRangeType)
               query = query.where_and(t.ReviewerID == option.UserId);
            else
               query.where_and(t.ManagerId == option.UserId);
         }

         if (option.Status != TaskKeys.SelectAll)
            query.where_and(t.TaskStatus == option.Status);
         else
            query.where_and(t.TaskStatus != TaskKeys.DeleteStatus);

         if (option.Type != TaskKeys.SelectAll)
            query.where_and(t.TaskType == option.Type);

         if (option.ProjectId != TaskKeys.SelectAll)
            query.where_and(t.Projectid == option.ProjectId);

         if (!option.TaskId.IsEmpty())
            query.where_and(t.TaskId == option.TaskId);

         if (!string.IsNullOrEmpty(option.TaskNamePhrase))
            query.where_and(t.TaskName.Match(option.TaskNamePhrase));

         if (option.StartDate > DateTime.MinValue)
            query = query.where_and(wj.RecordDate >= option.StartDate.TodayStart() & wj.WorkHours > 0);

         if (option.EndDate > DateTime.MinValue)
            query = query.where_and(wj.RecordDate < option.EndDate.TodayEnd() & wj.WorkHours > 0);
      }

   }

   public class ProjectTaskSearchHandler : TaskSearchHandler
   {
      public override void Handle(APSqlSelectCommand query, TaskSearchOption option)
      {
         base.Handle(query, option);

         if (!option.IsShowParent)
         {
            query.where_and(t.IsParent == false);
         }
         if (option.Level > 0)
         {
            query.where_and(t.TaskLevel == option.Level);
         }
      }

   }

   public class PersonalTaskSearchHandler : TaskSearchHandler
   {
      public override void Handle(APSqlSelectCommand query, TaskSearchOption option)
      {
         base.Handle(query, option);

         query.where_and(wj.RecordType == JournalKeys.ManuRecordType);
         query.where_and(t.ManagerId == option.UserId);

         if (option.ProjectId.IsEmpty())
         {
            query.where_and(t.TaskType == TaskKeys.TempTaskType);
         }

      }
   }


   public class TaskEditHandler : IHandler<WorkTask, TaskEditOption>
   {

      protected Dictionary<Guid, Action<EditContext>> EditStrategy;

      public TaskEditHandler()
      {
         EditStrategy = new Dictionary<Guid, Action<EditContext>>();
         EditStrategy.Add(TaskKeys.PlanStatus, WhenPlan);
         EditStrategy.Add(TaskKeys.ProcessStatus, WhenStart);
         EditStrategy.Add(TaskKeys.ReviewStatus, WhenReview);
         EditStrategy.Add(TaskKeys.TaskTempEditStatus, WhenTempEdit);
         EditStrategy.Add(TaskKeys.CompleteStatus, WhenComplete);
         EditStrategy.Add(TaskKeys.DeleteStatus, WhenDelete);
      }


      public virtual void Handle(WorkTask task, TaskEditOption option)
      {
      }


      protected class EditContext
      {
         public Project Project { get; set; }
         public WorkTask Task { get; set; }
         public APDBDef db { get; set; }
         public Result Result { get; set; }
         public List<WorkTask> AllTasks { get; set; }
         public Guid OperatorId { get; set; }
         public List<WorkTask> OrignalTasks { get; set; }
         public List<WorkTask> DeletedTasks { get; set; }
      }

      protected virtual Result Validate(WorkTask task, TaskEditOption ctx) { throw new NotImplementedException(); }

      protected virtual void UploadAttachment(Attachment attachment, WorkTask task, APDBDef db)
      {
         if (attachment != null && !string.IsNullOrEmpty(attachment.Url))
         {
            attachment.TaskId = task.TaskId;
            attachment.Projectid = task.Projectid;
            attachment.PublishUserId = task.ManagerId;
            attachment.UploadDate = DateTime.Now;
            attachment.AttachmentId = Guid.NewGuid();

            db.AttachmentDal.Insert(attachment);
         }
      }

      protected virtual void WhenPlan(EditContext ctx) { }

      protected virtual void WhenStart(EditContext ctx)
      {
         ctx.Task.Start();
      }

      protected virtual void WhenReview(EditContext ctx)
      {
         ctx.Task.SetStatus(TaskKeys.ReviewStatus);
      }

      protected virtual void WhenTempEdit(EditContext ctx)
      {
         //任务临时修改后会进入执行状态
         ctx.Task.SetStatus(TaskKeys.ProcessStatus);
      }

      protected virtual void WhenComplete(EditContext ctx)
      {
         //将最新的日志的进度设置为100%
         var wj = APDBDef.WorkJournal;

         var task = ctx.Task;
         var lastJournal = ctx.db.WorkJournalDal.ConditionQuery(wj.TaskId == task.TaskId, wj.RecordDate.Desc, null, null).FirstOrDefault();

         if (lastJournal != null)
            ctx.db.WorkJournalDal.UpdatePartial(lastJournal.JournalId, new { Progress = 100 });

      }

      protected virtual void WhenDelete(EditContext ctx)
      {
         if (ctx.Task.IsWorked)
         {
            ctx.Result.IsSuccess = false;
            ctx.Result.Msg = "已经有实际工时的任务无法删除";
            return;
         }

         ctx.Task.SetStatus(TaskKeys.DeleteStatus);
      }

   }

   public class ProjectTaskEditHandler : TaskEditHandler
   {

      APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;


      public ProjectTaskEditHandler() : base() { }


      public override void Handle(WorkTask task, TaskEditOption option)
      {
         var r = APDBDef.Resource;
         var db = option.db;
         var re = option.Result;
         var pj = option.Project ?? db.ProjectDal.PrimaryGet(task.Projectid);
         var tks = option.Tasks ?? db.WorkTaskDal.ConditionQuery(t.Projectid == pj.ProjectId & t.TaskStatus != TaskKeys.DeleteStatus, t.SortId.Asc, null, null);
         var orignalTks = tks.DeepClone();

         option.Project = pj;
         option.Tasks = tks;
         var result = Validate(task, option);
         if (!result.IsSuccess) return;


         var index = tks.FindIndex(tk => tk.TaskId == task.TaskId);
         if (index > -1)
            tks[index] = task;//将task 放入集合，方便后面一起修改


         if (!task.ParentId.IsEmpty()
            && task.ParentId != task.TaskId
            && task.TaskStatus != TaskKeys.CompleteStatus)
         {
            var parentTask = tks.FirstOrDefault(tk => tk.TaskId == task.ParentId);
            if (parentTask != null)
            {
               parentTask.IsParent = true;

               //TODO:项目启动后，为了不影响父任务的时间，限制住子任务的时间
               if (pj.IsProcessStatus || pj.IsEditStatus)
               {
                  if (task.StartDate > parentTask.EndDate) task.StartDate = parentTask.EndDate;
                  if (task.StartDate < parentTask.StartDate) task.StartDate = parentTask.StartDate;
                  if (task.EndDate > parentTask.EndDate) task.EndDate = parentTask.EndDate;
                  if (task.EndDate < parentTask.StartDate) task.EndDate = parentTask.StartDate;
               }

               // 现规定如果任务sortId和level大于0，将不能再任务模块更换任务节点位置,注意设置成其他任务类型时将sortid 和level 设为0
               if (!task.HasArrangement)
               {
                  //如果是新节点，节点插入父节点后，为了保持甘特图原本结构，所有的后续节点的sortid 都要自增
                  var lastChildIndex = tks.FindLastIndex(x => x.ParentId == task.ParentId);
                  var insertIndex = lastChildIndex <= 0 ? parentTask.SortId : lastChildIndex;
                  insertIndex = insertIndex == 0 ? 1 : insertIndex;

                  // 节点插入父节点的必要条件
                  task.SortId = insertIndex + 1;
                  task.TaskLevel = parentTask.TaskLevel + 1;

                  tks.Insert(insertIndex, task);

                  for (int i = insertIndex + 1; i < tks.Count; i++)
                  {
                     tks[i].SortId += 1;
                  }
               }
            }
         }
         if (task.Projectid != null)
         {
            var pjUserTasks = tks.FindAll(t => t.ManagerId == task.ManagerId);

            if (pjUserTasks.Count == 0 && task.SortId == 0)
            {
               task.SortId = 1;
               task.TaskLevel = 1;
            }
            else if (task.SortId == 0 && pjUserTasks.Count > 0)
            {
               task.SortId = pjUserTasks.Max(x => x.SortId) + 1;
               task.TaskLevel = pjUserTasks.Min(x => x.TaskLevel) + 1;
            }

         }


         db.BeginTrans();


         try
         {
            //如果该任务负责人不属于项目，则添加到项目资源
            ResourceHelper.AddUserToResourceIfNotExist(pj.ProjectId, task.TaskId, task.ManagerId, ResourceKeys.OtherType, db);

            //如果审核人不属于项目，则添加到项目资源
            ResourceHelper.AddUserToResourceIfNotExist(pj.ProjectId, task.TaskId, task.ReviewerID, ResourceKeys.OtherType, db);

            var ctx = new EditContext();
            if (EditStrategy.ContainsKey(task.TaskStatus))
            {
               //执行不同状态时的逻辑
               ctx = new EditContext { Project = pj, Task = task, db = db, Result = re, AllTasks = tks, OrignalTasks = orignalTks, OperatorId = option.OperatorId };
               EditStrategy[task.TaskStatus].Invoke(ctx);

               if (!ctx.Result.IsSuccess)
               {
                  db.Rollback();
                  return;
               }
            }

            //如果是新建任务则插入数据库
            if (db.WorkTaskDal.PrimaryGet(task.TaskId) == null)
               db.WorkTaskDal.Insert(task);


            //最后调整任务状态和项目进度,更新tasks集合所有task的状态
            //任务状态需要注意的原则是
            //1：所有子任务完成，父任务变完成
            //2：只要子任务已经启动，则父任务自动启动
            SetTaskAndProjectProcess(db, tks);


            //创建或者修改所有父任务的工作日志
            var parentTks = TaskHelper.GetAllParents(task, tks);
            WorkJournalHelper.CreateOrUpdateJournalByTasks(parentTks, db);
            WorkJournalHelper.CreateOrUpdateJournalByTask(task, db);

            //创建任务记录日志（非系统日志）
            TaskLogHelper.CreateLogs(tks, orignalTks, option.OperatorId, db);
            TaskLogHelper.CreateLogs(ctx.DeletedTasks, option.OperatorId, db);


            //更新项目进度和开始结束时间
            //if (tks.Count > 0)
            //   db.ProjectDal.UpdatePartial(pj.ProjectId, new { RateOfProgress = tks[0].RateOfProgress, StartDate = tks[0].StartDate, EndDate = tks[0].EndDate });


            //上传附件
            UploadAttachment(task.CurrentAttachment, task, db);


            db.Commit();
         }
         catch (Exception e)
         {
            db.Rollback();

            re.IsSuccess = false;
            re.Msg = e.InnerException.ToString();

            return;
         }

         //fix bug: 有点复杂，有可能并发同时修改任务树会导致sortId 的错乱，
         // 结果是任务节点发生上移的问题（父任务的sortId 可能会大于子任务
         var parentTk = db.WorkTaskDal.PrimaryGet(task.ParentId);
         if (parentTk != null && parentTk.SortId >= task.SortId)
            db.WorkTaskDal.UpdatePartial(task.TaskId, new { SortId = parentTk.SortId + 1, TaskLevel = parentTk.TaskLevel + 1 });

         re.IsSuccess = true;
         re.Msg = "操作成功";
      }


      protected override Result Validate(WorkTask task, TaskEditOption option)
      {
         var tks = option.Tasks;
         var pj = option.Project;
         var re = option.Result;
         var otk = option.Original;

         if (pj == null)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Project.NOT_EXIST;
         }
         else if (pj.IsCompleteStatus)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.NOT_ALLOWED_EDIT_TASK_WHEN_PROJECT_COMPELETE;
         }
         else if (tks.Count <= 0)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.NOT_HAVE_ANY_TASKS;
         }
         else if (otk != null && otk.IsWorked && otk.TaskType == TaskKeys.TempTaskType)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.TEMP_TASK_WHICH_HAS_WORKHOURS_NOT_ALLOWED_CHANG_AS_PROJECT_TASK;
         }
         else if (otk != null && otk.IsWorked &&
            (otk.SubTypeId != task.SubTypeId || otk.TaskType != task.TaskType)
            )
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.NOT_ALLOWED_CHANGE_IF_IS_WORKED;
         }
         else if (task.ParentId.IsEmpty() && task.SortId > 1)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.NOT_ALLOWED_PARENT_EMPTY;
         }
         else if (task.EstimateWorkHours <= 0 && !task.IsCompleteStatus)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.NOT_ALLOWED_ESTIMATE_HOURS_ZERO;
         }
         else if (pj.IsProcessStatus || pj.IsEditStatus)
         {
            if (task.StartDate > pj.EndDate ||
               task.StartDate < pj.StartDate ||
               task.EndDate > pj.EndDate ||
               task.EndDate < pj.StartDate)
            {
               re.IsSuccess = false;
               re.Msg = Errors.Task.TASKS_OUT_OF_PROJECT_RANGE;
            }

            if (task.IsParent && otk != null && (otk.EndDate > task.EndDate || otk.StartDate != task.StartDate))
            {
               re.IsSuccess = false;
               re.Msg = Errors.Task.PARENT_ONLY_ALLOW_DELAY_WHEN_PROJECT_START;
            }
         }
         else if (option.Tasks.Count > 0)
         {
            var parent = option.Tasks.Find(tk => tk.TaskId == task.ParentId);
            if (parent != null && !parent.IsProjectTypeOnly && parent.IsWorked)
            {
               re.IsSuccess = false;
               re.Msg = Errors.Task.NOT_ALLOWED_BE_PARNET_TYPE_IF_LEAF_TASK;
            }

            if (parent != null && parent.IsPlanTask)
            {
               re.IsSuccess = false;
               re.Msg = Errors.Task.NOT_ALLOWED_HAS_CHILD_IF_PARENT_IS_PLANTASK;
            }
         }

         return re;
      }


      protected override void WhenPlan(EditContext ctx) { }


      protected override void WhenStart(EditContext ctx)
      {
         var pj = ctx.Project;
         var re = ctx.Result;

         //启动任务策略：1 项目必须启动
         if (!(pj.IsProcessStatus || pj.IsEditStatus || pj.IsReviewStatus))
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.NOT_ALLOWED_START_DUE_TO_PROJECT_NOT_START;
            return;
         }


         base.WhenStart(ctx);
      }


      protected override void WhenComplete(EditContext ctx)
      {
         base.WhenComplete(ctx);
      }


      protected override void WhenDelete(EditContext ctx)
      {
         //删除策略：逻辑删除，找出某一个删除任务的所有子任务，设置为删除状态（单独放在一个集合，从所有任务集合中删除）
         //同时改变未删后所节点的sortId
         var db = ctx.db;
         var re = ctx.Result;
         var allTks = ctx.AllTasks;
         var delTk = ctx.Task;

         if (delTk.ParentId.IsEmpty())
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.NOT_ALLOWED_DELETE_IF_ROOT;
            return;
         }

         if (delTk.IsWorked)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.NOT_ALLOWED_DELETE_IF_WORKHOURS_IS_NOT_ZERO;
            return;
         }

         var delIndex = allTks.FindIndex(tk => tk.TaskId == delTk.TaskId);
         ctx.DeletedTasks = TaskHelper.GetAllChildren(ctx.Task, allTks, true);

         //将标记为删除的任务移出任务集合
         allTks.RemoveRange(delIndex, ctx.DeletedTasks.Count);

         //调整sortId
         var i = 1;
         allTks.ForEach(tk =>
         {
            tk.SortId = i;
            i++;
         });

         //设置所有删除节点为删除状态
         ctx.DeletedTasks.ForEach(tk =>
         {
            db.WorkTaskDal.UpdatePartial(tk.TaskId, new { TaskStatus = TaskKeys.DeleteStatus });
         });

         //只有全部删除状态更新成功后方能更改内存数据
         ctx.DeletedTasks.ForEach(t => t.SetStatus(TaskKeys.DeleteStatus));

      }


      private void SetTaskAndProjectProcess(APDBDef db, List<WorkTask> tks)
      {
         TaskHelper.SetTasksProcessRate(tks, tks.FirstOrDefault());

         foreach (var item in tks)
         {
            if (item.IsParent)
            {
               item.SetParentProgress();
            }

            db.WorkTaskDal.Update(item);
         }

      }

   }

   public class TempTaskEditHandler : TaskEditHandler
   {

      APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;

      public TempTaskEditHandler() : base() { }


      public override void Handle(WorkTask task, TaskEditOption option)
      {
         var db = option.db;
         var re = option.Result;

         var taskIndb = db.WorkTaskDal.PrimaryGet(task.TaskId);

         if (taskIndb != null && !taskIndb.Projectid.IsEmpty())
         {
            re.IsSuccess = false;
            re.Msg = "已绑定的项目任务无法更换为临时任务!";
            option.Result = re;

            return;
         }

         ClearTask(task);

         var ctx = new EditContext { Task = task, db = db, Result = re };
         if (EditStrategy.ContainsKey(task.TaskStatus))
            EditStrategy[task.TaskStatus].Invoke(ctx);

         if (ctx.Result.IsSuccess)
            if (taskIndb != null)
               db.WorkTaskDal.Update(task);
            else
               db.WorkTaskDal.Insert(task);

         UploadAttachment(task.CurrentAttachment, task, db);

         WorkJournalHelper.CreateOrUpdateJournalByTask(task, db);

         TaskLogHelper.CreateLog(task, option.OperatorId, db);

         option.Result = re;
      }


      protected override void WhenPlan(EditContext ctx)
      {
         ctx.Task.TaskStatus = TaskKeys.PlanStatus;
      }


      protected override void WhenStart(EditContext ctx)
      {
         base.WhenStart(ctx);
      }


      protected override void WhenComplete(EditContext ctx)
      {
         ctx.Task.TaskStatus = TaskKeys.CompleteStatus;

         base.WhenComplete(ctx);
      }


      protected override void WhenDelete(EditContext ctx)
      {
         base.WhenDelete(ctx);
      }


      private void ClearTask(WorkTask task)
      {
         task.Projectid = Guid.Empty;
         task.SortId = 0;
         task.TaskLevel = 0;
         task.ParentId = Guid.Empty;
         task.ParentTaskName = null;
         task.ServiceCount = 0;
      }

   }

   public class LeafTaskEditHandler : TaskEditHandler
   {
      TaskEditHandler _innderHandler;

      public LeafTaskEditHandler(TaskEditHandler handler) : base()
      {
         _innderHandler = handler;
      }

      public override void Handle(WorkTask task, TaskEditOption option)
      {
         var originalTask = option.Original;

         // 叶子任务无法成为根任务
         if (task.IsRoot)
         {
            option.Result.IsSuccess = false;
            option.Result.Msg = Errors.Task.ROOT_TASK_CANNOT_BE_LEAF;
            return;
         }

         // 叶子任务一旦启动，记录过子任务工作数量的无法修改成其他子任务
         if (originalTask != null &&
            task.SubTypeId != originalTask.SubTypeId &&
            task.IsWorked)
         {
            option.Result.IsSuccess = false;
            option.Result.Msg = Errors.Task.LEAF_TASK_CANNOT_BE_CHANGE;
            return;
         }

         if (originalTask != null && originalTask.IsPlanTask && !task.IsPlanTask && !task.IsPlanStatus)
         {
            option.Result.IsSuccess = false;
            option.Result.Msg = Errors.Task.NOT_ALLOWED_CHANGE_IF_IS_PLANTASK_IS_PROCESS;
            return;
         }

         // 叶子任务不能成为父任务,一旦成为父任务，就变为项目任务
         var tasks = option.Tasks;
         if (tasks != null && tasks.Count > 0)
         {
            var parent = option.Tasks.Find(tk => tk.TaskId == task.ParentId);
            if (parent != null)
               parent.TaskType = TaskKeys.ProjectTaskType;
         }

         if (_innderHandler != null)
            _innderHandler.Handle(task, option);
      }
   }

   public class ManagementTaskEditHandler : TempTaskEditHandler { }

   public class ProjectNodeTaskEditHandler: TaskEditHandler
   {

      public override void Handle(WorkTask task, TaskEditOption option)
      {
         base.Handle(task, option);
      }

      private void ClearTask(WorkTask task)
      {
         task.SortId = 0;
         task.TaskLevel = 0;
         task.ParentId = Guid.Empty;
         task.ParentTaskName = null;
         task.ServiceCount = 0;
      }

   }



   public class TaskSearchOption : SearchOption
   {

      public Guid SearchType { get; set; } = TaskKeys.SearchByProject;

      public Guid TaskId { get; set; }

      public Guid ProjectId { get; set; } = TaskKeys.SelectAll;

      public Guid UserId { get; set; }

      public string DataUrl { get; set; }

      public Guid Status { get; set; } = TaskKeys.SelectAll;

      public Guid Type { get; set; } = TaskKeys.SelectAll;

      public Guid RangeType { get; set; }

      public bool IsShowParent { get; set; } = true;

      public string TaskNamePhrase { get; set; }

      public int Level { get; set; } //层级

   }

   public class TaskEditOption
   {

      public APDBDef db { get; set; }

      public Result Result { get; set; }

      public List<WorkTask> Tasks { get; set; }

      public Guid OperatorId { get; set; }

      public Project Project { get; set; }

      public WorkTask Original { get; set; }

   }

}
