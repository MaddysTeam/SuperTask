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

   public class RequireController : BaseController
   {

		static APDBDef.RequireTableDef re = APDBDef.Require;
		static APDBDef.UserInfoTableDef u = APDBDef.UserInfo;
		static APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
		static APDBDef.OperationTableDef o = APDBDef.Operation;

		//GET  Require/List
		//POST-AJAX Require/List

		public ActionResult List()
		{
			ViewBag.Projects = MyJoinedProjects();

			return View();
		}

		[HttpPost]
		public ActionResult List(Guid projectId, Guid levelId, Guid statusId, bool isAssign,bool isJoin,
					 int current, int rowCount, AjaxOrder sort, string searchPhrase)
		{
			ThrowNotAjax();

			var user = GetUserInfo();
			var u2 = APDBDef.UserInfo.As("creator");

			var query = APQuery.select(re.RequireId, re.RequireName, re.RequireType,
				  re.RequireLevel, re.SortId, re.RequireStatus, re.EstimateEndDate,
				  re.ManagerId, u.UserName, re.CreateDate, u2.UserName.As("creator"))
			   .from(re,
			   u.JoinLeft(u.UserId == re.ManagerId),
			   u2.JoinLeft(u2.UserId == re.CreatorId)
			   );

			if (projectId != AppKeys.SelectAll)
				query = query.where(re.Projectid == projectId);

			var results = query.order_by(re.SortId.Asc)
			   .query(db, r => new Require
			   {
				   RequireId = re.RequireId.GetValue(r),
				   RequireName = re.RequireName.GetValue(r),
				   ManagerId = re.ManagerId.GetValue(r),
				   Manager = u.UserName.GetValue(r),
				   Creator = u2.UserName.GetValue(r, "creator"),
				   CreateDate = re.CreateDate.GetValue(r),
				   RequireLevel = re.RequireLevel.GetValue(r),
				   SortId = re.SortId.GetValue(r),
				   RequireType = re.RequireType.GetValue(r),
				   RequireStatus = re.RequireStatus.GetValue(r),
				   EstimateEndDate = re.EstimateEndDate.GetValue(r),
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
				results = results.FindAll(b => b.RequireLevel == levelId);
			}
			//if (typeId != AppKeys.SelectAll)
			//{
			//	results = results.FindAll(b => b.RequireType == typeId);
			//}
			if (statusId != AppKeys.SelectAll)
			{
				results = results.FindAll(t => t.RequireStatus == statusId);
			}
			if (!string.IsNullOrEmpty(searchPhrase))
			{
				results = results.FindAll(b => b.RequireName.IndexOf(searchPhrase) >= 0 || b.SortId.ToString() == searchPhrase);
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


		//GET  Require/Edit
		//POST-AJAX Require/Edit

		public ActionResult Edit(Guid? id)
		{
			// 所有人员
			ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);

			// 我参与的项目
			ViewBag.Projects = MyJoinedProjects();

			Require require = new Require();

			if (id != null && !id.Value.IsEmpty())
			{
				require = db.RequireDal.PrimaryGet(id.Value);

				return View("Edit", require);
			}

			return PartialView("Add", require);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(Require require, FormCollection collection)
		{
			if (!ModelState.IsValid)
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			if (require.Projectid == BugKeys.SelectAll)
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
				if (require.RequireId.IsEmptyGuid())
				{
					require.RequireId = Guid.NewGuid();
					require.CreateDate = DateTime.Now;
					require.CreatorId = user.UserId;
               require.RequireStatus = RequireKeys.readyToReview;

               db.RequireDal.Insert(require);
				}
				else
				{
					require.ModifyDate = DateTime.Now;
					require.ModifiedBy = user.UserId;

					db.RequireDal.Update(require);
				}


				// add attachment
				AttachmentHelper.UploadRequireAttachment(require, user.UserId, db);

				//add user to project resurce if not exits
				ResourceHelper.AddUserToResourceIfNotExist(require.Projectid, Guid.Empty, require.ManagerId, ResourceKeys.OtherType, db);


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


		//GET Require/Detail

		public ActionResult Detail(Guid id)
		{
			var require = Require.PrimaryGet(id);
			var users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
			require.Manager = users.Find(x => x.UserId == require.ManagerId)?.RealName;

			var operations = db.OperationDal.ConditionQuery(o.ItemId == id, o.OperationDate.Desc, null, null);
			var operationHistory = new List<OperationHistoryViewModel>();
			foreach (var item in operations)
			{
				operationHistory.Add(new OperationHistoryViewModel
				{
					Date = item.OperationDate.ToString("yyyy-MM-dd"),
					Operator = GetUserInfo().RealName,
					ResultId = item.OperationResult,
				}
			);
			}

			require.OperationHistory = operationHistory;

			//require.RelativeTasks.AddRange(GetRelativeTasks(id));

			ViewBag.Attahcments = AttachmentHelper.GetAttachments(require.Projectid, require.RequireId, db);

			return View(require);
		}


		//GET Require/Review
		//POST-AJAX Require/Review

		public ActionResult Review()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Review(RequireReviewViewModel model)
		{
			return Json(new { });
		}


		//GET Require/Handle
		//POST-AJAX Require/Handle

		public ActionResult Handle()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Handle(RequireHandleViewModel model)
		{
			return Json(new { });
		}


		//GET Require/Close
		//POST-AJAX Require/Close

		public ActionResult Close()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Close(CloseViewModel model)
		{
			return Json(new { });
		}


		private List<Project> MyJoinedProjects() => ProjectrHelper.UserJoinedAvailableProject(GetUserInfo().UserId, db);

	}

}