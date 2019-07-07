using Business;
using Business.Helper;
using Symber.Web.Data;
using System;
using System.Linq;
using System.Web.Mvc;

namespace TheSite.Controllers
{

   public class WorkFlowTaskController : BaseController
   {

      static APDBDef.WorkflowTableDef wf = APDBDef.Workflow;
      static APDBDef.ReviewTableDef r = APDBDef.Review;


      // GET: WorkFlowTask/Search
      // POST-Ajax: WorkFlowTask/Search

      public ActionResult Search(Guid? taskId)
      {
         return View();
      }


      [HttpPost]
      public ActionResult Search(Guid taskid, string searchType, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var user = GetUserInfo();

         var p = APDBDef.Project;
         var t = APDBDef.WorkTask;
         var pst = APDBDef.ProjectStoneTask;
         var u = APDBDef.UserInfo;
         var u2 = APDBDef.UserInfo.As("Recevier");

         var query = APQuery.select(wf.FlowId, u.UserName, u2.UserName.As("RecevierName"), pst.TaskName.As("StoneTaskName"), pst.TaskType.As("StoneTaskType")
                                    , p.ProjectName, t.TaskName.As("TaskName"), t.StartDate, t.EndDate, t.EstimateWorkHours, t.WorkHours,t.TaskType.As("TaskType"),
                                    r.SenderID, r.ReceiverID, r.TaskId, r.ProjectId, r.Result, r.Title,
                                    r.ReviewType, r.SendDate, r.ReviewDate, r.Comment, r.AttachmentUrl)
                            .from(wf,
                            r.JoinInner(r.ReviewId == wf.ReviewId),
                            u.JoinInner(r.SenderID == u.UserId),
                            u2.JoinInner(r.ReceiverID == u2.UserId),
                            p.JoinLeft(r.ProjectId == p.ProjectId),
                            t.JoinLeft(r.TaskId == t.TaskId),
                            pst.JoinLeft(r.TaskId == pst.PstId)
                            )
                            .primary(wf.FlowId)
                            .where(wf.ReceiverId == user.UserId | wf.SenderId == user.UserId)
                            .order_by(r.SendDate.Desc);

         if (!taskid.IsEmpty())
            query.where_and(r.TaskId == taskid);

         if (searchType == ReviewKeys.ResultWait.ToString())
            query.where_and(r.Result == ReviewKeys.ResultWait);//待审核流程
         else if (searchType == ReviewKeys.ResultSuccess.ToString())
            query.where_and(r.Result == ReviewKeys.ResultSuccess & wf.ReceiveTime> DateTime.Parse("1900-01-01"));//只取审核记录
         else if (searchType == ReviewKeys.ResultFailed.ToString())
            query.where_and(r.Result == ReviewKeys.ResultFailed & wf.ReceiveTime > DateTime.Parse("1900-01-01"));//只取审核记录


         if (searchPhrase != "")
         {
            query.where_and(t.TaskName.Match(searchPhrase) |
                            p.ProjectName.Match(searchPhrase) |
                            r.Title.Match(searchPhrase) |
                            u.UserName.Match(searchPhrase) |
                            u2.UserName.Match(searchPhrase));
         }


         //排序条件表达式

         if (sort != null)
         {
            switch (sort.ID)
            {
               case "projectName": query.order_by(sort.OrderBy(r.ProjectId)); break;
               case "taskName": query.order_by(sort.OrderBy(r.TaskId)); break;
               case "title": query.order_by(sort.OrderBy(r.Title)); break;
               case "sender": query.order_by(sort.OrderBy(r.SenderID)); break;
               case "receiver": query.order_by(sort.OrderBy(r.ReceiverID)); break;
               case "sendDate": query.order_by(sort.OrderBy(r.SendDate)); break;
               case "reviewDate": query.order_by(sort.OrderBy(r.ReviewDate)); break;
            }
         }


         //分页

         query.skip((current - 1) * rowCount)
              .take(rowCount);


         //获得查询的总数量

         var total = db.ExecuteSizeOfSelect(query);


         //查询结果集

         var result = query.query(db, rd =>
         {
            var id = wf.FlowId.GetValue(rd);
            var attachmentUrl = r.AttachmentUrl.GetValue(rd);
            var taskName = t.TaskName.GetValue(rd, "TaskName");
            if (string.IsNullOrEmpty(taskName))
            {
               taskName = pst.TaskName.GetValue(rd, "StoneTaskName");
            }
            var taskType = t.TaskType.GetValue(rd, "TaskType");
            if (taskType.IsEmpty())
            {
               taskType = pst.TaskType.GetValue(rd, "StoneTaskType");
            }

            var review = new Review
            {
               ReviewId = id,
               Comment = r.Comment.GetValue(rd),
               ProjectId = r.ProjectId.GetValue(rd),
               SenderID = r.SenderID.GetValue(rd),
               ReceiverID = r.ReceiverID.GetValue(rd),
               SendDate = r.SendDate.GetValue(rd),
               ReviewDate = r.ReviewDate.GetValue(rd),
               AttachmentUrl = string.IsNullOrEmpty(attachmentUrl) ? string.Empty : attachmentUrl,
               TaskId = r.TaskId.GetValue(rd),
               ReviewType = r.ReviewType.GetValue(rd),
               ProjectName = p.ProjectName.GetValue(rd),
               TaskName = taskName,
               Result = r.Result.GetValue(rd),
               Title = r.Title.GetValue(rd),
               DateRange = string.Format("{0}  至  {1}", t.StartDate.GetValue(rd), t.EndDate.GetValue(rd)),
               TaskEsHours = t.EstimateWorkHours.GetValue(rd),
               TaskHours = t.WorkHours.GetValue(rd)
            };

            return new
            {
               id = id,
               taskId = review.TaskId,
               taskTypeId = taskType,
               taskTypeName = TaskKeys.GetTypeKeyByValue(taskType),
               projectName = review.ProjectName,
               taskName = review.TaskName,
               title = review.Title,
               sendDate = review.SendDate,
               sender = u.UserName.GetValue(rd),
               receiver = u2.UserName.GetValue(rd, "RecevierName"),
               receiverId = review.ReceiverID,
               reviewTypeId = review.ReviewType,
               //query = queryStr,
               attachmentUrl = review.AttachmentUrl,
               result = review.Result,
               reviewDate = review.ReviewDate == DateTime.MinValue ? "-" : review.ReviewDate.ToString("yyyy-MM-dd"),
               comment = review.Comment,
               //objJSON = json,
            };
         }).ToList();

         return Json(new
         {
            rows = result,
            current,
            rowCount,
            total
         });
      }

   }

}
