using Business;
using Business.Config;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

	public class BugController : BaseController
	{
		static APDBDef.BugTableDef b = APDBDef.Bug;
		static APDBDef.UserInfoTableDef u = APDBDef.UserInfo;
		static APDBDef.RTPBRelationTableDef tb = APDBDef.RTPBRelation;
		static APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
		static APDBDef.OperationTableDef o = APDBDef.Operation;

		//GET  /Bug/List
		//POST-AJAX /Bug/List

		public ActionResult List()
		{

			ViewBag.Projects = MyJoinedProjects();

			return View();
		}

		[HttpPost]
		public ActionResult List(Guid projectId, Guid levelId, Guid typeId, Guid statusId, bool isAssign, bool isJoin,
						  int current, int rowCount, AjaxOrder sort, string searchPhrase)
		{
			ThrowNotAjax();

			var user = GetUserInfo();
			var u2 = APDBDef.UserInfo.As("creator");

			var query = APQuery.select(b.BugId, b.BugName, b.BugType,
				   b.BugLevel, b.SortId, b.BugStatus, b.FixDate,
				   b.ManagerId, u.UserName, b.CreateDate, u2.UserName.As("creator"))
			   .from(b,
			   u.JoinLeft(u.UserId == b.ManagerId),
			   u2.JoinLeft(u2.UserId == b.CreatorId)
			   );

			if (projectId != AppKeys.SelectAll)
				query = query.where(b.Projectid == projectId);

			var results = query.order_by(b.SortId.Asc)
			   .query(db, r => new Bug
			   {
				   BugId = b.BugId.GetValue(r),
				   BugName = b.BugName.GetValue(r),
				   ManagerId = b.ManagerId.GetValue(r),
				   Manager = u.UserName.GetValue(r),
				   Creator = u2.UserName.GetValue(r, "creator"),
				   CreateDate = b.CreateDate.GetValue(r),
				   BugLevel = b.BugLevel.GetValue(r),
				   SortId = b.SortId.GetValue(r),
				   BugType = b.BugType.GetValue(r),
				   BugStatus = b.BugStatus.GetValue(r),
				   FixDate = b.FixDate.GetValue(r),
			   }).ToList();


			//排序条件表达式

			if (sort != null)
			{
				switch (sort.ID)
				{
					case "SortId":
						results = sort.According == APSqlOrderAccording.Asc ? results.OrderBy(x => x.SortId).ToList() :
																  results.OrderByDescending(x => x.SortId).ToList(); break;
				}
			}


			if (levelId != AppKeys.SelectAll)
			{
				results = results.FindAll(b => b.BugLevel == levelId);
			}
			if (typeId != AppKeys.SelectAll)
			{
				results = results.FindAll(b => b.BugType == typeId);
			}
			if (statusId != AppKeys.SelectAll)
			{
				results = results.FindAll(t => t.BugStatus == statusId);
			}
			//else
			//{
			//	results = results.FindAll(t => t.TaskStatus != TaskKeys.DeleteStatus);
			//}
			if (!string.IsNullOrEmpty(searchPhrase))
			{
				results = results.FindAll(b => b.BugName.IndexOf(searchPhrase) >= 0 || b.SortId.ToString() == searchPhrase);
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


		//GET  /Bug/Edit
		//POST-AJAX /Bug/Edit

		public ActionResult Edit(Guid? id)
		{

			// 所有人员
			ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);

			// 我参与的项目
			ViewBag.Projects = MyJoinedProjects();

			Bug bug = new Bug();

			if (id != null && !id.Value.IsEmpty())
			{
				bug = db.BugDal.PrimaryGet(id.Value);

				// 关联的任务
				bug.RelativeTasks = RTPBRelationHelper.GetBugRelativeTasks(id.Value, db);
				bug.RelativeTaskIds = RTPBRelationHelper.GetTaskIds(bug.RelativeTasks);

				// 关联的需求
				bug.RelativeRequires = RTPBRelationHelper.GetBugRelativeRequires(id.Value, db);
				bug.RelativeRequireIds = RTPBRelationHelper.GetRequireIds(bug.RelativeRequires);

				return View("Edit", bug);
			}

			return PartialView("Add", bug);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(Bug bug, FormCollection collection)
		{
			if (!ModelState.IsValid)
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			if (bug.Projectid == BugKeys.SelectAll)
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			var user = GetUserInfo();

			db.BeginTrans();

			try
			{
				if (bug.BugId.IsEmptyGuid())
				{
					var sortId = GetBugMaxSortNo(bug.Projectid, db);
					bug.SortId = ++sortId;

					bug.BugId = Guid.NewGuid();
					bug.CreateDate = DateTime.Now;
					bug.CreatorId = user.UserId;
					bug.BugStatus = BugKeys.readyToConfirm;

					db.BugDal.Insert(bug);
				}
				else
				{
					bug.ModifyDate = DateTime.Now;

					db.BugDal.Update(bug);
				}


				// add attachment
				AttachmentHelper.UploadBugsAttachment(bug, user.UserId, db);

				//add user to project resurce if not exits
				ResourceHelper.AddUserToResourceIfNotExist(bug.Projectid, Guid.Empty, bug.ManagerId, ResourceKeys.OtherType, db);

				var taskids = bug.RelativeTaskIds?.Split(',');
				RTPBRelationHelper.BindRelationBetweenTasksAndBug(taskids?.ConvertToGuidArray(), bug.BugId, db);

				var requireIds = bug.RelativeRequireIds?.Split(',');
				RTPBRelationHelper.BindRelationBetweenRequiresAndBug(requireIds?.ConvertToGuidArray(), bug.BugId, db);

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


		//GET Bug/Detail

		public ActionResult Detail(Guid id)
		{
			var bug = Bug.PrimaryGet(id);
			var users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
			bug.Manager = users.Find(x => x.UserId == bug.ManagerId)?.RealName;

			// bug操作历史记录
			bug.OperationHistory = OperationHelper.GetOperationHistoryViewModels(
			   id,
			   GetUserInfo(),
			   t => BugKeys.OperationResultDic[t.OperationResult.ToGuid(Guid.Empty)],
			   db
			   );

			// 关联的任务
			bug.RelativeTasks = RTPBRelationHelper.GetBugRelativeTasks(id, db);

			// 关联的需求
			bug.RelativeRequires = RTPBRelationHelper.GetBugRelativeRequires(id, db);

			ViewBag.Attahcments = AttachmentHelper.GetAttachments(bug.Projectid, bug.BugId, db);

			return View(bug);
		}


		//GET  /Bug/Resolve
		//GET  /Bug/MultipleResolve
		//POST-AJAX /Bug/Resolve

		public ActionResult Resolve(Guid id)
		{
			Bug bug = db.BugDal.PrimaryGet(id);

			var existConfrim = OperationHelper.GetOperation(bug.BugId, BugKeys.BugResolveGuid);

			return PartialView("_resolve",
			  new BugResolveViewModel
			  {
				  Id = bug.BugId.ToString(),
				  Name = bug.BugName,
				  ProjectId = bug.Projectid,
				  SortId = bug.SortId,
				  Remark = existConfrim?.Content,
				  Result = existConfrim?.OperationResult,
			  });
		}

		public ActionResult MultipleResolve(string ids, Guid projectId)
		{
			return PartialView("_multipleResolve", new BugResolveViewModel { Id = ids, ProjectId = projectId });
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Resolve(BugResolveViewModel model)
		{
			if (!ModelState.IsValid)
				return Json(new { result = AjaxResults.Error });


			if (model != null && !model.Id.IsEmptyOrNull())
			{
				DateTime fixDate = DateTime.MinValue;
				DateTime.TryParse(model.Result2, out fixDate);
				if (model.Result.ToGuid(Guid.Empty) == BugKeys.Resolved && fixDate.IsEmpty())
					fixDate = DateTime.Now;

				Guid assignId = GetUserInfo().UserId;

				var bugs = GetBugListByIds(model.Id);

				db.BeginTrans();

				try
				{
					foreach (var bug in bugs)
					{
						var id = bug.BugId;
						db.OperationDal.Insert(new Operation(bug.Projectid, id, BugKeys.BugResolveGuid, model.Result, null, DateTime.Now, assignId, model.Remark));
						db.BugDal.UpdatePartial(id, new { ResolveStatus = model.Result, FixDate = fixDate, BugStatus = model.Result.ToGuid(Guid.Empty) == BugKeys.Resolving ? BugKeys.readyToResolve : BugKeys.hasResolve });
					}

					db.Commit();
				}
				catch
				{
					db.Rollback();

					return Json(new
					{
						result = AjaxResults.Error
					});
				}
			}

			return Json(new
			{
				result = AjaxResults.Success
			});
		}


		//GET  /Bug/Confirm
		//GET  /Bug/MultipleConfirm
		//POST-AJAX /Bug/Confirm

		public ActionResult Confirm(Guid id)
		{
			Bug bug = db.BugDal.PrimaryGet(id);

			var existConfrim = OperationHelper.GetOperation(bug.BugId, BugKeys.BugConfirmGuid);

			return PartialView("_confirm",
			   new BugConfrimViewModel
			   {
				   Id = bug.BugId.ToString(),
				   Name = bug.BugName,
				   ProjectId = bug.Projectid,
				   SortId = bug.SortId,
				   Remark = existConfrim?.Content,
				   Result = existConfrim?.OperationResult,
			   });
		}

		public ActionResult MultipleConfirm(string ids, Guid projectId)
		{
			return PartialView("_multipleConfirm", new BugConfrimViewModel { Id = ids, ProjectId = projectId });
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Confirm(BugConfrimViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}


			if (model != null && !model.Id.IsEmptyOrNull())
			{
				Guid assignId = GetUserInfo().UserId;

				var bugs = GetBugListByIds(model.Id);

				db.BeginTrans();

				try
				{
					foreach (var bug in bugs)
					{
						var id = bug.BugId;
						db.OperationDal.Insert(new Operation(bug.Projectid, id, BugKeys.BugConfirmGuid, model.Result, null, DateTime.Now, assignId, model.Remark));

						db.BugDal.UpdatePartial(id, new { ConfirmResult = model.Result, BugStatus = model.Result.ToGuid(Guid.Empty) == BugKeys.ConfrimYes ? BugKeys.readyToResolve : BugKeys.hasResolve });
					}

					db.Commit();
				}
				catch
				{
					db.Rollback();

					return Json(new
					{
						result = AjaxResults.Error
					});
				}
			}

			return Json(new
			{
				result = AjaxResults.Success
			});
		}



		//GET  /Bug/Relative
		//POST-AJAX /Bug/Relative

		public ActionResult Relative(Guid id)
		{
			Bug bug = db.BugDal.PrimaryGet(id);
			var model = MappingOperationViewModel(bug, BugKeys.BugRelativeGuid);

			// 关联的任务
			bug.RelativeTasks = RTPBRelationHelper.GetBugRelativeTasks(id, db);
			model.Result = RTPBRelationHelper.GetTaskIds(bug.RelativeTasks);

			// 关联的需求
			bug.RelativeRequires = RTPBRelationHelper.GetBugRelativeRequires(id, db);
			model.Result2 = RTPBRelationHelper.GetRequireIds(bug.RelativeRequires);

			return PartialView("_relative", model);
		}

		public ActionResult MultipleRelative(string ids, Guid projectId)
		{
			return PartialView("_multipleRelative", new OperationViewModel { Id = ids, ProjectId = projectId });
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Relative(OperationViewModel model)
		{
			if (!ModelState.IsValid || !model.IsValid())
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			Guid assignId = GetUserInfo().UserId;

			var bugs = GetBugListByIds(model.Id);

			db.BeginTrans();

			try
			{
				foreach (var bug in bugs)
				{
					var id = bug.BugId;
					db.OperationDal.Insert(new Operation(bug.Projectid, id, BugKeys.BugRelativeGuid, BugKeys.BugRelativeGuid.ToString(), null, DateTime.Now, assignId, model.Remark));

					string[] relativeTaskIds = model.Result.Split(',');
					string[] relativeRequireIds = model.Result2.Split(',');
					RTPBRelationHelper.BindRelationBetweenTasksAndBug(relativeTaskIds.ConvertToGuidArray(), id, db);
					RTPBRelationHelper.BindRelationBetweenRequiresAndBug(relativeRequireIds.ConvertToGuidArray(), id, db);
				}

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


		// POST /Bug/GetProjectBugs

		[HttpPost]
		public ActionResult GetProjectBugs(Guid projectId)
		{
			if (projectId.IsEmptyGuid())
			{
				return Json(new { });
			}

			var results = BugHelper.GetBugsByProjectId(projectId, db);

			return Json(new
			{
				rows = results.Select(x => new { id = x.BugId, text = x.BugName }).ToList()
			});

		}

		private List<Project> MyJoinedProjects() => ProjectrHelper.UserJoinedProjects(GetUserInfo().UserId, db).FindAll(p => p.ProjectStatus != ProjectKeys.CompleteStatus & p.ProjectStatus != ProjectKeys.DeleteStatus);

		private OperationViewModel MappingOperationViewModel(Bug bug, Guid operationTypeId)
		{
			if (bug == null)
				return new OperationViewModel();

			var existReviewResult = OperationHelper.GetOperation(bug.BugId, operationTypeId);

			return
			   new OperationViewModel
			   {
				   Id = bug.BugId.ToString(),
				   Name = bug.BugName,
				   ProjectId = bug.Projectid,
				   SortId = bug.SortId,
				   Remark = existReviewResult?.Content,
				   Result = existReviewResult?.OperationResult,
				   Result2 = existReviewResult?.OperationResult2
			   };
		}

		private int GetBugMaxSortNo(Guid projectId, APDBDef db)
		{
			var result = APQuery.select(b.SortId.Max().As("SortId"))
				.from(b)
				.where(b.Projectid == projectId)
				.query(db, r => new { sortId = b.SortId.GetValue(r, "SortId") }).FirstOrDefault();

			if (result == null) return 1;

			return result.sortId;
		}


		private List<Bug> GetBugListByIds(string id)
		{
			Guid[] ids = id.Split(',').ConvertToGuidArray(); ;
			return db.BugDal.ConditionQuery(b.BugId.In(ids), null, null, null);
		}


	}

}