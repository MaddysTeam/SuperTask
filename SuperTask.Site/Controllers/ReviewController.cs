using Business;
using Business.Helper;
using System;
using System.Web.Mvc;
using System.Linq;
using Business.Config;
using TheSite.Models;

namespace TheSite.Controllers
{
   public class ReviewController : BaseController
   {

      static APDBDef.ReviewTableDef r = APDBDef.Review;

      public ActionResult WorkTaskReviewRequest(Guid id, Guid reviewType)
      {
         if (id.IsEmpty())
            throw new ArgumentException(Errors.Task.NOT_ALLOWED_ID_NULL);

         var task = db.WorkTaskDal.PrimaryGet(id);
         if (task == null)
            throw new ArgumentException(Errors.Task.NOT_EXIST);

         var project = db.ProjectDal.PrimaryGet(task.Projectid);

         var requestOption = new ReviewRequestOption
         {
            Project = project,
            User = GetUserInfo(),
            ReviewType = reviewType,
            db = db,
            Result = Result.Initial()
         };

         HandleManager.TaskReviewRequestHandlers[reviewType].Handle(task, requestOption);

         if (!requestOption.Result.IsSuccess)
            throw new ApplicationException(requestOption.Result.Msg);


         return PartialView(requestOption.Review);
      }

      [HttpPost]
      public ActionResult WorkTaskReviewRequest(Review review)
      {
         db.BeginTrans();

         try
         {
            if (review.ReviewId.IsEmpty())
            {
               review.ReviewId = Guid.NewGuid();
               db.ReviewDal.Insert(review);

               if (review.ReviewType == ReviewKeys.ReviewTypeForTkChanged)
               {
                  AfterTaskEditReviewSend(review);
               }

               if (review.ReviewType == ReviewKeys.ReviewTypeForTkSubmit)
               {
                  AfterTaskSubmitReviewSend(review);
               }

               db.WorkflowDal.Insert(Mapping(review));
            }

            db.Commit();
         }
         catch
         {
            db.Rollback();

            return Json(new
            {
               result = AjaxResults.Success,
               msg = "操作失败，请联系管理员"
            });
         }

         if (Request.IsAjaxRequest())
         {
            return Json(new
            {
               result = AjaxResults.Success,
               msg = "审核操作成功"
            });
         }
         else
            return RedirectToAction("Index", "Home");
      }


      public ActionResult WorkTaskReviewAction(Guid id)
      {
         var task = db.WorkTaskDal.PrimaryGet(id);
         if (task == null)
            throw new ArgumentException(Errors.Task.NOT_EXIST);

         var project = db.ProjectDal.PrimaryGet(task.Projectid);
         if (project == null)
            throw new ArgumentException(Errors.Project.NOT_EXIST);

         var review = db.ReviewDal.ConditionQuery(r.TaskId == id & r.Result == ReviewKeys.ResultWait, null, null, null).FirstOrDefault();
         review.TaskName = task.TaskName;
         review.ProjectName = project.ProjectName;
         review.DateRange = string.Format("{0}  至  {1}", task.StartDate, task.EndDate);

         return PartialView(review);
      }

      [HttpPost]
      public ActionResult WorkTaskReviewAction(Review review)
      {
         db.BeginTrans();

         try
         {
            db.ReviewDal.UpdatePartial(review.ReviewId, new { Result = review.Result,Comment=review.Comment, ReviewDate = DateTime.Now });

            if (review.ReviewType == ReviewKeys.ReviewTypeForTkChanged && review.Result == ReviewKeys.ResultSuccess)
            {
               AfterTaskEditReview(review);
            }

            else if (review.ReviewType == ReviewKeys.ReviewTypeForTkSubmit && review.Result == ReviewKeys.ResultSuccess)
            {
               AfterTaskSubmitReview(review);
            }

            else if (review.Result == ReviewKeys.ResultFailed)
            {
               AfterTaskReviewFail(review);
            }

            var workflow = Mapping(review);
            workflow.ReceiveTime = DateTime.Now;

            db.WorkflowDal.Insert(workflow);

            db.Commit();
         }
         catch
         {
            db.Rollback();

            return Json(new
            {
               result = AjaxResults.Success,
               msg = "操作失败，请联系管理员"
            });
         }

         if (Request.IsAjaxRequest())
         {
            return Json(new
            {
               result = AjaxResults.Success,
               msg = "审核操作成功"
            });
         }
         else
            return RedirectToAction("Index", "Home");

      }



      public ActionResult StoneTaskReviewRequest(Guid id, Guid reviewType)
      {
         if (id.IsEmpty())
            throw new ArgumentException(Errors.Task.NOT_ALLOWED_ID_NULL);

         var task = db.ProjectStoneTaskDal.PrimaryGet(id);
         if (task == null)
            throw new ArgumentException(Errors.Task.NOT_EXIST);

         var project = db.ProjectDal.PrimaryGet(task.ProjectId);

         var requestOption = new ReviewRequestOption
         {
            Project = project,
            User = GetUserInfo(),
            ReviewType = reviewType,
            db = db,
            Result = Result.Initial()
         };


         HandleManager.ProjectStoneTaskReviewHandlers[reviewType].Handle(task, requestOption);

         if (!requestOption.Result.IsSuccess)
            throw new ApplicationException(requestOption.Result.Msg);


         return PartialView(requestOption.Review);
      }


      [HttpPost]
      public ActionResult StoneTaskReviewRequest(Review review)
      {

         var isInReview = db.ReviewDal.ConditionQueryCount(r.TaskId == review.TaskId & r.Result == ReviewKeys.ResultWait) > 0;
         if (isInReview)
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = "不要重复提交"
            });
         }

         db.BeginTrans();

         try
         {

            if (review.ReviewId.IsEmpty())
            {
               review.ReviewId = Guid.NewGuid();
               db.ReviewDal.Insert(review);

               if (review.ReviewType == ReviewKeys.ReviewTypeForStoneTaskChanged)
               {
                  AfterEditReviewSend(review);
               }

               if (review.ReviewType == ReviewKeys.ReviewTypeForStoneTaskSubmit)
               {
                  AfterSubmitReviewSend(review);
               }

               db.WorkflowDal.Insert(Mapping(review));
            }

            db.Commit();
         }
         catch
         {
            db.Rollback();

            return Json(new
            {
               result = AjaxResults.Success,
               msg = "操作失败，请联系管理员"
            });
         }

         if (Request.IsAjaxRequest())
         {
            return Json(new
            {
               result = AjaxResults.Success,
               msg = "操作成功"
            });
         }
         else
            return RedirectToAction("Index", "Home");
      }


      public ActionResult StoneTaskReviewAction(Guid id)
      {
         var task = db.ProjectStoneTaskDal.PrimaryGet(id);
         if (task == null)
            throw new ArgumentException(Errors.Task.NOT_EXIST);

         var project = db.ProjectDal.PrimaryGet(task.ProjectId);
         if (project == null)
            throw new ArgumentException(Errors.Project.NOT_EXIST);

         var review = db.ReviewDal.ConditionQuery(r.TaskId == id & r.Result == ReviewKeys.ResultWait, null, null, null).FirstOrDefault();
         review.TaskName = task.TaskName;
         review.ProjectName = project.ProjectName;
         review.DateRange = string.Format("{0}  至  {1}", task.StartDate, task.EndDate);

         return PartialView(review);
      }

      [HttpPost]
      public ActionResult StoneTaskReviewAction(Review review)
      {
         db.BeginTrans();

         try
         {
            db.ReviewDal.UpdatePartial(review.ReviewId, new { Result = review.Result, Comment = review.Comment, ReviewDate=DateTime.Now});

            if (review.ReviewType == ReviewKeys.ReviewTypeForStoneTaskChanged && review.Result == ReviewKeys.ResultSuccess)
            {
               AfterEditReview(review);
            }

            else if (review.ReviewType == ReviewKeys.ReviewTypeForStoneTaskSubmit && review.Result == ReviewKeys.ResultSuccess)
            {
               AfterSubmitReview(review);
            }

            else if (review.Result == ReviewKeys.ResultFailed)
            {
               AfterReviewFail(review);
            }

            var workflow = Mapping(review);
            workflow.ReceiveTime = DateTime.Now;

            db.WorkflowDal.Insert(workflow);


            db.Commit();
         }
         catch
         {
            db.Rollback();

            return Json(new
            {
               result = AjaxResults.Success,
               msg = "操作失败，请联系管理员"
            });
         }

         if (Request.IsAjaxRequest())
         {
            return Json(new
            {
               result = AjaxResults.Success,
               msg = "审核操作成功"
            });
         }
         else
            return RedirectToAction("Index", "Home");

      }


      #region StoneTask Review

      private void AfterSubmitReview(Review review)
      {
         //找其任务提交审核的第一条记录，将任务结束时间设置成第一次提交任务的时间

         var pst = db.ProjectStoneTaskDal.PrimaryGet(review.TaskId);

         pst.Complete(review.SendDate < pst.StartDate ? pst.EndDate : review.SendDate);

         db.ProjectStoneTaskDal.Update(pst);

         db.ProjectDal.UpdatePartial(pst.ProjectId, new { RateOfProgress = ProjectrHelper.GetProcessByNodeTasks(pst.ProjectId, db) });

         // return RedirectToAction("Search", "WorkFlowTask");
      }

      private void AfterEditReview(Review review)
      {
         AfterReviewSendOrSubmit(review, TaskKeys.TaskTempEditStatus);
      }

      private void AfterEditReviewSend(Review review)
      {
         AfterReviewSendOrSubmit(review, TaskKeys.ReviewStatus);
      }

      private void AfterSubmitReviewSend(Review review)
      {
         AfterReviewSendOrSubmit(review, TaskKeys.ReviewStatus);
      }

      private void AfterReviewFail(Review review)
      {
         AfterReviewSendOrSubmit(review, TaskKeys.ProcessStatus);
      }

      private void AfterReviewSendOrSubmit(Review review, Guid status)
      {
         var pst = db.ProjectStoneTaskDal.PrimaryGet(review.TaskId);

         pst.SetStatus(status);

         db.ProjectStoneTaskDal.Update(pst);
      }

      #endregion

      #region WorkTask Review

      private void AfterTaskSubmitReview(Review review)
      {
         //找其任务提交审核的第一条记录，将任务结束时间设置成第一次提交任务的时间

         var pst = db.WorkTaskDal.PrimaryGet(review.TaskId);

         pst.Complete(review.SendDate < pst.StartDate ? pst.EndDate : review.SendDate);

         db.WorkTaskDal.Update(pst);
      }

      private void AfterTaskEditReview(Review review)
      {
         AfterTaskReviewSendOrSubmit(review, TaskKeys.TaskTempEditStatus);
      }

      private void AfterTaskEditReviewSend(Review review)
      {
         AfterTaskReviewSendOrSubmit(review, TaskKeys.ReviewStatus);
      }

      private void AfterTaskSubmitReviewSend(Review review)
      {
         AfterTaskReviewSendOrSubmit(review, TaskKeys.ReviewStatus);
      }

      private void AfterTaskReviewFail(Review review)
      {
         AfterTaskReviewSendOrSubmit(review, TaskKeys.ProcessStatus);
      }

      private void AfterTaskReviewSendOrSubmit(Review review, Guid status)
      {
         var pst = db.WorkTaskDal.PrimaryGet(review.TaskId);

         pst.SetStatus(status);

         db.WorkTaskDal.Update(pst);
      }

      #endregion

      private Workflow Mapping(Review review)
      {
         return new Workflow
         {
            FlowId = Guid.NewGuid(),
            Comment = review.Comment,
            CompletedTime = DateTime.Now,
            ReceiverId = review.ReceiverID,
            ReviewId=review.ReviewId,
            SenderId=review.SenderID,
            SenderTime=review.SendDate,
            ReceiveTime=DateTime.Parse("1900-01-01"),
            Title=review.Title,
            Status=review.Result
         };
      }

   }

}