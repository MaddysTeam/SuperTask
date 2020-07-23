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
		public ActionResult List(Guid projectId, Guid levelId, Guid statusId, bool isAssign, bool isJoin,
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
					require.ReviewStatus = RequireKeys.ReviewWaiting;

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
					Date = item.OperationDate.ToyyMMdd(),
					Operator = GetUserInfo().RealName,
					ResultId = item.OperationResult,
					Result = RequireKeys.OperationResultDic[item.OperationResult.ToGuid(Guid.Empty)],
				}
			);
			}

			require.OperationHistory = operationHistory;

			ViewBag.Attahcments = AttachmentHelper.GetAttachments(require.Projectid, require.RequireId, db);

			return View(require);
		}


		//GET Require/Review
		//POST-AJAX Require/Review

		public ActionResult Review(Guid id)
		{
			Require require = db.RequireDal.PrimaryGet(id);

			var viewModel = MappingOperationViewModel(require, RequireKeys.ReviewResultGuid);

			return PartialView("_review", viewModel);
		}


		public ActionResult MultipleReview(string ids, Guid projectId)
		{
			return PartialView("_multipleReview", new OperationViewModel { Id = ids, ProjectId = projectId });
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Review(OperationViewModel model)
		{
			if (!ModelState.IsValid || !model.IsValid())
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			db.BeginTrans();

			try
			{
				Guid[] ids = model.Id.Split(',').ConvertToGuidArray();
				Guid assignId = GetUserInfo().UserId;

				foreach (var id in ids)
				{
					db.OperationDal.Insert(new Operation(model.ProjectId, id, RequireKeys.ReviewResultGuid, model.Result, null, DateTime.Now, assignId, model.Remark));

					db.RequireDal.UpdatePartial(id, new { ReviewDate = DateTime.Now, ReviewStatus = model.Result });
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


		//GET Require/Handle
		//POST-AJAX Require/Handle

		public ActionResult Handle(Guid id)
		{
			Require require = db.RequireDal.PrimaryGet(id);

			var viewModel = MappingOperationViewModel(require, RequireKeys.ReviewHandleGuid);

			return PartialView("_handle", viewModel);
		}

		[HttpPost]
		public ActionResult Handle(OperationViewModel model)
		{
			if (!ModelState.IsValid || !model.IsValid())
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			Guid[] ids = model.Id.Split(',').ConvertToGuidArray();
			Guid assignId = GetUserInfo().UserId;
			var existsReviews = db.RequireDal.ConditionQuery(re.RequireId.In(ids), null, null, null);


			db.BeginTrans();

			try
			{
				int workhours = 0;
				foreach (var item in existsReviews)
				{
					db.OperationDal.Insert(new Operation(model.ProjectId, item.RequireId, RequireKeys.ReviewHandleGuid, model.Result, model.Result2, DateTime.Now, assignId, model.Remark));

					int.TryParse(model.Result2, out workhours);
					db.RequireDal.UpdatePartial(item.RequireId, new { ReviewDate = DateTime.Now, RequireStatus = model.Result, WorkHours = item.WorkHours + workhours });
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


		//GET Require/Close
		//POST-AJAX Require/Close

		public ActionResult Close(Guid id)
		{
			Require require = db.RequireDal.PrimaryGet(id);

			var viewModel = MappingOperationViewModel(require, RequireKeys.StatusGuid);
			viewModel.Result = RequireKeys.Close.ToString();

			return PartialView("_close", viewModel);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Close(OperationViewModel model)
		{
			if (!ModelState.IsValid || !model.IsValid())
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			db.BeginTrans();

			try
			{
				Guid[] ids = model.Id.Split(',').ConvertToGuidArray();
				Guid assignId = GetUserInfo().UserId;

				foreach (var id in ids)
				{
					db.OperationDal.Insert(new Operation(model.ProjectId, id, RequireKeys.StatusGuid, model.Result, null, DateTime.Now, assignId, model.Remark));

					db.RequireDal.UpdatePartial(id, new { RequireStatus = model.Result.ToGuid(Guid.Empty), CloseDate=DateTime.Now });
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


		private List<Project> MyJoinedProjects() => ProjectrHelper.UserJoinedAvailableProject(GetUserInfo().UserId, db);

		private OperationViewModel MappingOperationViewModel(Require require,Guid operationTypeId)
		{
			if (require == null)
				return new OperationViewModel();

			var existReviewResult = OperationHelper.GetOperation(require.RequireId, operationTypeId);

			return
			   new OperationViewModel
			   {
				   Id = require.RequireId.ToString(),
				   Name = require.RequireName,
				   ProjectId = require.Projectid,
				   SortId = require.SortId,
				   Remark = existReviewResult?.Content,
				   Result = existReviewResult?.OperationResult,
			   };
		}

	}

}