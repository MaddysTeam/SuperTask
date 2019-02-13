using Business;
using Business.Config;
using Business.Helper;
using Business.Security;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class TaskController : BaseController
   {

      APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;


      //GET  /Task/Index

      public ActionResult Index(int? taskType, int? taskStatus)
      {
         var p = APDBDef.Project;

         ViewBag.Projects = MyJoinedProjects();


         return View();
      }


      //POST-Ajax  /Task/List

      [HttpPost]
      public ActionResult List(TaskSearchOption option)
      {
         ThrowNotAjax();

         var ma = APDBDef.UserInfo.As("M");//负责人
         var cr = APDBDef.UserInfo.As("C"); //创建人
        // var o = APDBDef.Organize;
         var p = APDBDef.Project;
         var wj = APDBDef.WorkJournal;

         if (option.UserId.IsEmpty())
            option.UserId = GetUserInfo().UserId;

         var query = APQuery.select(t.TaskId, t.TaskName, t.TaskType, t.StartDate, t.TaskStatus, t.EndDate, t.RateOfProgress, t.WorkHours, p.ProjectId, p.ProjectName.As("projectName"), ma.UserId, ma.UserName,
                                    cr.UserId, cr.UserName.As("Creator"), wj.WorkHours.Sum().As("TaskTotalHours"))
            .from(t,
                  wj.JoinLeft(wj.TaskId == t.TaskId),
                  p.JoinLeft(t.Projectid == p.ProjectId),
                  ma.JoinLeft(t.ManagerId == ma.UserId),
                  cr.JoinLeft(t.CreatorId == cr.UserId)
                  )
                  .group_by(t.TaskId, t.TaskName, t.TaskType, t.StartDate, t.TaskStatus, t.EndDate, t.RateOfProgress, t.WorkHours, p.ProjectId, p.ProjectName, ma.UserId, ma.UserName,
                                    cr.UserId, cr.UserName, t.CreateDate)
                  .order_by(t.CreateDate.Desc);

         if (HandleManager.TaskSearchHandlers.ContainsKey(option.SearchType))
         {
            HandleManager.TaskSearchHandlers[option.SearchType].Handle(query, option);
         }

         var models = query.query(db, r =>
         {
            var workhours = r["TaskTotalHours"] is DBNull ? 0 : Convert.ToDouble(r["TaskTotalHours"]);
            return new WorkTask
            {
               TaskId = t.TaskId.GetValue(r),
               TaskName = t.TaskName.GetValue(r),
               StartDate = t.StartDate.GetValue(r),
               EndDate = t.EndDate.GetValue(r),
               Manager = ma.UserName.GetValue(r),
               Creator = cr.UserName.GetValue(r, "Creator"),
               DataUrl = Url.Action("Details", "Task", new { taskId = t.TaskId.GetValue(r) }),
               TaskStatus = t.TaskStatus.GetValue(r),
               RateOfProgress = t.RateOfProgress.GetValue(r),
               ProjectName = p.ProjectName.GetValue(r, "projectName"),
               TaskType = t.TaskType.GetValue(r),
               WorkHours = workhours
            };
         }).ToList();


         if (option.ReturnJson)
         {
            return Json(new
            {
               result = models,
            });
         }
         else
         {
            return PartialView("_list", models);
         }
      }


      //GET  /Task/Edit
      //POST-Ajax  /Task/Edit
      //POST-Ajax  /Task/MultiEdit

      public ActionResult Edit()
      {
         var st = APDBDef.TaskStandardItem;
         var ac = APDBDef.Account;

         ViewBag.Projects = MyJoinedProjects();

         ViewBag.Resource = db.AccountDal.ConditionQuery(ac.Status == 0, null, null, null); //没有被禁用的所有人员

         ViewBag.Me = GetUserInfo();

         var standardItems = db.TaskStandardItemDal.ConditionQuery(null, st.ItemName.Desc, null, null);
         foreach (var item in standardItems)
         {
            item.ItemName = string.IsNullOrEmpty(item.ItemDescription) ?
               item.ItemName : $"{item.ItemName}({item.ItemDescription})";
         }

         ViewBag.StandardItems = standardItems;

         var task = WorkTask.Create(
            GetUserInfo().UserId,
            DateTime.Now.GetNextMondayIfIsWeekend(),
            DateTime.Now.GetNextMondayIfIsWeekend().AddDays(1),
            TaskKeys.PlanStatus,
            TaskKeys.ProjectTaskType
            );

         return PartialView(task);
      }

      [HttpPost]
      public JsonResult Edit(WorkTask task)
      {
         var rs = task.Validate();
         if (!rs.IsSuccess)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = rs.Msg
            });
         }

         var editOption = GetEditOption(task, rs);

         if (HandleManager.TaskEditHandlers.ContainsKey(task.TaskType))
            HandleManager.TaskEditHandlers[task.TaskType].Handle(task, editOption);


         return Json(new
         {
            result = editOption.Result.IsSuccess ? AjaxResults.Success : AjaxResults.Error,
            msg = editOption.Result.Msg
         });
      }

      //群发任务，任务属性相同，任务执行人不同

      [HttpPost]
      public ActionResult MultiSend(WorkTask task, string managers)
      {
         var rs = task.Validate();
         if (string.IsNullOrEmpty(managers))
            rs = new Result { IsSuccess = false, Msg = Errors.Task.NOT_ALLOWED_MANAGER_NULL };
         if (!rs.IsSuccess)
            return Json(new
            {
               result = AjaxResults.Error,
               msg = rs.Msg
            });

         var tasks = new List<WorkTask> { task };
         var editOption = GetEditOption(task, rs);
         var managerIds = managers.Split(',').ToList();

         foreach (var item in managerIds)
         {
            var t = tasks.DeepClone().First();
            t.TaskId = Guid.NewGuid();
            t.ManagerId = item.ToGuid(Guid.Empty);
            if (t.ManagerId.IsEmpty())
               continue;

            if (HandleManager.TaskEditHandlers.ContainsKey(t.TaskType))
               HandleManager.TaskEditHandlers[t.TaskType].Handle(t, editOption);
         }


         return Json(new
         {
            result = rs.IsSuccess ? AjaxResults.Success : AjaxResults.Error,
            msg = rs.Msg
         });
      }


      //POST-Ajax  /Task/GetSubTaskTypes
      //POST-Ajax  /Task/GetSubTaskTypes TODO: 以后改为全部缓存读取，不走服务器

      [HttpPost]
      public ActionResult GetSubTaskTypes(Guid taskTypeId)
      {
         var subTaskTypes = DictionaryHelper.GetSubTypeDics(taskTypeId);
         if (subTaskTypes != null && subTaskTypes.Count > 0)
         {
            return Json(new
            {
               result = AjaxResults.Success,
               data = subTaskTypes.Select(x => new
               {
                  Value = x.ID.ToString(),
                  Text = string.IsNullOrEmpty(x.Note) ? x.Title : $"{x.Title} [{x.Note}]",
                  UnitName = x.Other,
                  SortId = x.Sort
               }).OrderBy(x => x.SortId) //TODO:会修改暂时按照sortId 排序
            });
         }
         else
            return Json(new
            {
               result = AjaxResults.Error
            });
      }

      //POST-Ajax  /Task/Delete

      [HttpPost]
      public ActionResult Delete(Guid id)
      {
         var delTk = db.WorkTaskDal.PrimaryGet(id);
         if (delTk != null)
            delTk.SetStatus(TaskKeys.DeleteStatus);

         return Edit(delTk);
      }


      //POST-Ajax  /Task/Details
      //POST-Ajax  /Task/GetMyTasks

      [HttpPost]
      public ActionResult Details(Guid? taskId)
      {
         var ma = APDBDef.UserInfo.As("M");//负责人
         var cr = APDBDef.UserInfo.As("C"); //创建人
         var rr = APDBDef.UserInfo.As("R");//审核人
         var a = APDBDef.Attachment;
         var p = APDBDef.Project;
         var st = APDBDef.TaskStandardItem; //TODO:已停用，以后删除
         var ac = APDBDef.Account; //TODO: 资源逻辑以后修改
         var d = APDBDef.Dictionary;

         var task = APQuery.select(t.Asterisk, ma.UserId, ma.UserName, p.ProjectName.As("ProejectName"),
                                    cr.UserId, cr.UserName.As("Creator"),
                                    rr.UserName.As("Reviewer"),
                                    d.Other, d.Title, d.Code)
            .from(t,
                  p.JoinLeft(t.Projectid == p.ProjectId),
                  ma.JoinLeft(t.ManagerId == ma.UserId),
                  cr.JoinLeft(t.CreatorId == cr.UserId),
                  rr.JoinLeft(t.ReviewerID == rr.UserId),
                  d.JoinLeft(d.ID == t.SubTypeId)
                  )
            .where(t.TaskId == taskId)
            .query(db, r =>
            {
               var subTaskTypeUnitName = d.Other.GetValue(r) == null ? "" : d.Other.GetValue(r);
               var subTaskTypeName = d.Title.GetValue(r) == null ? "" : d.Title.GetValue(r);
               var score = t.SubTypeValue.GetValue(r) * Convert.ToDouble((d.Code.GetValue(r)));
               var ta = new WorkTask();
               t.Fullup(r, ta, false);
               ta.Creator = cr.UserName.GetValue(r, "Creator");
               ta.Manager = ma.UserName.GetValue(r);
               ta.ProjectName = p.ProjectName.GetValue(r, "ProejectName").Ellipsis(TaskKeys.TaskNameDisplayLength);
               ta.Reviewer = rr.UserName.GetValue(r, "Reviewer");
               ta.SubTaskTypeUnitName = subTaskTypeUnitName;
               ta.SubTaskTypeName = subTaskTypeName;
               ta.SubTaskScore = score;
               return ta;
            }).FirstOrDefault();


         //获取当前用户参与的项目

         ViewBag.Projects = MyJoinedProjects();

         ViewBag.Resource = db.AccountDal.ConditionQuery(ac.Status == 0, null, null, null); //可用资源

         ViewBag.Attachments = db.AttachmentDal.ConditionQuery(a.TaskId == task.TaskId, null, null, null);

         ViewBag.StandardItems = db.TaskStandardItemDal.ConditionQuery(null, st.SortId.Asc, null, null);


         //如果任务不是新任务，则获取父任务名
         var parentTk = db.WorkTaskDal.PrimaryGet(task.ParentId);
         if (!task.ParentId.IsEmpty() && parentTk != null)
            task.ParentTaskName = db.WorkTaskDal.PrimaryGet(task.ParentId).TaskName;
         else
            task.ParentTaskName = task.TaskName;

         return PartialView("_details", task);
      }

      [HttpPost]
      public ActionResult GetMyTasks(Guid projectId)
      {
         var userid = GetUserInfo().UserId;
         var tasks = TaskHelper.GetProjectUserTasks(projectId, userid, db)
             .Select(x => new SelectListItem
             {
                Value = x.TaskId.ToString(),
                Text = x.TaskName
             }).ToList();

         return Json(new
         {
            result = AjaxResults.Success,
            data = tasks
         });
      }


      //POST-Ajax  /Task/TaskArrangement
      //POST-Ajax  /Task/EditArrangement

      [HttpPost]
      public ActionResult TaskArrangement(TaskSearchOption option)
      {
         var re = APDBDef.Resource;
         var user = GetUserInfo();

         WorkTask rootTask = null;
         var taskTypes = DictionaryHelper.GetAll();
         var previewTasks = Session["previewTasks"];
         var list = previewTasks != null ? previewTasks as List<WorkTask>
                                : db.WorkTaskDal.ConditionQuery(t.Projectid == option.ProjectId
                                                             & t.TaskStatus != TaskKeys.DeleteStatus, t.SortId.Asc, null, null);
         if (list.Count() <= 0)
         {
            var project = ProjectrHelper.GetCurrentProject(option.ProjectId);
            rootTask = TaskHelper.CreateAndSaveRootTaskInDB(user, project, db);
            return Json(new
            {
               tasks = GetTaskViewModels(list, rootTask),
               taskTypes = taskTypes == null ? null : taskTypes.OrderBy(x => x.Sort)
            });
         }

         var resource = ResourceHelper.GetCurrentProjectResource(option.ProjectId, user.UserId, false, db);
         if (resource != null && resource.IsLeader())
            rootTask = list.OrderBy(tk => tk.SortId).First();
         else
            rootTask = list.Find(x => x.TaskId == option.TaskId);

         var tasks = GetTaskViewModels(list, rootTask);

         return Json(new
         {
            tasks,
            taskTypes = taskTypes == null ? null : taskTypes.OrderBy(x => x.Sort)
         });
      }

      [HttpPost]
      public ActionResult EditArrangement(List<ArrangeTaskViewModel> tasks, List<string> delTaskIds)
      {
         delTaskIds = delTaskIds ?? new List<string>();

         if (tasks == null || tasks.Count <= 0)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Task.NOT_EXIST
            });
         }

         //绑定每个节点的父ID,有可能有新的节点通过甘特图创建
         BindParentId(tasks);

         if (tasks.Exists(tk => SecurityScenario.SpecialCharChecker.HasSpecialChar(tk.name)))
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Task.NOT_ALLOWED_SEPCIAL_CHAR
            });


         var minLevel = tasks.Min(tk => tk.level);
         var minLevelCount = tasks.FindAll(tk => tk.level <= minLevel).Count;
         if (minLevelCount > 1)
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Task.NOT_ALLOWED_MULITE_TASKS
            });


         //获取项目任务列表数据，找到当前第一个任务在任务列表的索引
         var topTask = tasks.First();
         var pjTasks = TaskHelper.GetProjectTasks(topTask.projectId.ToGuid(Guid.Empty), db);
         var orignalTasks = pjTasks.DeepClone();//用于最后判断某个任务是否修改过
         var topTaskIndex = pjTasks.FindIndex(x => x.TaskId.ToString() == topTask.id);
         topTaskIndex = topTaskIndex < 0 ? 0 : topTaskIndex;

         if (pjTasks[topTaskIndex].TaskLevel != topTask.level)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Task.ROOT_TASK_POSITION_NOT_ALLOW_CHANGE
            });
         }

         if (tasks.Min(tk => tk.level) < pjTasks[topTaskIndex].TaskLevel)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.Task.TASK_POSITION_ERROR
            });
         }

         foreach (var item in tasks)
         {
            var pjTask = pjTasks.Find(tk => tk.TaskId == item.id.ToGuid(Guid.Empty));
            if (pjTask == null) continue;
            if (pjTask.TaskId == item.id.ToGuid(Guid.Empty)
                                    && pjTask.ParentId != item.parentId.ToGuid(Guid.Empty)
                                    && pjTask.IsWorked)
            {
               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.Task.NOT_ALLOWED_CHANGE_PARENT
               });
            }

            if (pjTask.IsWorked
                && (pjTask.TaskType != item.taskType.ToGuid(Guid.Empty) || pjTask.SubTypeId != item.subType.ToGuid(Guid.Empty)
                ))
            {
               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.Task.NOT_ALLOWED_CHANGE_TYPE_IF_HAS_WORK
               });
            }

            if (pjTask.IsPlanTask && !pjTask.IsPlanStatus && !pjTask.IsEqualsWithViewModel(item))
            {
               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.Task.NOT_ALLOWED_CHANGE_IF_IS_NOT_PLAN_STATUS
               });
            }
         }

         foreach (var item in delTaskIds)
         {
            if (pjTasks.Exists(pjTask => pjTask.TaskId == item.ToGuid(Guid.Empty)
                                     && pjTask.IsWorked))
               return Json(new
               {
                  result = AjaxResults.Error,
                  msg = Errors.Task.NOT_ALLOWED_DELETE_IF_HAS_WORKHOUR
               });
         }

         //调整节点位置
         var temp = new List<WorkTask>();
         for (int i = 1; i < tasks.Count + 1; i++)
         {
            var workTask = Mapper(tasks[i - 1], orignalTasks);
            temp.Add(workTask);
            pjTasks.RemoveAll(x => x.TaskId == workTask.TaskId);
         }

         if (pjTasks.Count == 0)
         {
            topTaskIndex = 0;
         }

         pjTasks.InsertRange(topTaskIndex, temp);


         //将被删除的任务移出任务列表
         var removedList = TaskHelper.RemoveDelTasks(delTaskIds, pjTasks, true);

         //重新计算任务进度和预估工时
         TaskHelper.SetTasksProcessRate(pjTasks, pjTasks.First());


         //如果是预览状态
         if (topTask.isPreview)
         {
            Session["previewTasks"] = pjTasks;

            //父任务进度赋值,注意子任务没有赋值意义
            pjTasks.ForEach(item => item.SetParentProgress());

            return Json(new
            {
               result = AjaxResults.Success,
               msg = "显示预览"
            });
         }

         if (Session["previewTasks"] != null)
            Session.Remove("previewTasks");

         var projectId = tasks[0].projectId.ToGuid(Guid.Empty);
         var project = ProjectrHelper.GetCurrentProject(projectId, db, true);


         db.BeginTrans();

         try
         {
            //删除项目任务数据
            db.WorkTaskDal.ConditionDelete(t.Projectid == projectId & t.TaskStatus != TaskKeys.DeleteStatus);

            //将逻辑删除的任务添加回数据库
            removedList.ForEach(tk => db.WorkTaskDal.Insert(tk));

            //重新添加项目任务数据，根据pjTasks各个任务顺序重新规整SortId
            var idx = 1;
            foreach (var tk in pjTasks)
            {
               tk.SortId = idx;
               //父任务进度赋值,注意子任务没有赋值意义
               tk.SetParentProgress();

               //如果项目已启动，任务自动启动 TODO：部门一些人要求更改
               if (project.IsProcessStatus && tk.IsPlanStatus && tk.EstimateWorkHours > 0)
                  tk.Start();

               db.WorkTaskDal.Insert(tk);

               var originTask = orignalTasks.Find(x => x.TaskId == tk.TaskId);
               if (originTask == null || !originTask.Equals(tk))
                  WorkJournalHelper.CreateOrUpdateJournalByTask(tk, db);

               idx++;
            }

            TaskLogHelper.CreateLogs(pjTasks, orignalTasks, GetUserInfo().UserId, db);
            TaskLogHelper.CreateLogs(removedList, GetUserInfo().UserId, db);

            //通过根任务更新项目进度
            db.ProjectDal.UpdatePartial(projectId, new { RateOfProgress = pjTasks[0].RateOfProgress, StartDate = pjTasks[0].StartDate, EndDate = pjTasks[0].EndDate });

         }
         catch (Exception e)
         {
            db.Rollback();

            return Json(new
            {
               result = AjaxResults.Success,
               msg = "编辑失败，请联系管理员，原因：" + e.InnerException
            });
         }


         db.Commit();


         return Json(new
         {
            result = AjaxResults.Success,
            msg = "编辑成功"
         });

      }


      // GET: Task/PlanTaskList
      //	POST-Ajax: Task/PlanTaskList
      // GET: TaskManage/PlanTaskEdit
      // Post: TaskManage/PlanTaskStart
      // Post: TaskManage/PlanTaskDeny

      public ActionResult PlanTaskList()
      {
         var ac = APDBDef.Account; //TODO: 资源逻辑以后修改

         ViewBag.Projects = MyJoinedProjects();

         ViewBag.Resources = db.AccountDal.ConditionQuery(ac.Status == 0, null, null, null); //可用资源

         return View();
      }

      [HttpPost]
      public ActionResult PlanTaskList(Guid projectId, Guid resourceId, DateTime start, DateTime end, Guid taskTypeId,
                                      int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var user = GetUserInfo();

         var sql = $@"
                  
                  select 
                  id,
                  name,
                  projectId,
                  project,
                  reviewerId,
                  start,
                  [end],
                  upgradeEndDate,
                  statusId,
                  isParent,
                  realEnd,
                  taskTypeId,
                  manager,
                  managerId      
                   from (
                  select
                   t.id,
                   t.ProjectId as 'projectId',
                   t.name,
                   t.ReviewerId,
                   t.StartDate as start,
                   t.EndDate as [end],
                    null as upgradeEndDate,
                   t.Status as statusId,
                   t.IsParent,
                   t.ManagerId,
                   t.RealEndDate as 'realEnd',
                   t.Type as 'taskTypeId',
                   u.username as 'manager',
                   u.id as 'mangerId',
                   p.name as project
                   from worktask t
                   join UserInfo u on t.ManagerId=u.ID
                   join Project p on t.ProjectId= p.ID
                   where t.Type = @PlanTaskType
                   --and t.managerId= @ManagerId

                   union all

                   select
                   t.id,
                   t.ProjectId as 'projectId',
                   t.name,
                   t.ReviewerId,
                   t.StartDate,
                   t.EndDate,
                   t.upgradeEndDate,
                   t.Status, 
                   0,
                   p.ManagerId,
                   t.RealEndDate,
                   @NodeTaskType,
                   u.username as 'manager',
                   u.id,
                   p.name
                   from ProjectStoneTask t
                   join UserInfo u on t.managerId=u.ID
                   join Project p on t.ProjectId= p.ID
                   --where t.managerId=@ManagerId
                  )
                  s
                  where s.start>= @StartDate and s.[end] <=@EndDate
          ";

         var builder = new System.Text.StringBuilder(sql);

         if (!projectId.IsEmpty() && projectId != AppKeys.SelectAll)
         {
            builder.Append($" and  s.projectId = @ProjectId ");
         }

         if (!taskTypeId.IsEmpty() && taskTypeId != AppKeys.SelectAll)
         {
            builder.Append($" and  s.taskTypeId = @TaskTypeId ");
         }

         // 如果是易佳,则可以查看所有项目经理的计划和节点任务
         if (user.UserId == ResourceKeys.TempBossId)
         {
            builder.Append($" and  s.statusId <> @PlanStatusId ");

            if (!resourceId.IsEmpty() && resourceId != TaskKeys.SelectAll)
               builder.Append($" and  s.ManagerId= @ResouceId ");
         }
         else
         {
            builder.Append($" and  s.ManagerId= @ManagerId ");
         }
   
         //过滤条件
         //模糊搜索用户名、实名进行

         searchPhrase = searchPhrase.Trim();
         if (searchPhrase != "")
         {
            builder.Append($" and s.name like '%"+ searchPhrase + "%' or s.project like '%"+ searchPhrase + "%' ");
         }


         //排序条件表达式

         if (sort != null)
         {
            var according = sort.According == APSqlOrderAccording.Asc ? "Asc" : "Desc";

            switch (sort.ID)
            {
               case "name": builder.Append($" order by s.name {according}"); break;
               case "project": builder.Append($" order by s.project  {according}"); break;
               case "manager": builder.Append($" order by s.manager  {according}"); break;
               //case "taskType": builder.Append($" order by s.taskType {according}"); break;
               case "start": builder.Append($" order by s.start {according}"); break;
               case "end": builder.Append($" order by s.[end] {according} "); break;
               default: builder.Append($" order by s.start desc "); break;
            }
         }
         else
         {
            builder.Append($" order by s.[end] desc ");
         }

         var result = DapperHelper.QueryBySQL<PlanAndNodeTaskViewModel>(builder.ToString(), new
         {
            StartDate = start,
            EndDate = end,
            ManagerId = user.UserId,
            PlanTaskType = TaskKeys.PlanTaskTaskType,
            NodeTaskType = TaskKeys.NodeTaskType,
            ProjectId = projectId,
            PlanStatusId = TaskKeys.PlanStatus,
            ResouceId = resourceId,
            TaskTypeId = taskTypeId
         });

         var total = result.Count;

         foreach (var item in result)
         {
            item.status = TaskKeys.GetStatusKeyByValue(item.statusId);
            item.taskType =  item.taskTypeId==TaskKeys.NodeTaskType?
                                                      TaskKeys.TempNodeTaskName : //TODO: 由于不想修改原逻辑，所以暂时把节点任务与一般任务脱离
                                                      TaskKeys.GetTypeKeyByValue(item.taskTypeId);
            item.isMe = user.UserId == item.managerId;
            item.reviewerIsMe = user.UserId == item.reviewerId;
            item.realEndString = item.realEnd.IsEmpty() ? "-" : item.realEnd.ToString("yyyy-MM-dd");
            item.upgradeString = item.UpgradeEndDate.IsEmpty() ? "-" : item.UpgradeEndDate.ToString("yyyy-MM-dd");
         }

         result = result.Skip(rowCount * (current - 1)).Take(rowCount).ToList();

         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });
      }

      public ActionResult PlanTaskEdit(Guid id)
      {
         var task = db.WorkTaskDal.PrimaryGet(id);

         return PartialView(task);
      }

      [HttpPost]
      public ActionResult PlanTaskStart(Guid id)
      {
         var task = db.WorkTaskDal.PrimaryGet(id);
         task.SetStatus(TaskKeys.ProcessStatus);

         return Edit(task);
      }

      [HttpPost]
      public ActionResult PlanTaskDeny(Guid id)
      {
         var task = db.WorkTaskDal.PrimaryGet(id);
         task.SetStatus(TaskKeys.PlanStatus);

         return Edit(task);
      }


      //GET  /Task/ReviewRequest
      //GET  /Task/AfterEditReview
      //GET  /Task/AfterSubmitReview
      //GET  /Task/AfterEditReviewSend
      //GET  /Task/AfterReviewFail


      public ActionResult ReviewRequest(Guid id, Guid reviewType)
      {
         if (id.IsEmpty())
            throw new ArgumentException(Errors.Task.NOT_ALLOWED_ID_NULL);

         var task = db.WorkTaskDal.PrimaryGet(id);
         if (task == null)
            throw new ArgumentException(Errors.Task.NOT_EXIST);

         var project = db.ProjectDal.PrimaryGet(task.Projectid);

         var requestOption = new ReviewRequestOption
         {
            Project = project,
            User = GetUserInfo(),
            ReviewType = reviewType,
            db = db,
            Result = Result.Initial()
         };


         HandleManager.TaskReviewRequestHandlers[reviewType].Handle(task, requestOption);

         if (!requestOption.Result.IsSuccess)
            throw new ApplicationException(requestOption.Result.Msg);


         return RedirectToAction("FlowIndex", "WorkFlowRun", requestOption.RunParams);
      }

      public ActionResult AfterEditReview(Guid instanceId)
      {
         if (instanceId.IsEmpty())
            throw new NullReferenceException("instance id 不能为空！");

         var review = db.ReviewDal.PrimaryGet(instanceId);
         var task = db.WorkTaskDal.PrimaryGet(review.TaskId);

         //判断流程是否结束


         //设置任务为临时编辑状态
         task.SetStatus(TaskKeys.TaskTempEditStatus);
         db.WorkTaskDal.Update(task);


         //重定向到首页
         // return RedirectToAction("Search", "WorkFlowTask");

         return Content("<script>window.location.href='about:blank'; window.close();</script>");
      }

      public ActionResult AfterSubmitReview(Guid instanceId)
      {
         if (instanceId.IsEmpty())
            throw new NullReferenceException("instance id 不能为空！");

         //找其任务提交审核的第一条记录，将任务结束时间设置成第一次提交任务的时间
         var review = ReviewHelper.GetEarlistReview(db, instanceId);
         var task = db.WorkTaskDal.PrimaryGet(review.TaskId);
         task.Complete(review.SendDate < task.StartDate ? task.EndDate : review.SendDate);

         //执行完成后的事件
         var editOption = new TaskEditOption { db = db, Result = Result.Initial() };
         HandleManager.TaskEditHandlers[task.TaskType].Handle(task, editOption);


         //重定向到首页
         // return RedirectToAction("Search", "WorkFlowTask");
         return Content("<script>window.location.href='about:blank'; window.close();</script>");
      }

      public ActionResult AfterEditReviewSend(Guid instanceId)
      {
         if (instanceId.IsEmpty())
            throw new NullReferenceException("instance id 不能为空！");

         var review = db.ReviewDal.PrimaryGet(instanceId);
         var task = db.WorkTaskDal.PrimaryGet(review.TaskId);

         task.SetStatus(TaskKeys.ReviewStatus);

         db.WorkTaskDal.Update(task);


         //重定向到首页
         return RedirectToAction("Search", "WorkFlowTask");
      }

      public ActionResult AfterSubmitReviewSend(Guid instanceId)
      {
         if (instanceId.IsEmpty())
            throw new NullReferenceException("instance id 不能为空！");

         var review = db.ReviewDal.PrimaryGet(instanceId);
         var task = db.WorkTaskDal.PrimaryGet(review.TaskId);

         task.SetStatus(TaskKeys.ReviewStatus);

         db.WorkTaskDal.Update(task);


         //重定向到首页
         return RedirectToAction("Search", "WorkFlowTask");
      }

      public ActionResult AfterReviewFail(Guid instanceId)
      {
         var review = db.ReviewDal.PrimaryGet(instanceId);
         var task = db.WorkTaskDal.PrimaryGet(review.TaskId);

         task.SetStatus(TaskKeys.ProcessStatus);

         db.WorkTaskDal.Update(task);

         //重定向到首页
         //return RedirectToAction("Search", "WorkFlowTask");

         //关闭页面
         return Content("<script>window.location.href='about:blank'; window.close();</script>");
      }


      #region  [Helper]


      private WorkTask Mapper(ArrangeTaskViewModel vm, List<WorkTask> orignalTasks)
      {
         var uid = GetUserInfo().UserId;
         var originalTk = orignalTasks.Find(t => t.TaskId == vm.id.ToGuid(Guid.Empty));
         var status = vm.stat.ToGuid(Guid.Empty) == Guid.Empty ? TaskKeys.PlanStatus : vm.stat.ToGuid(Guid.Empty);
         var tk = new WorkTask
         {
            TaskId = vm.id.ToGuid(Guid.Empty),
            ManagerId = vm.executorId.ToGuid(uid),
            CreatorId = vm.createrId.ToGuid(uid),
            ReviewerID = vm.reviewerId.ToGuid(uid),
            Projectid = vm.projectId.ToGuid(Guid.Empty),
            TaskLevel = vm.level,
            TaskName = vm.name,
            EstimateWorkHours = vm.hours <= 0 ? TaskKeys.DefaultEstimateHours : vm.hours,
            StartDate = DateTime.Parse(vm.start),
            EndDate = DateTime.Parse(vm.end),
            SortId = vm.sortId,
            ParentId = vm.parentId.ToGuid(Guid.Empty),
            RateOfProgress = originalTk == null ? vm.progress : originalTk.RateOfProgress,
            WorkHours = originalTk == null ? vm.workhours : originalTk.WorkHours,
            Description = vm.description,
            TaskType = GetTaskTypeID(vm), // 获取任务类型
            ServiceCount = vm.serviceCount,   //运维任务逻辑添加
            TaskStatus = status,
            SubTypeId = vm.subType.ToGuid(Guid.Empty), // 获取任务子类型
            SubTypeValue = vm.subTypeValue,// 获取任务子类型工作量
            RealStartDate = string.IsNullOrEmpty(vm.realStart) ? DateTime.MinValue : DateTime.Parse(vm.realStart),
            RealEndDate = string.IsNullOrEmpty(vm.realEnd) ? DateTime.MinValue : DateTime.Parse(vm.realEnd),
         };

         return tk;
      }

      private Guid GetTaskTypeID(ArrangeTaskViewModel vm)
      {
         var taskType = vm.taskType.ToGuid(Guid.Empty);
         var isParent = vm.IsParent;
         var isDefault = taskType == TaskKeys.DefaultType;
         var hasSubType = WorkTask.HasSubTypeTask(taskType);
         if (isDefault || isParent || taskType.IsEmpty())
         {
            if (!hasSubType)
               return taskType;

            return TaskKeys.ProjectTaskType;
         }

         return taskType;
      }


      private ArrangeTaskViewModel MapperViewModel(WorkTask item)
      => new ArrangeTaskViewModel
      {
         id = item.TaskId.ToString(),
         code = item.ManagerId.ToString(),
         duration = item.StartDate.WorkDayBetween(item.EndDate).Count() + 1,
         hours = item.EstimateWorkHours <= 0 ? TaskKeys.DefaultEstimateHours : item.EstimateWorkHours,
         start = item.StartDate.ToString(),
         end = item.EndDate.ToString(),
         level = item.TaskLevel,
         name = item.TaskName,
         parentId = item.ParentId.ToString(),
         isMine = item.ManagerId == GetUserInfo().UserId,
         sortId = item.SortId,
         stat = item.TaskStatus.ToString(),
         description = item.Description,
         workhours = item.WorkHours,
         progress = item.RateOfProgress,
         executorId = item.ManagerId.ToString(),
         reviewerId = item.ReviewerID.ToString(),
         createrId = item.CreatorId.ToString(),
         taskType = item.TaskType.ToString(),
         serviceCount = item.ServiceCount,
         subType = item.SubTypeId.ToString(),
         subTypeValue = item.SubTypeValue,
         realStart = item.RealStartDate.ToString(),
         realEnd = item.RealEndDate.ToString()
      };


      private List<Project> MyJoinedProjects() => ProjectrHelper.UserJoinedProjects(GetUserInfo().UserId, db).FindAll(p => p.ProjectStatus != ProjectKeys.CompleteStatus);


      private void BindParentId(List<ArrangeTaskViewModel> tasks)
      {

         for (int i = 0; i < tasks.Count; i++)
         {
            var parent = tasks[i];

            for (int j = i + 1; j <= tasks.Count - 1; j++)
            {
               if (tasks[j].level == parent.level + 1)
               {
                  tasks[j].parentId = parent.id;
                  tasks[i].IsParent = true;
               }
               else if (tasks[j].level == parent.level)
                  break;
            }
         }
      }


      private List<ArrangeTaskViewModel> GetTaskViewModels(List<WorkTask> tasks, WorkTask root)
      {
         var taskViewModels = new List<ArrangeTaskViewModel>();
         var children = TaskHelper.GetAllChildren(root, tasks, true);

         foreach (var item in children)
         {
            if (item.IsDelteStatus) continue;

            taskViewModels.Add(
               MapperViewModel(item)
               );
         };

         return taskViewModels;
      }


      private TaskEditOption GetEditOption(WorkTask task, Result rs)
      {
         var tasks = db.WorkTaskDal.ConditionQuery(t.Projectid == task.Projectid & t.TaskStatus != TaskKeys.DeleteStatus, t.SortId.Asc, null, null);
         var project = ProjectrHelper.GetCurrentProject(task.Projectid, db, true);
         var originalTask = db.WorkTaskDal.PrimaryGet(task.TaskId);

         return new TaskEditOption { db = db, Result = rs, OperatorId = GetUserInfo().UserId, Tasks = tasks, Project = project, Original = originalTask };

      }


      #endregion

   }

}