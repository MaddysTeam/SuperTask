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
				var relativeTasks = RTPBRelationHelper.GetBugRelativeTasks(id.Value, db);
            bug.RelativeTaskIds = RTPBRelationHelper.GetTaskIds(relativeTasks);
            bug.RelativeTasks = relativeTasks;

            //var relativeRequires= RTPBRelationHelper.getre
            bug.RelativeRequireIds = "";
            bug.RelativeRequires = new List<Require>();
             
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
            RTPBRelationHelper.BindRelationBetweenTasksAndBug(taskids.ConvertToGuidArray(), bug.BugId, db);


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

			var operations = db.OperationDal.ConditionQuery(o.ItemId == id, o.OperationDate.Desc, null, null);
			var operationHistory = new List<OperationHistoryViewModel>();
			foreach (var item in operations)
			{
				operationHistory.Add(new OperationHistoryViewModel
				{
					Date = item.OperationDate.ToString("yyyy-MM-dd"),
					Operator = GetUserInfo().RealName,
					Result = BugKeys.OperationResultDic[item.OperationResult.ToGuid(Guid.Empty)],
					ResultId = item.OperationResult,
				}
			 );
			}

			bug.OperationHistory = operationHistory;

			bug.RelativeTasks = RTPBRelationHelper.GetBugRelativeTasks(id, db);

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

			if (model.ProjectId.IsEmpty())
				return Json(new { result = AjaxResults.Error });

			if (model != null && !model.Id.IsEmptyOrNull())
			{
				db.BeginTrans();

				try
				{
					Guid[] ids = model.Id.Split(',').ConvertToGuidArray();
					Guid assignId = GetUserInfo().UserId;

					foreach (var id in ids)
					{
						db.OperationDal.Insert(new Operation(model.ProjectId, id, BugKeys.BugResolveGuid, model.Result, null, DateTime.Now, assignId, model.Remark));
						db.BugDal.UpdatePartial(id, new { ResolveStatus = model.Result, BugStatus = model.Result.ToGuid(Guid.Empty) == BugKeys.Resolving ? BugKeys.readyToResolve : BugKeys.hasResolve });
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

			if (model.ProjectId.IsEmpty())
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			if (model != null && !model.Id.IsEmptyOrNull())
			{
				// var existConfrims = db.OperationDal.ConditionQuery(o.ProjectId == model.ProjectId, null, null, null);

				db.BeginTrans();

				try
				{
					Guid[] ids = model.Id.Split(',').ConvertToGuidArray();
					Guid assignId = GetUserInfo().UserId;

					foreach (var id in ids)
					{
						db.OperationDal.Insert(new Operation(model.ProjectId, id, BugKeys.BugConfirmGuid, model.Result, null, DateTime.Now, assignId, model.Remark));

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

			return PartialView("_relative", bug);
		}

		[HttpPost]
		public ActionResult Relative(Guid status, string comment)
		{
			return Json(new { });
		}


		// POST /Bug/GetProjectBugs

		[HttpPost]
		public ActionResult GetProjectBugs(Guid projectId)
		{
			if (projectId.IsEmptyGuid())
			{
				return Json(new { });
			}

         var results = BugHelper.GetBugsByProjectId(projectId,db);

         return Json(new
			{
				rows = results.Select(x => new { id = x.BugId, text = x.BugName }).ToList()
			});

		}

		private List<Project> MyJoinedProjects() => ProjectrHelper.UserJoinedProjects(GetUserInfo().UserId, db).FindAll(p => p.ProjectStatus != ProjectKeys.CompleteStatus & p.ProjectStatus != ProjectKeys.DeleteStatus);

	}

}