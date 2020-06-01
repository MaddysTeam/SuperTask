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
		public ActionResult List(Guid projectId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
		{
			ThrowNotAjax();

			var user = GetUserInfo();

			var tasks = APQuery.select(t.TaskId, t.TaskName, t.TaskType, t.ParentId, t.IsParent,
									   t.TaskStatus, t.ManagerId, t.WorkHours, t.EndDate, u.UserName)
			   .from(t,
				   u.JoinLeft(u.UserId == t.ManagerId)
				   )
			   .where(t.Projectid == projectId)
			   .query(db, r => new WorkTask
			   {
				   TaskId = t.TaskId.GetValue(r),
				   TaskName = t.TaskName.GetValue(r),
				   ParentId = t.ParentId.GetValue(r),
				   IsParent = t.IsParent.GetValue(r),
				   ManagerId = t.ManagerId.GetValue(r),
				   Manager = u.UserName.GetValue(r)
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

			//过滤条件
			//模糊搜索用户名、实名进行

			//searchPhrase = searchPhrase.Trim();
			//if (searchPhrase != "")
			//{
			//   query.where_and(p.ProjectName.Match(searchPhrase) |
			//      p.ProjectOwner.Match(searchPhrase) |
			//      p.Code.Match(searchPhrase) |
			//      p.ProjectName.Match(searchPhrase) |
			//      p.ProjectExecutor.Match(searchPhrase) |
			//      u.UserName.Match(searchPhrase)
			//      );
			//}


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


			//获得查询的总数量

			// var total = db.ExecuteSizeOfSelect(query);


			//查询结果集
			//var result = query.query(db, rd =>
			//{
			//   var statusVal = p.ProjectStatus.GetValue(rd);
			//   var type = p.ProjectType.GetValue(rd);

			//   return new
			//   {
			//      id = p.ProjectId.GetValue(rd),
			//      code = p.RealCode.GetValue(rd),
			//      projectName = p.ProjectName.GetValue(rd),
			//      progress = p.RateOfProgress.GetValue(rd) + "%",
			//      projectOwner = p.ProjectOwner.GetValue(rd),
			//      projectExecutor = ProjectKeys.GetExecutorById(p.ProjectExecutorId.GetValue(rd)),
			//      pmName = u.UserName.GetValue(rd, "managerName"),
			//      projectStatus = ProjectKeys.GetStatusKeyById(statusVal),
			//      orgId = p.OrgId.GetValue(rd),
			//      //orgName = o.Name.GetValue(rd, "orgName"),
			//      type = ProjectKeys.GetTypeKeyById(type)
			//   };
			//});


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
				try
				{
					db.BeginTrans();

					if (task.TaskId.IsEmpty())
						task.TaskId = Guid.NewGuid();

					bool hasParnet = !string.IsNullOrEmpty(collection["parentTasks"]);
					if (!hasParnet)
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
								TaskLevel = task.TaskLevel + 1
							};

							db.WorkTaskDal.Insert(subTask);

							index++;
						}
					}
					else
					{
						task.ParentId = collection["parentTasks"].ToGuid(Guid.Empty);
					}

					task.IsParent = hasParnet;
					db.WorkTaskDal.Insert(task);


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

		public ActionResult EditDetail(Guid id)
		{
			var task = WorkTask.PrimaryGet(id);

			task.ProjectName = MyJoinedProjects().Find(x => x.ProjectId == task.Projectid)?.ProjectName;

			ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
			ViewBag.SubTask = TaskHelper.GetAllChildren(id);

			return View(task);
		}

		[HttpPost]
		public ActionResult EditDetail(WorkTask task, FormCollection collection)
		{
			try
			{
				var subTasks = TaskHelper.GetAllChildren(task.TaskId);

				if (task.IsParent && subTasks.Count > 0)
				{
					// delete all subtasks
					db.WorkTaskDal.ConditionDelete(t.TaskId.In(subTasks.Select(x => x.TaskId).ToArray()));

					// re-add all subtasks whitch properties value should be changed

					var subTaskEsTimes = collection["subTaskWorkHours"];
					var subTaskExecutors = collection["subTaskExecutorId"];
					var subTaskIds = collection["subTaskId"];
					var index = 0;
					foreach (var id in subTaskIds.Split(','))
					{
						var subTask = subTasks.Find(x => x.TaskId == id.ToGuid(Guid.Empty));
						subTask.ManagerId = subTaskExecutors.Split(',')[index].ToGuid(Guid.Empty);
						subTask.EstimateWorkHours = double.Parse(subTaskEsTimes.Split(',')[index]);

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

			return View(task);
		}


		[HttpPost]
		public ActionResult Delete(Guid id)
		{
			return Json(new { });
		}


		[HttpPost]
		public ActionResult Start()
		{
			return Json(new { });
		}

		[HttpPost]
		public ActionResult Complete()
		{
			return Json(new { });
		}


		[HttpPost]
		public ActionResult Close()
		{
			return Json(new { });
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

		#endregion

	}

}