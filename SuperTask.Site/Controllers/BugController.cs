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
		static APDBDef.TaskBugsTableDef tb = APDBDef.TaskBugs;
		static APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;

		//GET  /Task/List
		//POST-AJAX /Task/List

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
					 b.ManagerId, u.UserName, u2.UserName.As("creator"))
			   .from(b,
			   u.JoinInner(u.UserId == b.ManagerId),
			   u2.JoinInner(u2.UserId == b.CreatorId)
			   )
			   .where(b.Projectid == projectId);

			var results = query.order_by(b.SortId.Asc)
			   .query(db, r => new Bug
			   {
				   BugId = b.BugId.GetValue(r),
				   BugName = b.BugName.GetValue(r),
				   ManagerId = b.ManagerId.GetValue(r),
				   Manager = u.UserName.GetValue(r),
				   Creator=u2.UserName.GetValue(r, "creator"),
				   CreateDate=b.CreateDate.GetValue(r),
				   BugLevel = b.BugLevel.GetValue(r),
				   SortId = b.SortId.GetValue(r),
				   BugType = b.BugType.GetValue(r),
				   BugStatus = b.BugStatus.GetValue(r),
				   FixDate=b.FixDate.GetValue(r)
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


		//GET  /Task/Edit
		//POST-AJAX /Task/Edit

		public ActionResult Edit(Guid? id)
		{
			Bug bug = new Bug();
			if (id != null && !id.Value.IsEmpty())
				bug = db.BugDal.PrimaryGet(id.Value);
				
			ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);

			// 我参与的项目
			ViewBag.Projects = MyJoinedProjects();

			// 关联的任务id
			ViewBag.RelativeTaskId = db.TaskBugsDal.ConditionQuery(tb.BugId == id,null,null,null).FirstOrDefault();

			return PartialView(bug);
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

			var user = GetUserInfo();

			db.BeginTrans();

			try
			{
				if (bug.BugId.IsEmptyGuid())
				{
					bug.BugId = Guid.NewGuid();
					bug.CreateDate = DateTime.Now;
					bug.CreatorId = user.UserId;

					db.BugDal.Insert(bug);
				}
				else
				{
					bug.ModifyDate = DateTime.Now;

					db.BugDal.Update(bug);
				}

				AttachmentHelper.UploadBugsAttachment(bug, db);

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

			return Json(new
			{
				result = AjaxResults.Success
			});

		}


		//GET  /Task/Fix
		//POST-AJAX /Task/Fix

		public ActionResult Fix(Guid id)
		{
			return View();
		}

		[HttpPost]
		public ActionResult Fix(Guid status,string comment)
		{
			return Json(new { });
		}


		//GET  /Confirm/Fix
		//POST-AJAX /Confirm/Fix

		public ActionResult Confirm(Guid id)
		{
			return View();
		}

		[HttpPost]
		public ActionResult Confirm(Guid status, string comment)
		{
			return Json(new { });
		}



		private List<Project> MyJoinedProjects() => ProjectrHelper.UserJoinedProjects(GetUserInfo().UserId, db).FindAll(p => p.ProjectStatus != ProjectKeys.CompleteStatus & p.ProjectStatus != ProjectKeys.DeleteStatus);


	}

}