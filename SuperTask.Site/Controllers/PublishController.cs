using Business;
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
		public ActionResult List(Guid projectId, Guid statusId, bool isAssign, bool isJoin,
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

			Publish publish = new Publish();

			if (id != null && !id.Value.IsEmpty())
			{
				publish = db.PublishDal.PrimaryGet(id.Value);
				var relativeTasks = RTPBRelationHelper.GetPublishRelativeTasks(id.Value, db);
				publish.RelativeTaskIds = RTPBRelationHelper.GetTaskIds(relativeTasks);
				publish.RelativeTasks = relativeTasks;

				var relativeRequires = RTPBRelationHelper.GetPublishRelativeRequires(id.Value, db);
				publish.RelativeRequireIds = RTPBRelationHelper.GetRequireIds(relativeRequires);
				publish.RelativeRequires = relativeRequires;

				return View("Edit", publish);
			}


			return PartialView("Add", publish);
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
					var sortId = GetMaxSortNo(publish.Projectid, db);
					publish.SortId = ++sortId;

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
				var attachments = AttachmentHelper.UploadPublishAttachment(publish, user.UserId, db);

				ProjectHelper.UploadFileToProjectFolder(publish.Projectid, ShareFolderKeys.MaintainType, attachments.AttachmentId, user.UserId, db);

				// add user to project resurce if not exits
				ResourceHelper.AddUserToResourceIfNotExist(publish.Projectid, Guid.Empty, publish.ManagerId, ResourceKeys.OtherType, db);

				// add relationship between publish and task
				string[] relativeTaskIds = publish.RelativeTaskIds.Split(',');
				RTPBRelationHelper.BindRelationBetweenTasksAndPublish(relativeTaskIds.ConvertToGuidArray(), publish.PublishId, db);

				string[] relativeRequireIds = publish.RelativeRequireIds.Split(',');
				RTPBRelationHelper.BindRelationBetweenRequiresAndPublish(relativeRequireIds.ConvertToGuidArray(), publish.PublishId, db);


				db.Commit();
			}
			catch (Exception e)
			{
				db.Rollback();

				return Json(new
				{
					result = AjaxResults.Error,
					msg = e.Message
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
			var publish = db.PublishDal.PrimaryGet(id);
			var users = db.UserInfoDal.ConditionQuery(u.IsDelete == false, null, null, null);
			publish.Manager = users.Find(x => x.UserId == publish.ManagerId)?.RealName;
			publish.Creator = users.Find(x => x.UserId == publish.CreatorId)?.RealName;
			publish.OperationHistory = GetOperationHistoryViewModels(id);

			publish.RelativeTasks = RTPBRelationHelper.GetPublishRelativeTasks(id, db);
			publish.RelativeRequires = RTPBRelationHelper.GetPublishRelativeRequires(id, db);

			ViewBag.Attahcments = AttachmentHelper.GetAttachments(publish.Projectid, publish.PublishId, db);
			ViewBag.Users = users;

			return View(publish);
		}


		//GET Publish/Handle
		//POST-AJAX Publish/Handle

		public ActionResult Handle(Guid id)
		{
			Publish Publish = db.PublishDal.PrimaryGet(id);

			var viewModel = MappingOperationViewModel(Publish, PublishKeys.HandleGuid);

			return PartialView("_handle", viewModel);
		}

		public ActionResult MultipleHandle(string ids, Guid projectId)
		{
			return PartialView("_multipleHandle", new OperationViewModel { Id = ids, ProjectId = projectId });
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Handle(OperationViewModel model)
		{
			if (!ModelState.IsValid || !model.IsValid())
			{
				return Json(new
				{
					result = AjaxResults.Error
				});
			}


			Guid assignId = GetUserInfo().UserId;

			var publishs = GetPublishListByIds(model.Id);


			db.BeginTrans();

			try
			{
				Guid publishStats = PublishKeys.HandleMapping[model.Result.ToGuid(Guid.Empty)];
				foreach (var publish in publishs)
				{
					var id = publish.PublishId;
					db.OperationDal.Insert(new Operation(publish.Projectid, id, PublishKeys.HandleGuid, model.Result, model.Result2, DateTime.Now, assignId, model.Remark));
					db.PublishDal.UpdatePartial(id, new { PublishStatus = publishStats });
				}

				db.Commit();
			}
			catch (Exception e)
			{
				db.Rollback();

				return Json(new
				{
					result = AjaxResults.Error,
					msg = e.Message
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
			Publish publish = db.PublishDal.PrimaryGet(id);
			OperationViewModel viewModel = MappingOperationViewModel(publish, PublishKeys.StatusGuid);
			viewModel.Result = PublishKeys.Close.ToString();
			viewModel.ProjectId = publish.Projectid;
			viewModel.Result = PublishKeys.Close.ToString();

			return PartialView("_close", viewModel);
		}


		public ActionResult MultipleClose(string ids, Guid projectId)
		{
			return PartialView("_multipleClose", new OperationViewModel { Id = ids, ProjectId = projectId });
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

			Guid assignId = GetUserInfo().UserId;

			var publishs = GetPublishListByIds(model.Id);

			db.BeginTrans();

			try
			{
				foreach (var publish in publishs)
				{
					var id = publish.PublishId;
					db.OperationDal.Insert(new Operation(publish.Projectid, id, PublishKeys.StatusGuid, PublishKeys.HandleClose.ToString(), null, DateTime.Now, assignId, model.Remark));

					db.PublishDal.UpdatePartial(id, new { PublishStatus = PublishKeys.Close, CloseDate = DateTime.Now });
				}

				db.Commit();
			}
			catch (Exception e)
			{
				db.Rollback();

				return Json(new
				{
					result = AjaxResults.Error,
					msg=e.Message
				});
			}


			return Json(new
			{
				result = AjaxResults.Success
			});
		}


		//GET Publish/Relative
		//POST-AJAX Publish/Relative

		public ActionResult Relative(Guid id)
		{
			Publish publish = db.PublishDal.PrimaryGet(id);
			var model = MappingOperationViewModel(publish, PublishKeys.RelativeGuid);
			var relativeTasks = RTPBRelationHelper.GetPublishRelativeTasks(id, db);
			var relativeRequires = RTPBRelationHelper.GetPublishRelativeRequires(id, db);
			model.Result = RTPBRelationHelper.GetTaskIds(relativeTasks);  // relative task ids
			model.Result2 = RTPBRelationHelper.GetRequireIds(relativeRequires); // relative require ids

			return PartialView("_relative", model);
		}

		[HttpPost]
		public ActionResult Relative(OperationViewModel model)
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
					db.OperationDal.Insert(new Operation(model.ProjectId, id, PublishKeys.RelativeGuid, PublishKeys.RelativeGuid.ToString(), null, DateTime.Now, assignId, model.Remark));

					string[] relativeTaskIds = model.Result.Split(',');
					string[] relativeRequireIds = model.Result2.Split(',');
					RTPBRelationHelper.BindRelationBetweenTasksAndPublish(relativeTaskIds.ConvertToGuidArray(), id, db);
					RTPBRelationHelper.BindRelationBetweenRequiresAndPublish(relativeRequireIds.ConvertToGuidArray(), id, db);

				}

				db.Commit();
			}
			catch (Exception e)
			{
				db.Rollback();

				return Json(new
				{
					result = AjaxResults.Error,
					msg=e.Message
				});
			}


			return Json(new
			{
				result = AjaxResults.Success
			});
		}


		//POST-Ajax  Publish/GetProjectPublishs

		[HttpPost]
		public ActionResult GetProjectPublishs(Guid projectId)
		{
			if (projectId.IsEmptyGuid())
			{
				return Json(new { });
			}

			var results = db.PublishDal.ConditionQuery(p.Projectid == projectId, null, null, null);

			return Json(new
			{
				rows = results.Select(x => new { id = x.PublishId, text = x.PublishName }).ToList()
			});

		}


		private List<Project> MyJoinedProjects() => ProjectHelper.UserJoinedAvailableProject(GetUserInfo().UserId, db);

		private OperationViewModel MappingOperationViewModel(Publish Publish, Guid operationTypeId)
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
				   Remark = existReviewResult?.Content,
				   Result = existReviewResult?.OperationResult,
				   Result2 = existReviewResult?.OperationResult2
			   };
		}

		private List<OperationHistoryViewModel> GetOperationHistoryViewModels(Guid id)
		{
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
			return operationHistory;
		}


		private int GetMaxSortNo(Guid projectId, APDBDef db)
		{
			var result = APQuery.select(p.SortId.Max().As("SortId"))
			   .from(p)
			   .where(p.Projectid == projectId)
			   .query(db, r => new { sortId = p.SortId.GetValue(r, "SortId") }).FirstOrDefault();

			if (result == null) return 1;

			return result.sortId;
		}


		private List<Publish> GetPublishListByIds(string id)
		{
			Guid[] ids = id.Split(',').ConvertToGuidArray(); ;
			return db.PublishDal.ConditionQuery(p.PublishId.In(ids), null, null, null);
		}

	}

}