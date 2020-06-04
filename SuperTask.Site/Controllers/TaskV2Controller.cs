using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class TaskV2Controller : BaseController
   {

      APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
      APDBDef.UserInfoTableDef u = APDBDef.UserInfo;

      public ActionResult List()
      {
         ViewBag.Projects = MyJoinedProjects();

         return View();
      }


      [HttpPost]
      public ActionResult List(Guid projectId, Guid levelId, Guid typeId, Guid statusId, bool isAssign, bool isJoin,string taskName,
                             int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         ThrowNotAjax();

         var user = GetUserInfo();

         var query = APQuery.select(t.TaskId, t.TaskName, t.TaskType, t.ParentId, t.IsParent,
                     t.V2Level, t.SortId, t.TaskStatus, t.EstimateWorkHours,
                     t.TaskStatus, t.ManagerId, t.WorkHours, t.EndDate, u.UserName)
            .from(t,
             u.JoinLeft(u.UserId == t.ManagerId)
             )
            .where(t.Projectid == projectId);

         if (levelId != TaskKeys.SelectAll)
         {
            query = query.where_and(t.V2Level == levelId);
         }
         if (typeId != TaskKeys.SelectAll)
         {
            query = query.where_and(t.TaskType == typeId);
         }
         if (statusId != TaskKeys.SelectAll)
         {
            query = query.where_and(t.TaskStatus == statusId);
         }
         if (!string.IsNullOrEmpty(taskName))
         {
            query = query.where_and(t.TaskName.Match(taskName));
         }

         var tasks = query.order_by(t.SortId.Asc)
            .query(db, r => new WorkTask
            {
               TaskId = t.TaskId.GetValue(r),
               TaskName = t.TaskName.GetValue(r),
               ParentId = t.ParentId.GetValue(r),
               IsParent = t.IsParent.GetValue(r),
               ManagerId = t.ManagerId.GetValue(r),
               Manager = u.UserName.GetValue(r),
               V2Level = t.V2Level.GetValue(r),
               SortId = t.SortId.GetValue(r),
               TaskType = t.TaskType.GetValue(r),
               TaskStatus = t.TaskStatus.GetValue(r),
               EstimateWorkHours = t.EstimateWorkHours.GetValue(r)
            }).ToList();


         var results = new List<WorkTask>();
         var parents = tasks.FindAll(x => x.IsParent);
         if (parents.Count > 0)
         {
            foreach (var item in parents)
            {
               results.Add(item);
               results.AddRange(tasks.FindAll(x => x.ParentId == item.TaskId));
            }
         }

         if (isAssign)
         {
            results = results.FindAll(t => t.ManagerId == user.UserId);
         }

         var total = results.Count;
         if (total > 0)
         {
            results = results.Skip(rowCount * (current - 1)).Take(rowCount).ToList();
         }

         return Json(new
         {
            rows = results,
            current,
            rowCount,
            total
         });


         //排序条件表达式

         //if (sort != null)
         //{
         //   switch (sort.ID)
         //   {
         //      case "projectName": query.order_by(sort.OrderBy(p.ProjectName)); break;
         //      case "projectOwner": query.order_by(sort.OrderBy(p.ProjectOwner)); break;
         //      //case "projectExecutor": query.order_by(sort.OrderBy(p.ProjectExecutor)); break;
         //      case "pmName": query.order_by(sort.OrderBy(u.UserName)); break;
         //      case "type": query.order_by(sort.OrderBy(p.ProjectType)); break;
         //      case "code": query.order_by(sort.OrderBy(p.Code)); break;
         //      case "progress": query.order_by(sort.OrderBy(p.RateOfProgress)); break;
         //   }
         //}



      }


      //GET  TaskV2/Add
      //POST-Ajax  /TaskV2/Add
      //POST-Ajax  /TaskV2/AddExecutor

      public ActionResult Add()
      {
         ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
         ViewBag.Projects = MyJoinedProjects();

         return View(new WorkTask { ManagerId = GetUserInfo().UserId });
      }

      public ActionResult AddSubTask(Guid projectId, Guid parentId)
      {
         ViewBag.Projects = MyJoinedProjects();
         ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);

         return View("Add", new WorkTask { ManagerId = GetUserInfo().UserId, Projectid = projectId, ParentId = parentId });
      }

      [HttpPost]
      public ActionResult Add(WorkTask task, FormCollection collection)
      {
         if (task.Validate().IsSuccess)
         {

            var sortId = task.SortId = GetTaskSortNo(task.Projectid);

            try
            {
               db.BeginTrans();

               if (task.TaskId.IsEmpty())
                  task.TaskId = Guid.NewGuid();

               var isParent = task.ParentId.IsEmpty();
               if (isParent)
               {
                  var subTaskEsTimes = collection["estimateWorkHours"];
                  var subTaskExecutors = collection["executorId"];
                  var index = 0;
                  foreach (var item in subTaskExecutors.Split(','))
                  {
                     WorkTask subTask = new WorkTask
                     {
                        TaskId = Guid.NewGuid(),
                        Projectid = task.Projectid,
                        TaskName = task.TaskName,
                        TaskType = task.TaskType,
                        ParentId = task.TaskId,
                        StartDate = task.StartDate,
                        EndDate = task.EndDate,
                        ManagerId = item.ToGuid(Guid.Empty),
                        Description = task.Description,
                        EstimateWorkHours = double.Parse(subTaskEsTimes.Split(',')[index]),
                        V2Level = task.V2Level,
                        SortId = ++sortId,

                        TaskStatus = TaskKeys.PlanStatus
                     };

                     db.WorkTaskDal.Insert(subTask);

                     //create subtask journal
                     WorkJournalHelper.CreateByTask(subTask, db);

                     index++;
                  }
               }

               task.IsParent = isParent;
               task.TaskStatus = TaskKeys.PlanStatus;
               db.WorkTaskDal.Insert(task);

               //create default journal
               WorkJournalHelper.CreateByTask(task, db);


               db.Commit();
            }
            catch (Exception e)
            {
               db.Rollback();

               return Json(new
               {
                  result = AjaxResults.Error
               });
            }
         }
         else
         {
            return Json(new
            {
               result = AjaxResults.Error
            });
         }

         return Json(new
         {
            result = AjaxResults.Success
         });
      }

      [HttpPost]
      public ActionResult AddExecutor()
      {
         ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
         return PartialView("_subTaskExecutor", new SubTaskExecutorViewModel { ExecutorId = GetUserInfo().UserId });
      }


      //GET  TaskV2/EditDetail

      public ActionResult Edit(Guid id)
      {
         var task = WorkTask.PrimaryGet(id);
         task.ProjectName = MyJoinedProjects().Find(x => x.ProjectId == task.Projectid)?.ProjectName;

         ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
         ViewBag.SubTask = TaskHelper.GetAllChildren(id);

         return View(task);
      }

      [HttpPost]
      public ActionResult Edit(WorkTask task, FormCollection collection)
      {
         try
         {
            var subTasks = TaskHelper.GetAllChildren(task.TaskId);
            var subTaskEsTimes = collection["EstimateWorkHours"] ?? string.Empty;
            var subTaskExecutors = collection["ExecutorId"] ?? string.Empty;
            var subTaskIds = collection["SubTaskId"] ?? string.Empty;

            if (task.IsParent && !string.IsNullOrEmpty(subTaskIds))
            {
               // delete all subtasks

               if (subTasks.Count > 0)
                  db.WorkTaskDal.ConditionDelete(t.TaskId.In(subTasks.Select(x => x.TaskId).ToArray()));


               db.BeginTrans();

               // re-add all subtasks whitch properties value should be changed

               var index = 0;
               foreach (var id in subTaskIds.Split(','))
               {
                  var subTask = subTasks.Find(x => x.TaskId == id.ToGuid(Guid.Empty));
                  if (null == subTask)
                  {
                     subTask = new WorkTask
                     {
                        TaskId = Guid.NewGuid(),
                        Projectid = task.Projectid,
                        TaskName = task.TaskName,
                        TaskType = task.TaskType,
                        ParentId = task.TaskId,
                        StartDate = task.StartDate,
                        EndDate = task.EndDate,
                        Description = task.Description,
                        V2Level = task.V2Level,
                        // TaskLevel = task.TaskLevel + 1,
                        TaskStatus = TaskKeys.PlanStatus
                     };
                  }

                  subTask.ManagerId = subTaskExecutors.Split(',')[index].ToGuid(Guid.Empty);

                  // EstimateWorkHours 也用在了父任务负责人的【预估工时】填写字段，所以需要index+1,
                  subTask.EstimateWorkHours = double.Parse(subTaskEsTimes.Split(',')[index + 1]);

                  db.WorkTaskDal.Insert(subTask);

                  index++;
               }

            }

            WorkTask orignalTask = db.WorkTaskDal.PrimaryGet(task.TaskId);
            if (orignalTask.IsPlanStatus && task.IsProcessStatus)
            {
               task.Start();
            }
            else if (!orignalTask.IsCompleteStatus && task.IsCompleteStatus)
            {
               task.Complete(DateTime.Now);
            }

            db.WorkTaskDal.Update(task);


            db.Commit();
         }
         catch (Exception e)
         {
            db.Rollback();

            return Json(new
            {
               result = AjaxResults.Error
            });
         }

         return Json(new
         {
            result = AjaxResults.Success
         });
      }

      //GET TaskV2/Detail

      public ActionResult Detail(Guid id)
      {
         var task = WorkTask.PrimaryGet(id);
         task.Manager = string.Empty;

         ViewBag.SubTasks = TaskHelper.GetAllChildren(id);

         ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);

         return View(task);
      }


      [HttpPost]
      public ActionResult Delete(Guid id)
      {
         return Json(new { });
      }


      [HttpPost]
      public ActionResult Start(Guid id)
      {
         APQuery.update(t)
               .set(t.TaskStatus.SetValue(TaskKeys.ProcessStatus))
               .set(t.RealStartDate.SetValue(DateTime.Now))
               .where(t.TaskId == id)
            .execute(db);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Task.EDIT_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult Complete(Guid id)
      {
         APQuery.update(t)
               .set(t.TaskStatus.SetValue(TaskKeys.CompleteStatus))
               .set(t.RealEndDate.SetValue(DateTime.Now))
               .where(t.TaskId == id | t.ParentId == id)
               .execute(db);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Task.EDIT_SUCCESS
         });
      }


      [HttpPost]
      public ActionResult Close()
      {
         return Json(new { });
      }


      public ActionResult JournalEdit(Guid id)
      {
         var j = APDBDef.WorkJournal;

         var journal = db
            .WorkJournalDal
            .ConditionQuery(j.TaskId == id & j.RecordDate.Between(DateTime.Now.TodayStart(), DateTime.Now.TodayEnd()), null, null, null)
            .FirstOrDefault();

         return RedirectToAction("Edit", "WorkJournal", new { id = journal.JournalId });
      }


      //POST  TaskV2/GetLeafTasks

      [HttpPost]
      public ActionResult GetLeafTasks(Guid projectId)
      {
         var userid = GetUserInfo().UserId;
         var tasks = TaskHelper.GetProjectLeafTasks(projectId, userid, db)
            .Select(x => new
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


      //POST  TaskV2/GetChildTasks

      [HttpPost]
      public ActionResult GetChildTasks(Guid parentId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var results = TaskHelper.GetAllChildren(parentId);

         return Json(new
         {
            rows = results,
            current,
            rowCount,
            total = results.Count
         });
      }


      #region [private]


      private List<Project> MyJoinedProjects() => ProjectrHelper.UserJoinedProjects(GetUserInfo().UserId, db).FindAll(p => p.ProjectStatus != ProjectKeys.CompleteStatus & p.ProjectStatus != ProjectKeys.DeleteStatus);


      private int GetTaskSortNo(Guid projectId)
      {
         var tasks = TaskHelper.GetProjectTasks(projectId);

         return tasks.Count;
      }

      #endregion

   }

}