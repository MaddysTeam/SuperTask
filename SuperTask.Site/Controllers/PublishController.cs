﻿using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

	public class PublishController : BaseController
	{

		static APDBDef.PublishTableDef p = APDBDef.Publish;
		static APDBDef.UserInfoTableDef u = APDBDef.UserInfo;
		static APDBDef.WorkTaskTableDef t = APDBDef.WorkTask;
		static APDBDef.OperationTableDef o = APDBDef.Operation;

		//GET  Publish/List
		//POST-AJAX Publish/List

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

			var query = APQuery.select(p.PublishId, p.PublishName, p.PublishType,
				 p.SortId, p.PublishStatus, p.CloseDate,
				  p.ManagerId, u.UserName, p.CreateDate, u2.UserName.As("creator"))
			   .from(p,
			   u.JoinLeft(u.UserId == p.ManagerId),
			   u2.JoinLeft(u2.UserId == p.CreatorId)
			   );

			if (projectId != AppKeys.SelectAll)
				query = query.where(p.Projectid == projectId);

			var results = query.order_by(p.SortId.Asc)
			   .query(db, r => new Publish
			   {
				   PublishId = p.PublishId.GetValue(r),
				   PublishName = p.PublishName.GetValue(r),
				   ManagerId = p.ManagerId.GetValue(r),
				   Manager = u.UserName.GetValue(r),
				   Creator = u2.UserName.GetValue(r, "creator"),
				   CreateDate = p.CreateDate.GetValue(r),          
				   SortId = p.SortId.GetValue(r),
				   PublishType = p.PublishType.GetValue(r),
				   PublishStatus = p.PublishStatus.GetValue(r),
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

			if (statusId != AppKeys.SelectAll)
			{
				results = results.FindAll(t => t.PublishStatus == statusId);
			}
			if (!string.IsNullOrEmpty(searchPhrase))
			{
				results = results.FindAll(b => b.PublishName.IndexOf(searchPhrase) >= 0 || b.SortId.ToString() == searchPhrase);
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


		//GET  Publish/Edit
		//POST-AJAX Publish/Edit

		public ActionResult Edit(Guid? id)
		{
			// 所有人员
			ViewBag.Users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);

			// 我参与的项目
			ViewBag.Projects = MyJoinedProjects();

			Publish Publish = new Publish();

			if (id != null && !id.Value.IsEmpty())
			{
				Publish = db.PublishDal.PrimaryGet(id.Value);

				return View("Edit", Publish);
			}

			return PartialView("Add", Publish);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(Publish publish, FormCollection collection)
		{
			if (!ModelState.IsValid)
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}

			if (publish.Projectid == BugKeys.SelectAll)
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
				if (publish.PublishId.IsEmptyGuid())
				{
					publish.PublishId = Guid.NewGuid();
					publish.CreateDate = DateTime.Now;
					publish.CreatorId = user.UserId;
					publish.PublishStatus = PublishKeys.StatusGuid;
	
					db.PublishDal.Insert(publish);
				}
				else
				{
					publish.ModifyDate = DateTime.Now;
					publish.ModifiedBy = user.UserId;

					db.PublishDal.Update(publish);
				}


				// add attachment
				AttachmentHelper.UploadPublishAttachment(publish, user.UserId, db);

				//add user to project resurce if not exits
				ResourceHelper.AddUserToResourceIfNotExist(publish.Projectid, Guid.Empty, publish.ManagerId, ResourceKeys.OtherType, db);


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


		//GET Publish/Detail

		public ActionResult Detail(Guid id)
		{
			var Publish = db.PublishDal.PrimaryGet(id);
			var users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
			Publish.Manager = users.Find(x => x.UserId == Publish.ManagerId)?.RealName;

			var operations = db.OperationDal.ConditionQuery(o.ItemId == id, o.OperationDate.Desc, null, null);
			var operationHistory = new List<OperationHistoryViewModel>();
			foreach (var item in operations)
			{
				operationHistory.Add(new OperationHistoryViewModel
				{
					Date = item.OperationDate.ToyyMMdd(),
					Operator = GetUserInfo().RealName,
					ResultId = item.OperationResult,
					Result = PublishKeys.OperationResultDic[item.OperationResult.ToGuid(Guid.Empty)],
				}
			);
			}

			Publish.OperationHistory = operationHistory;

			ViewBag.Attahcments = AttachmentHelper.GetAttachments(Publish.Projectid, Publish.PublishId, db);

			return View(Publish);
		}


		//GET Publish/Handle
		//POST-AJAX Publish/Handle

		public ActionResult Handle(Guid id)
		{
			Publish Publish = db.PublishDal.PrimaryGet(id);

			var viewModel = MappingOperationViewModel(Publish,PublishKeys.HandleGuid);

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
			var existsReviews = db.PublishDal.ConditionQuery(p.PublishId.In(ids), null, null, null);


			db.BeginTrans();

			try
			{
				int workhours = 0;
				foreach (var item in existsReviews)
				{
					db.OperationDal.Insert(new Operation(model.ProjectId, item.PublishId, PublishKeys.HandleGuid, model.Result, model.Result2, DateTime.Now, assignId, model.Remark));

					int.TryParse(model.Result2, out workhours);
					db.PublishDal.UpdatePartial(item.PublishId, new { PublishStatus = model.Result.ToGuid(Guid.Empty) });
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


		//GET Publish/Close
		//POST-AJAX Publish/Close

		public ActionResult Close(Guid id)
		{
			Publish Publish = db.PublishDal.PrimaryGet(id);

			var viewModel = MappingOperationViewModel(Publish, PublishKeys.StatusGuid);
			viewModel.Result = PublishKeys.Close.ToString();

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
					db.OperationDal.Insert(new Operation(model.ProjectId, id, PublishKeys.StatusGuid, model.Result, null, DateTime.Now, assignId, model.Remark));

					db.PublishDal.UpdatePartial(id, new { PublishStatus = model.Result.ToGuid(Guid.Empty), CloseDate = DateTime.Now });
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

		private OperationViewModel MappingOperationViewModel(Publish Publish,Guid operationTypeId)
		{
			if (Publish == null)
				return new OperationViewModel();

			var existReviewResult = OperationHelper.GetOperation(Publish.PublishId, operationTypeId);

			return
			   new OperationViewModel
			   {
				   Id = Publish.PublishId.ToString(),
				   Name = Publish.PublishName,
				   ProjectId = Publish.Projectid,
				   SortId = Publish.SortId,
				   Remark = existReviewResult.Content,
				   Result = existReviewResult.OperationResult,
			   };
		}

	}

}