using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Linq;
using System.Web.Mvc;
using TheSite.Models;

namespace TheSite.Controllers
{

   public class ProjectStoneTaskController : BaseController
   {

      // POST-Ajax: ProjectStoneTask/List

      [HttpPost]
      public ActionResult List(Guid projectId, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var pst = APDBDef.ProjectStoneTask;

         var query = APQuery.select(pst.Asterisk)
                        .from(pst)
                        .where(pst.ProjectId == projectId);

         if (!string.IsNullOrEmpty(searchPhrase))
         {
            query.where_and(pst.TaskName.Match(searchPhrase));
         }

         query.primary(pst.PstId)
          .order_by(pst.CreateDate.Desc)
          .skip((current - 1) * rowCount)
          .take(rowCount);


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);

         var rows = query.query(db, r =>
         {
            var realStart = pst.RealStartDate.GetValue(r);
            var realEnd = pst.RealEndDate.GetValue(r);
            return new
            {
               id = pst.PstId.GetValue(r),
               name = pst.TaskName.GetValue(r),
               start = pst.StartDate.GetValue(r),
               end = pst.EndDate.GetValue(r),
               realStart = realStart.ConvertToString(),
               realEnd = realEnd.ConvertToString(),
               status = TaskKeys.GetStatusKeyByValue(pst.TaskStatus.GetValue(r)),
               statusId=pst.TaskStatus.GetValue(r)
            };
         }).ToList();

         return Json(new
         {
            rows,
            current,
            rowCount,
            total
         });
      }


      // GET: ProjectStoneTask/Edit
      // POST-Ajax: ProjectStoneTask/Edit

      public ActionResult Edit(Guid id)
      {
         var stoneTask = db.ProjectStoneTaskDal.PrimaryGet(id);

         return PartialView(stoneTask);
      }

      [HttpPost]
      public ActionResult Edit(ProjectStoneTask task)
      {
         if (task.ProjectId.IsEmpty())
         {
            return Json(new
            {
               result = AjaxResults.Error,
               msg = Errors.StoneTask.EDIT_FAIL
            });
         }
         if (task.PstId.IsEmpty())
         {
            db.ProjectStoneTaskDal.Insert(task);
         }
         else
         {
            if (task.IsTempEditStatus)
            {
               task.SetStatus(TaskKeys.ProcessStatus);
            }
            db.ProjectStoneTaskDal.Update(task);
         }

         return Json(new
         {
            result = AjaxResults.Success,
            msg = Success.StoneTask.EDITSUCCESS
         });
      }


      // GET: ProjectStoneTask/ReviewRequest
      // GET: ProjectStoneTask/AfterProjectStartReviewSend
      // GET: ProjectStoneTask/AfterProjectStartSubimitReview
      // GET: ProjectStoneTask/AfterReviewFail

      public ActionResult ReviewRequest(Guid id, Guid reviewType)
      {
         if (id.IsEmpty())
            throw new ArgumentException(Errors.Task.NOT_ALLOWED_ID_NULL);

         var pst = db.ProjectStoneTaskDal.PrimaryGet(id);
         if (pst == null)
            throw new ArgumentException(Errors.Task.NOT_EXIST);

         var project = db.ProjectDal.PrimaryGet(pst.ProjectId);

         var requestOption = new ReviewRequestOption
         {
            Project = project,
            User = GetUserInfo(),
            ReviewType = reviewType,
            db = db,
            Result = Result.Initial()
         };


         HandleManager.ProjectStoneTaskReviewHandlers[reviewType].Handle(pst, requestOption);

         if (!requestOption.Result.IsSuccess)
            throw new ApplicationException(requestOption.Result.Msg);


         return RedirectToAction("FlowIndex", "WorkFlowRun", requestOption.RunParams);
      }

      public ActionResult AfterEditReview(Guid instanceId)
      {
         return AfterReviewSendOrSubmit(instanceId, TaskKeys.TaskTempEditStatus);
      }

      public ActionResult AfterSubmitReview(Guid instanceId)
      {
         if (instanceId.IsEmpty())
            throw new NullReferenceException("instance id 不能为空！");

         //找其任务提交审核的第一条记录，将任务结束时间设置成第一次提交任务的时间
         var review = ReviewHelper.GetEarlistReview(db, instanceId);
         var pst = db.ProjectStoneTaskDal.PrimaryGet(review.TaskId);

         pst.Complete(review.SendDate < pst.StartDate ? pst.EndDate : review.SendDate);

         db.ProjectStoneTaskDal.Update(pst);

         //重定向到项目明细
         return RedirectToAction("Details", "Project", new { id = pst.ProjectId });
      }

      public ActionResult AfterEditReviewSend(Guid instanceId)
      {
         return AfterReviewSendOrSubmit(instanceId, TaskKeys.ReviewStatus);
      }

      public ActionResult AfterSubmitReviewSend(Guid instanceId)
      {
         return AfterReviewSendOrSubmit(instanceId, TaskKeys.ReviewStatus);
      }

      public ActionResult AfterReviewFail(Guid instanceId)
      {
         return AfterReviewSendOrSubmit(instanceId, TaskKeys.ProcessStatus);

      }

      private ActionResult AfterReviewSendOrSubmit(Guid instanceId,Guid status)
      {
         if (instanceId.IsEmpty())
            throw new NullReferenceException("instance id 不能为空！");

         var review = db.ReviewDal.PrimaryGet(instanceId);
         var pst = db.ProjectStoneTaskDal.PrimaryGet(review.TaskId);

         pst.SetStatus(status);

         db.ProjectStoneTaskDal.Update(pst);

         return RedirectToAction("Details", "Project", new { id = pst.ProjectId });
      }

      private ActionResult NodeTaskDeny()
      {
         return null;
      }

   }

}