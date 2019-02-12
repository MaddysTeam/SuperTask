﻿using Business;
using Business.Config;
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
      public ActionResult Search(Guid searchTypeId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         ThrowNotAjax();

         // var o = APDBDef.Organize;
         var u = APDBDef.UserInfo;
         var r = APDBDef.Resource;
         var user = GetUserInfo();

         var query = APQuery.select(p.ProjectId, p.RealCode, p.ProjectType, p.ProjectExecutor, p.ProjectName, p.RateOfProgress,
                                    p.ProjectOwner, p.ProjectStatus, p.OrgId, u.UserName.As("managerName")
                                    )
            .from(p,
                  u.JoinLeft(u.UserId == p.ManagerId)
                  //r.JoinInner((r.Projectid == p.ProjectId & r.UserId == user.UserId))
                  )
            .where(p.ProjectStatus.NotIn(ProjectKeys.DeleteStatus, ProjectKeys.CompleteStatus));

         if(searchTypeId==ProjectKeys.SearchMyProject)
            query.where_and(p.ManagerId == user.UserId);

         if (searchTypeId == ProjectKeys.SearchMyJoinedProject)
         {
            var subquery = APQuery.select(r.Projectid).from(r).where(r.UserId == user.UserId );
            query.where_and(p.ProjectId.In(subquery) & p.ManagerId!=user.UserId);
         }
            //if (owner != ThisApp.SelectAll)
            //   query.where_and(p.ProjectOwner == owner);

            //if (executor != ThisApp.SelectAll)
            //   query.where_and(p.ProjectExecutor == executor);


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
         //var o = APDBDef.Organize;
         var re = APDBDef.Resource;
         var rev = APDBDef.Review;

         var project = APQuery.select(p.Asterisk, he.UserName.As("Header"), cu.UserName, pu.UserName.As("Manager"), ru.UserName.As("ReviewerName")
            //o.Name.As("OrgName")
            )
            .from(p,
            cu.JoinLeft(cu.UserId == p.CreatorId),
            pu.JoinLeft(pu.UserId == p.ManagerId),//项目经理
            he.JoinLeft(he.UserId == p.PMId),//项目负责人
            ru.JoinLeft(ru.UserId == p.ReviewerId)
            // o.JoinLeft(o.ID == p.OrgId)
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

         project.Resources = db.ResourceDal.ConditionQuery(re.Projectid == project.ProjectId, null, null, null);
         project.ProjectProgress = ProjectrHelper.GetProcessByNodeTasks(id, db);
         project.CurrentReview = db.ReviewDal.ConditionQuery(rev.ProjectId == id
                                                             & rev.ReviewType == ReviewKeys.ReviewTypeForPjStart
                                                             & rev.Result == Guid.Empty
                                                             & rev.ReceiverID == GetUserInfo().UserId , null, null, null).FirstOrDefault();

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
         var review = new Review { ProjectId = project.ProjectId, ProjectName= project.ProjectName ,ReviewType=ReviewKeys.ReviewTypeForPjStart};

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

         var hasRequest = db.ReviewDal.ConditionQueryCount(r.ProjectId == review.ProjectId & r.ReviewType == ReviewKeys.ReviewTypeForPjStart & r.Result != Guid.Empty) > 0;
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

            db.ReviewDal.Insert(review);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Review.REQUEST_SEND_SUCCESS
         });
      }

      [HttpPost]
      public ActionResult SubmitReview(Guid id)
      {
         var project = db.ProjectDal.PrimaryGet(id);
         project.SetStatus(ProjectKeys.ProcessStatus);
         db.ProjectDal.Update(project);

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.Review.REQUEST_SUBMIT
         });
      }

      [HttpPost]
      public ActionResult ReviewFail(Guid id)
      {
         var project = db.ProjectDal.PrimaryGet(id);
         project.SetStatus(ProjectKeys.PlanStatus);
         db.ProjectDal.Update(project);

         return Json(new
         {
            result = AjaxResults.Error,
            msg = Errors.Review.REVIEW_FAILURE
         });
      }


      // GET: Project/ReviewRequest
      // GET: Project/AfterProjectStartReviewSend
      // GET: Project/AfterProjectStartSubimitReview
      // GET: Project/AfterReviewFail

      //public ActionResult ReviewRequest(Guid id, Guid reviewType)
      //{
      //   if (id.IsEmpty())
      //      throw new ArgumentException(Errors.Task.NOT_ALLOWED_ID_NULL);

      //   var project = db.ProjectDal.PrimaryGet(id);

      //   var requestOption = new ReviewRequestOption
      //   {
      //      Project = project,
      //      User = GetUserInfo(),
      //      ReviewType = reviewType,
      //      db = db,
      //      Result = Result.Initial()
      //   };

      //   HandleManager.ProjectReviewHandlers[reviewType].Handle(project, requestOption);

      //   if (!requestOption.Result.IsSuccess)
      //      throw new ApplicationException(requestOption.Result.Msg);


      //   return RedirectToAction("FlowIndex", "WorkFlowRun", requestOption.RunParams);
      //}

      //public ActionResult AfterProjectStartReviewSend(Guid instanceId)
      //   => AfterReviewSendOrSubmit(instanceId, ProjectKeys.ReviewStatus);

      //public ActionResult AfterProjectStartSubimitReview(Guid instanceId)
      //{
      //   if (instanceId.IsEmpty())
      //      throw new NullReferenceException("instance id 不能为空！");

      //   var review = db.ReviewDal.PrimaryGet(instanceId);
      //   var project = db.ProjectDal.PrimaryGet(review.ProjectId);

      //   project.SetStatus(ProjectKeys.ProcessStatus);

      //   Edit(project);

      //   return RedirectToAction("Details", "Project", new { id = project.ProjectId });

      //}

      //public ActionResult AfterReviewFail(Guid instanceId)
      //   => AfterReviewSendOrSubmit(instanceId, ProjectKeys.ProcessStatus);


      #region [ private ]

      private ActionResult AfterReviewSendOrSubmit(Guid instanceId, Guid status)
      {
         if (instanceId.IsEmpty())
            throw new NullReferenceException("instance id 不能为空！");

         var review = db.ReviewDal.PrimaryGet(instanceId);
         var project = db.ProjectDal.PrimaryGet(review.ProjectId);

         project.SetStatus(status);

         db.ProjectDal.Update(project);

         return RedirectToAction("Details", "Project", new { id = project.ProjectId });
      }

      #endregion

   }

}