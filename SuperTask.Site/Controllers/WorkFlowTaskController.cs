using Business;
using Business.Helper;
using Business.Roadflow;
using Symber.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TheSite.Controllers
{

   public class WorkFlowTaskController : BaseController
   {

     static APDBDef.WorkflowTaskTableDef wft = APDBDef.WorkflowTask;
     static APDBDef.ReviewTableDef r = APDBDef.Review;


      // GET: WorkFlowTask/Search
      // POST-Ajax: WorkFlowTask/Search

      public ActionResult Search(Guid? taskId)
      {
         return View();
      }


      [HttpPost]
      public ActionResult Search(Guid taskid,string searchType, int current, int rowCount, AjaxOrder sort, string searchPhrase)
      {
         var user = GetUserInfo();

         var p = APDBDef.Project;
         var t = APDBDef.WorkTask;

         var query = APQuery.select(wft.TaskId, wft.FlowId, wft.StepID, wft.GroupID, wft.InstanceID, wft.StepName, wft.Status,
                                    wft.SenderName, wft.ReceiveName, p.ProjectName, t.TaskName.As("TaskName"),t.StartDate,t.EndDate,t.EstimateWorkHours,t.WorkHours,
                                    r.SenderID, r.ReceiverID, r.TaskId, r.ProjectId, r.Result, r.Title,
                                    r.ReviewType, r.SendDate, r.ReviewDate, r.Comment, r.AttachmentUrl)
                            .from(wft,
                            r.JoinInner(r.ReviewId == wft.InstanceID),
                            p.JoinLeft(r.ProjectId == p.ProjectId),
                            t.JoinLeft(r.TaskId == t.TaskId)
                            )
                            .primary(wft.TaskId)
                            .where(wft.ReceiveID == user.UserId | wft.SenderID==user.UserId)
                            .where_and(wft.Sort == 2)
                            .order_by(r.SendDate.Desc);

         if (!taskid.IsEmpty())
            query.where_and(r.TaskId==taskid);

         if (searchType == ReviewKeys.ResultWait.ToString())
            query.where_and(r.Result == ReviewKeys.ResultWait);//待审核流程
         else if (searchType == ReviewKeys.ResultSuccess.ToString())
            query.where_and(r.Result == ReviewKeys.ResultSuccess);//只取审核记录
         else if (searchType == ReviewKeys.ResultFailed.ToString())
            query.where_and(r.Result == ReviewKeys.ResultFailed);//只取审核记录


         if (searchPhrase != "")
         {
            query.where_and(t.TaskName.Match(searchPhrase) |
                            p.ProjectName.Match(searchPhrase) |
                            r.Title.Match(searchPhrase) |
                            wft.SenderName.Match(searchPhrase)|
                            wft.ReceiveName.Match(searchPhrase));
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
               case "reviewDate": query.order_by(sort.OrderBy(r.ReviewDate));break;
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
            var id = wft.TaskId.GetValue(rd);
            var attachmentUrl = r.AttachmentUrl.GetValue(rd);

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
               TaskName = t.TaskName.GetValue(rd, "TaskName"),
               Result = r.Result.GetValue(rd),
               Title = r.Title.GetValue(rd),
               DateRange=string.Format("{0}  至  {1}",t.StartDate.GetValue(rd),t.EndDate.GetValue(rd)),
               TaskEsHours=t.EstimateWorkHours.GetValue(rd),
               TaskHours=t.WorkHours.GetValue(rd)
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(review);

            var queryStr = string.Format("stepid={0}&flowid={1}&instanceid={2}&taskid={3}&groupid={4}&appid={5}&userId={6}",
                wft.StepID.GetValue(rd).ToString(),
                wft.FlowId.GetValue(rd).ToString(),
                wft.InstanceID.GetValue(rd).ToString(),
                wft.TaskId.GetValue(rd).ToString(),
                wft.GroupID.GetValue(rd).ToString(),
                "",
                user.UserId
                );

            return new
            {
               id = id,
               projectName = review.ProjectName,
               taskName = review.TaskName,
               title = review.Title,
               sendDate=review.SendDate,
               sender = wft.SenderName.GetValue(rd),
               receiver = wft.ReceiveName.GetValue(rd),
               receiverId=review.ReceiverID,
               query = queryStr,
               attachmentUrl = review.AttachmentUrl,
               result= review.Result,
               reviewDate =review.ReviewDate==DateTime.MinValue?"-":review.ReviewDate.ToString("yyyy-MM-dd"),
               comment=review.Comment,
               objJSON = json,
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

   }

}
