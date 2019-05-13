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

	public class ProjectController : BaseController
	{

		static APDBDef.ProjectTableDef p = APDBDef.Project;


		// GET: Project/Search
		// Post-ajax: Project/Search

		public ActionResult Search()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Search(Guid searchTypeId, string statusIds, int current, int rowCount, AjaxOrder sort, string searchPhrase)
		{
			ThrowNotAjax();

			var u = APDBDef.UserInfo;
			var r = APDBDef.Resource;
			var user = GetUserInfo();

			var query = APQuery.select(p.ProjectId, p.RealCode, p.ProjectType, p.ProjectExecutor, p.ProjectName, p.RateOfProgress,
									   p.ProjectOwner, p.ProjectStatus, p.OrgId, u.UserName.As("managerName")
									   )
			   .from(p,
					 u.JoinLeft(u.UserId == p.ManagerId)
					 );
			//.where(p.ProjectStatus.NotIn(ProjectKeys.DeleteStatus));

			if (searchTypeId == ProjectKeys.SearchMyProject && !user.IsBoss)
				query.where_and(p.ManagerId == user.UserId);

			if (searchTypeId == ProjectKeys.SearchMyJoinedProject)
			{
				var subquery = APQuery.select(r.Projectid).from(r).where(r.UserId == user.UserId);
				query.where_and(p.ProjectId.In(subquery) & p.ManagerId != user.UserId);
			}

			if (!string.IsNullOrEmpty(statusIds))
			{
				List<Guid> idList = new List<Guid>();
				statusIds.Split(',').ToList().ForEach(x => { idList.Add(Guid.Parse(x)); });

				query.where_and(p.ProjectStatus.In(idList.ToArray()));
			}

			query.primary(p.ProjectId)
			.order_by(p.CreateDate.Desc)
			.skip((current - 1) * rowCount)
			 .take(rowCount);


			//过滤条件
			//模糊搜索用户名、实名进行

			searchPhrase = searchPhrase.Trim();
			if (searchPhrase != "")
			{
				query.where_and(p.ProjectName.Match(searchPhrase) |
				   p.ProjectOwner.Match(searchPhrase) |
				   p.Code.Match(searchPhrase) |
				   p.ProjectName.Match(searchPhrase) |
				   p.ProjectExecutor.Match(searchPhrase) |
				   u.UserName.Match(searchPhrase)
				   );
			}


			//排序条件表达式

			if (sort != null)
			{
				switch (sort.ID)
				{
					case "projectName": query.order_by(sort.OrderBy(p.ProjectName)); break;
					case "projectOwner": query.order_by(sort.OrderBy(p.ProjectOwner)); break;
					case "projectExecutor": query.order_by(sort.OrderBy(p.ProjectExecutor)); break;
					case "pmName": query.order_by(sort.OrderBy(u.UserName)); break;
					case "type": query.order_by(sort.OrderBy(p.ProjectType)); break;
					case "code": query.order_by(sort.OrderBy(p.Code)); break;
					case "progress": query.order_by(sort.OrderBy(p.RateOfProgress)); break;
				}
			}


			//获得查询的总数量

			var total = db.ExecuteSizeOfSelect(query);


			//查询结果集
			var result = query.query(db, rd =>
			{
				var statusVal = p.ProjectStatus.GetValue(rd);
				var type = p.ProjectType.GetValue(rd);

				return new
				{
					id = p.ProjectId.GetValue(rd),
					code = p.RealCode.GetValue(rd),
					projectName = p.ProjectName.GetValue(rd),
					progress = p.RateOfProgress.GetValue(rd) + "%",
					projectOwner = p.ProjectOwner.GetValue(rd),
					projectExecutor = p.ProjectExecutor.GetValue(rd),
					pmName = u.UserName.GetValue(rd, "managerName"),
					projectStatus = ProjectKeys.GetStatusKeyById(statusVal),
					orgId = p.OrgId.GetValue(rd),
					//orgName = o.Name.GetValue(rd, "orgName"),
					type = ProjectKeys.GetTypeKeyById(type)
				};
			});

			return Json(new
			{
				rows = result,
				current,
				rowCount,
				total
			});
		}


		// GET: Project/Edit
		// Post-ajax: Project/Edit

		public ActionResult Edit(Guid id)
		{
			var userId = GetUserInfo().UserId;
			var prj = new Project { PMId = userId, ManagerId = userId, ReviewerId = userId, CreatorId = userId, CreateDate = DateTime.Now }; //default

			return PartialView(prj);
		}

		[HttpPost]
		public ActionResult Edit(Project project)
		{
			var orignal = ProjectrHelper.GetCurrentProject(project.ProjectId, db, true);
			var option = new ProjectEditOption { CurrentUser = GetUserInfo(), db = db, Status = project.ProjectStatus, Orignal = orignal };
			HandleManager.ProjectEditHandlers[ProjectKeys.DefaultProjectType].Handle(project, option);

			if (!option.Result.IsSuccess)
			{
				return Json(new
				{
					result = AjaxResults.Error,
					msg = option.Result.Msg
				});
			}

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Project.EDIT_SUCCESS
			});
		}


		// Post-ajax: Project/Details

		[HttpGet]
		public ActionResult Details(Guid id)
		{
			if (id.IsEmpty())
				return RedirectToAction("Search", "Project");

			var cu = APDBDef.UserInfo;
			var pu = APDBDef.UserInfo.As("Manager");//项目经理
			var he = APDBDef.UserInfo.As("Header");//项目负责人
			var ru = APDBDef.UserInfo.As("Reviewer");
			var re = APDBDef.Resource;
			var rev = APDBDef.Review;

			var project = APQuery.select(p.Asterisk, he.UserName.As("Header"), cu.UserName, pu.UserName.As("Manager"), ru.UserName.As("ReviewerName")
			   )
			   .from(p,
			   cu.JoinLeft(cu.UserId == p.CreatorId),
			   pu.JoinLeft(pu.UserId == p.ManagerId),//项目经理
			   he.JoinLeft(he.UserId == p.PMId),//项目负责人
			   ru.JoinLeft(ru.UserId == p.ReviewerId)
			   )
			   .where(p.ProjectId == id)
			   .query(db, r =>
			   {
				   var proj = new Project();
				   p.Fullup(r, proj, false);

				   proj.Creator = cu.UserName.GetValue(r);
				   // proj.OrgName = o.Name.GetValue(r, "OrgName");
				   proj.Reviewer = ru.UserName.GetValue(r, "ReviewerName");
				   proj.Manager = pu.UserName.GetValue(r, "Manager");
				   proj.Header = he.UserName.GetValue(r, "Header");

				   return proj;
			   }).FirstOrDefault();

			//项目资源
			project.Resources = db.ResourceDal.ConditionQuery(re.Projectid == project.ProjectId, null, null, null);

			return View(project);
		}


		// Post-ajax: Project/EditEestimateMoney

		[HttpPost]
		public ActionResult EditEestimateMoney(Guid id, double emoney)
		{
			APQuery.update(p).set(p.Money.SetValue(emoney))
			   .where(p.ProjectId == id)
			   .execute(db);

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Project.EDIT_SUCCESS
			});
		}

		[HttpPost]
		public ActionResult EditConstractMoney(Guid id, double cmoney)
		{
			APQuery.update(p).set(p.CMoney.SetValue(cmoney))
			   .where(p.ProjectId == id)
			   .execute(db);

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Project.EDIT_SUCCESS
			});
		}


		// GET: Project/ReviewRequest
		// Post-ajax: Project/ReviewRequest

		public ActionResult ReviewRequest(Guid id)
		{
			var project = ProjectrHelper.GetCurrentProject(id);
			var review = new Review
			{
				ProjectId = project.ProjectId,
				ProjectName = project.ProjectName,
				ReviewType = ReviewKeys.ReviewTypeForPjChanged,
				ReceiverID = project.ReviewerId,
				SenderID = GetUserInfo().UserId,
				SendDate = DateTime.Now,
				Title = ReviewKeys.GetTypeKeyByValue(ReviewKeys.ReviewTypeForPjChanged)
			};

			return PartialView(review);
		}

		[HttpPost]
		public ActionResult ReviewRequest(Review review)
		{
			var validateResult = review.Validate();
			if (!validateResult.IsSuccess)
			{
				return Json(new
				{
					result = AjaxResults.Error,
					msg = validateResult.Msg
				});
			}

			var r = APDBDef.Review;

			var hasRequest = db.ReviewDal.ConditionQueryCount(r.ProjectId == review.ProjectId & r.ReviewType == ReviewKeys.ReviewTypeForPjChanged & r.Result == Guid.Empty) > 0;
			if (hasRequest)
			{
				return Json(new
				{
					result = AjaxResults.Error,
					msg = Errors.Review.HAS_IN_REVIEW
				});
			}
			else
			{
				var project = db.ProjectDal.PrimaryGet(review.ProjectId);
				project.SetStatus(ProjectKeys.ReviewStatus);
				db.ProjectDal.Update(project);

				review.ReviewId = Guid.NewGuid();
				db.ReviewDal.Insert(review);
			}

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Review.REQUEST_SEND_SUCCESS
			});
		}


		// Post-ajax:  Project/SubmitReview
		// Post-ajax:  Project/SubmitReviewAction

		[HttpPost]
		public ActionResult SubmitReview(Guid id)
		{
			var rev = APDBDef.Review;
			var project = Project.PrimaryGet(id);
			var userId = GetUserInfo().UserId;
			Review result = null;
			if (userId == project.ReviewerId)
			{
				//当前项目审核请求， receiverId是审核者，只有审核者才能操作
				result = db.ReviewDal.ConditionQuery(rev.ProjectId == id
																	& rev.ReviewType == ReviewKeys.ReviewTypeForPjChanged
																	& (rev.Result == Guid.Empty)
																	& (rev.ReceiverID == GetUserInfo().UserId),
																	rev.ReviewDate.Desc, null, null).FirstOrDefault();
			}
			else
			{
				result = db.ReviewDal.ConditionQuery(rev.ProjectId == id
																 & rev.ReviewType == ReviewKeys.ReviewTypeForPjChanged
																 & (rev.SenderID == GetUserInfo().UserId),
																 rev.SendDate.Desc, null, null).FirstOrDefault();
			}

			return PartialView("_submitReview", result);
		}

		[HttpPost]
		public ActionResult SubmitReviewAction(Review review)
		{
			if (review.Result == ReviewKeys.ResultSuccess)
			{
				return ReviewSuccess(review);
			}
			else if (review.Result == ReviewKeys.ResultFailed)
			{
				return ReviewFail(review);
			}

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Errors.Review.REVIEW_FAILURE
			});
		}


		#region [ private ]

		private ActionResult ReviewSuccess(Review review)
		{
			var pjms = APDBDef.ProjectMileStone;
			var r = APDBDef.Review;
			var t = APDBDef.WorkTask;

			var projectId = review.ProjectId;
			var project = db.ProjectDal.PrimaryGet(projectId);
			var tasks = db.WorkTaskDal.ConditionQuery(t.Projectid == review.ProjectId, null, null, null);
			var nodes = db.ProjectMileStoneDal.ConditionQuery(pjms.Projectid == projectId, null, null, null);
			var stones = MileStone.GetAll();

			db.BeginTrans();

			try
			{
				APQuery.update(r)
				 .set(r.Result.SetValue(ReviewKeys.ResultSuccess), r.ReviewDate.SetValue(DateTime.Now), r.ReviewComment.SetValue(review.ReviewComment))
				 .where(r.ReviewId == review.ReviewId)
				 .execute(db);

				APQuery.update(p)
					   .set(p.ProjectStatus.SetValue(ProjectKeys.ProcessStatus))
					   .where(p.ProjectId == projectId)
					   .execute(db);

				// 依据节点创建项目任务
				//foreach (var node in nodes)
				//{
				//   var root = tasks.Find(x => x.ParentId == Guid.Empty);
				//   db.WorkTaskDal.Insert(new WorkTask
				//   {
				//      TaskId = Guid.NewGuid(),
				//      Projectid = projectId,
				//      CreateDate = DateTime.Now,
				//      CreatorId = project.ManagerId,
				//      StartDate = node.StartDate,
				//      EndDate = node.EndDate,
				//      ManagerId = project.ManagerId,
				//      ReviewerID = project.ReviewerId,
				//      TaskStatus = project.IsPlanStatus ? TaskKeys.PlanStatus : TaskKeys.ProcessStatus,
				//      IsParent = false,
				//      ParentId = root.TaskId,
				//      TaskName = stones.Find(s=>s.StoneId==node.StoneId).StoneName,
				//      TaskType = TaskKeys.ProjectTaskType,
				//      TaskLevel = 2,
				//      SortId = tasks.Count + 1
				//   });
				//}

				db.Commit();
			}
			catch
			{
				db.Rollback();
			}

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Review.OPERATION_SUCCESS
			});
		}

		private ActionResult ReviewFail(Review review)
		{
			var r = APDBDef.Review;

			var project = Project.PrimaryGet(review.ProjectId);

			APQuery.update(r)
				   .set(r.Result.SetValue(ReviewKeys.ResultFailed), r.ReviewDate.SetValue(DateTime.Now), r.ReviewComment.SetValue(review.ReviewComment))
				   .where(r.ReviewId == review.ReviewId)
				   .execute(db);

			APQuery.update(p)
				  .set(p.ProjectStatus.SetValue(ProjectKeys.EditStatus))
				  .where(p.ProjectId == project.ProjectId)
				  .execute(db);

			return Json(new
			{
				result = AjaxResults.Success,
				msg = Success.Review.OPERATION_SUCCESS
			});
		}

		#endregion

	}

}