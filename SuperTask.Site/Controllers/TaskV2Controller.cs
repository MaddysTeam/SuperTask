﻿using Business;
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
		public ActionResult List(Guid projectId, Guid levelId, Guid typeId, Guid statusId, bool isAssign, bool isJoin, string taskName,
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
				   WorkHours = t.WorkHours.GetValue(r),
				   EstimateWorkHours = t.EstimateWorkHours.GetValue(r)
			   }).ToList();


			var results = new List<WorkTask>();
			var parents = tasks.FindAll(x => x.IsParent);


			if (parents.Count > 0)
			{

				//排序条件表达式

				if (sort != null)
				{
					switch (sort.ID)
					{
						case "SortId":
							parents = sort.According == APSqlOrderAccording.Asc ? parents.OrderBy(x => x.SortId).ToList() :
																				   parents.OrderByDescending(x => x.SortId).ToList(); break;
					}
				}

				foreach (var item in parents)
				{
					if (item.ManagerId == user.UserId)
					{
						results.Add(item);
						results.AddRange(tasks.FindAll(x => x.ParentId == item.TaskId));
					}
					else
					{
						results.AddRange(tasks.FindAll(x => x.ParentId == item.TaskId && x.ManagerId == user.UserId));
					}
				}
			}

			if (levelId != TaskKeys.SelectAll)
			{
				results = results.FindAll(t => t.V2Level == levelId);
			}
			if (typeId != TaskKeys.SelectAll)
			{
				results = results.FindAll(t => t.TaskType == typeId);
			}
			if (statusId != TaskKeys.SelectAll)
			{
				results = results.FindAll(t => t.TaskStatus == statusId);
			}
			else
			{
				results = results.FindAll(t => t.TaskStatus != TaskKeys.DeleteStatus);
			}
			if (!string.IsNullOrEmpty(taskName))
			{
				results = results.FindAll(t => t.TaskName.IndexOf(taskName) >= 0);
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

		}


		//GET  TaskV2/Add
		//POST-Ajax  /TaskV2/Add
		//POST-Ajax  /TaskV2/AddExecutor

		public ActionResult Add()
		{
			ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
			ViewBag.Projects = MyJoinedProjects();

			var currentProjectID = Request["projectId"];
			return View(new WorkTask { ManagerId = GetUserInfo().UserId,Projectid= currentProjectID.ToGuid(Guid.Empty) });
		}

		public ActionResult AddSubTask(Guid projectId, Guid parentId)
		{
			ViewBag.Projects = MyJoinedProjects();
			ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);

			return View("Add", new WorkTask { ManagerId = GetUserInfo().UserId, Projectid = projectId, ParentId = parentId });
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Add(WorkTask task, FormCollection collection)
		{
			if (task.Validate().IsSuccess)
			{
				// each project task sort number start from position 1
				var sortId = GetTaskSortNo(task.Projectid, db);
				task.SortId = ++sortId;

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

							//create subtask
							db.WorkTaskDal.Insert(subTask);

							//create subtask journal
							WorkJournalHelper.CreateByTask(subTask, db);

							//add user to project resurce if not exits
							ResourceHelper.AddUserToResourceIfNotExist(subTask.Projectid, subTask.TaskId, subTask.ManagerId, ResourceKeys.OtherType, db);

							index++;
						}
					}

					task.IsParent = isParent;
					task.TaskStatus = TaskKeys.PlanStatus;
					db.WorkTaskDal.Insert(task);

					//create default journal
					WorkJournalHelper.CreateByTask(task, db);

					//add user to project resurce if not exits
					ResourceHelper.AddUserToResourceIfNotExist(task.Projectid, task.TaskId, task.ManagerId, ResourceKeys.OtherType, db);

					//add relative items TODO:  sub task do not need bind any relative items now
					var requires = task.RelativeRequireIds?.Split(',');
					RTPBRelationHelper.BindRelationBetweenRequiresAndTask(requires.ConvertToGuidArray(), task.TaskId, db);

					var bugs = task.RelativeBugIds?.Split(',');
					RTPBRelationHelper.BindRelationBetweenBugsAndTask(bugs.ConvertToGuidArray(), task.TaskId, db);


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


		//GET  TaskV2/Edit

		public ActionResult Edit(Guid id)
		{
			var task = WorkTask.PrimaryGet(id);
			task.ProjectName = MyJoinedProjects().Find(x => x.ProjectId == task.Projectid)?.ProjectName;
			task.RelativeRequireIds = RTPBRelationHelper.GetTaskRelativeRequireIds(id, db);

			ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
			ViewBag.SubTask = TaskHelper.GetAllChildren(id, db, t.CreateDate.Desc).FindAll(x => !x.IsDelteStatus);
			ViewBag.Attahcments = AttachmentHelper.GetAttachments(task.Projectid, task.TaskId, db);

			return View(task);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(WorkTask task, FormCollection collection)
		{
			db.BeginTrans();

			try
			{
				var subTasks = TaskHelper.GetAllChildren(task.TaskId).FindAll(t => !t.IsDelteStatus);
				var subTaskEsTimes = collection["EstimateWorkHours"] ?? string.Empty;
				var subTaskExecutors = collection["ExecutorId"] ?? string.Empty;
				var subTaskIds = collection["SubTaskId"] ?? string.Empty;

				if (task.IsParent && !string.IsNullOrEmpty(subTaskIds))
				{
					// due to in transaction so we need set sort no for new tasks in memory
					var newSortNo = GetTaskSortNo(task.Projectid, db);

					// delete all subtasks

					if (subTasks.Count > 0)
						db.WorkTaskDal.ConditionDelete(t.TaskId.In(subTasks.Select(x => x.TaskId).ToArray()));

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
								SortId = ++newSortNo,
								TaskStatus = TaskKeys.PlanStatus,
								ManagerId = task.ManagerId
							};

							//create subtask journal
							WorkJournalHelper.CreateByTask(subTask, db);
						}

						subTask.ManagerId = subTaskExecutors.Split(',')[index].ToGuid(Guid.Empty);

						// EstimateWorkHours 也用在了父任务负责人的【预估工时】填写字段，所以需要index+1,
						subTask.EstimateWorkHours = double.Parse(subTaskEsTimes.Split(',')[index + 1]);

						db.WorkTaskDal.Insert(subTask);

						//add user to project resurce if not exits
						ResourceHelper.AddUserToResourceIfNotExist(subTask.Projectid, subTask.TaskId, subTask.ManagerId, ResourceKeys.OtherType, db);

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
				else if (!orignalTask.IsCloseStatus && orignalTask.IsCloseStatus)
				{
					task.Close(DateTime.Now);
				}
				else if (task.IsProcessStatus)
				{
					WorkJournalHelper.CreateOrUpdateJournalByTask(task, db);
				}

				db.WorkTaskDal.Update(task);

				//add user to project resurce if not exits
				ResourceHelper.AddUserToResourceIfNotExist(task.Projectid, task.TaskId, task.ManagerId, ResourceKeys.OtherType, db);

				//upload attachments
				AttachmentHelper.UploadTaskAttachment(task, db);

				//add relative items TODO:  sub task do not need bind any relative items now
				var requires = task.RelativeRequireIds?.Split(',');
				RTPBRelationHelper.BindRelationBetweenRequiresAndTask(requires.ConvertToGuidArray(), task.TaskId, db);

				var bugs = task.RelativeBugIds?.Split(',');
				RTPBRelationHelper.BindRelationBetweenBugsAndTask(bugs.ConvertToGuidArray(), task.TaskId, db);


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

			ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
			ViewBag.SubTasks = TaskHelper.GetAllChildren(id, db, t.CreateDate.Desc).FindAll(x => !x.IsDelteStatus);
			ViewBag.Attahcments = AttachmentHelper.GetAttachments(task.Projectid, task.TaskId, db);

			return View(task);
		}


		[HttpPost]
		public ActionResult Delete(Guid id)
		{
			APQuery.update(t)
			   .set(t.TaskStatus.SetValue(TaskKeys.DeleteStatus))
			   .where(t.TaskId == id)
			   .execute(db);

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Task.EDIT_SUCCESS
			});
		}


		//POST-Ajax TaskV2/Start
		//POST-Ajax TaskV2/MultipleStart

		public ActionResult Start(Guid id)
		{
			return PartialView("_start");
		}

		[HttpPost]
		public ActionResult Start(OperationViewModel model)
		{
			//APQuery.update(t)
			//	 .set(t.TaskStatus.SetValue(TaskKeys.ProcessStatus))
			//	 .set(t.RealStartDate.SetValue(DateTime.Now))
			//	 .where(t.TaskId == id)
			//   .execute(db);

			//return Json(new
			//{
			//	result = AjaxResults.Success,
			//	msg = Success.Task.EDIT_SUCCESS
			//});

			return null;
		}

		[HttpPost]
		public ActionResult MultipleStart(Guid[] ids)
		{
			var tasks = db.WorkTaskDal.ConditionQuery(t.TaskId.In(ids), null, null, null);

			db.BeginTrans();

			try
			{
				APQuery.update(t)
			   .set(t.TaskStatus.SetValue(TaskKeys.ProcessStatus))
			   .set(t.RealStartDate.SetValue(DateTime.Now))
			   .where(t.TaskId.In(ids))
			   .execute(db);

				tasks.ForEach(x => x.Start());
				WorkJournalHelper.CreateOrUpdateJournalByTasks(tasks, db);

				db.Commit();
			}
			catch
			{
				db.Rollback();
			}




			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Task.EDIT_SUCCESS
			});
		}


		//POST-Ajax TaskV2/Complete
		//POST-Ajax TaskV2/MultipleComplete

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
		public ActionResult MultipleComplete(Guid[] ids)
		{
			APQuery.update(t)
			 .set(t.TaskStatus.SetValue(TaskKeys.CompleteStatus))
			 .set(t.RealEndDate.SetValue(DateTime.Now))
			 .where(t.TaskId.In(ids))
			 .execute(db);

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Task.EDIT_SUCCESS
			});
		}


		//POST-Ajax TaskV2/Close
		//POST-Ajax TaskV2/MultipleClose
		[HttpPost]
		public ActionResult Close(Guid id)
		{
			APQuery.update(t)
				.set(t.TaskStatus.SetValue(TaskKeys.CloseStatus))
				.set(t.CloseDate.SetValue(DateTime.Now))
				.where(t.TaskId == id | t.ParentId == id)
				.execute(db);

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Task.EDIT_SUCCESS
			});
		}

		[HttpPost]
		public ActionResult MultipleClose(Guid[] ids)
		{
			APQuery.update(t)
			 .set(t.TaskStatus.SetValue(TaskKeys.CloseStatus))
			 .set(t.RealEndDate.SetValue(DateTime.Now))
			 .where(t.TaskId.In(ids))
			 .execute(db);

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Task.EDIT_SUCCESS
			});
		}



		//POST-Ajax TaskV2/JournalEdit

		public ActionResult JournalEdit(Guid id)
		{
			var j = APDBDef.WorkJournal;

			var journal = WorkJournalHelper.GetJournal(id, GetUserInfo().UserId, db);

			return RedirectToAction("Edit", "WorkJournal", new { id = journal == null ? Guid.NewGuid() : journal.JournalId });
		}


		//POST-Ajax  TaskV2/GetLeafTasks

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


		//POST-Ajax  TaskV2/GetChildTasks

		[HttpPost]
		public ActionResult GetChildTasks(Guid parentId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
		{
			var results = APQuery.select(t.TaskId, t.SortId, t.ManagerId, t.TaskStatus, t.TaskName, u.UserName)
							.from(t, u.JoinInner(u.UserId == t.ManagerId))
							.order_by(t.CreateDate.Desc)
							.where(t.ParentId == parentId & t.TaskStatus != TaskKeys.DeleteStatus)
							.query(db, r => new WorkTask
							{
								TaskId = t.TaskId.GetValue(r),
								TaskName = t.TaskName.GetValue(r),
								SortId = t.SortId.GetValue(r),
								ManagerId = t.ManagerId.GetValue(r),
								Manager = u.UserName.GetValue(r),
								TaskStatus = t.TaskStatus.GetValue(r),
							})
							.ToList();


			return Json(new
			{
				rows = results.Skip(rowCount * (current - 1)).Take(rowCount),
				current,
				rowCount = rowCount,
				total = results.Count
			});
		}


		//POST-Ajax  TaskV2/GetProjectTasks

		[HttpPost]
		public ActionResult GetProjectTasks(Guid projectId)
		{
         if (projectId.IsEmptyGuid())
         {
            return Json(new { });
         }
			var all = TaskHelper.GetProjectTasks(projectId, db);
			var parents = all?.FindAll(x => x.IsParent);
			var results= new List<WorkTask>();
			foreach(var item in parents)
			{
				results.Add(item);
				results.AddRange(parents.FindAll(x => x.ParentId == item.TaskId));
			}

         return Json(new {
            rows = results.Select(x => new { id = x.TaskId, text = x.TaskName }).ToList()
			});
			
		}


		#region [private]


		private List<Project> MyJoinedProjects() => ProjectrHelper.UserJoinedProjects(GetUserInfo().UserId, db).FindAll(p => p.ProjectStatus != ProjectKeys.CompleteStatus & p.ProjectStatus != ProjectKeys.DeleteStatus);


		private int GetTaskSortNo(Guid projectId, APDBDef db)
		{
			var tasks = TaskHelper.GetProjectTasks(projectId, db);

			return tasks.Count > 0 ? tasks.Max(x => x.SortId) : tasks.Count;
		}

		#endregion

	}

}