using Business;
using Business.Helper;
using Business.Roadflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheSite.Models;

namespace Business
{

   public class TaskReviewRequestHandler : IHandler<WorkTask, ReviewRequestOption>
   {

      public virtual void Handle(WorkTask ta, ReviewRequestOption v)
      {
         var user = v.User;
         var title = ReviewKeys.GetTypeKeyByValue(v.ReviewType);

         var review = new Review
         {
            SenderID = user.UserId,
            Sender = user.UserName,
            ReceiverID = ta.ReviewerID,
            SendDate = DateTime.Now,
            ReviewDate = DateTime.Parse("1/1/1753").AddYears(100),
            ReviewType = v.ReviewType,
            Result = ReviewKeys.ResultWait,
            ProjectId = ta.IsProjectTaskType ? v.Project.ProjectId : Guid.Empty,
            TaskId = ta.TaskId,
            Title = title,
            TaskName = ta.TaskName,
            ProjectName = ta.IsProjectTaskType ? v.Project.ProjectName : string.Empty,
            DateRange = string.Format("{0}  至  {1}", ta.StartDate, ta.EndDate),
            TaskEsHours = ta.EstimateWorkHours,
            TaskHours = ta.WorkHours
         };

         var json = Newtonsoft.Json.JsonConvert.SerializeObject(review, new Newtonsoft.Json.JsonSerializerSettings() { StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.EscapeNonAscii });
         var flowParams = new RunParams
         {
            UserId = user.UserId.ToString(),
            DetaultMember = ta.ReviewerID.ToString(),
            FlowId = v.FlowId.ToString(),
            Display = "1",
            ObjJson = json,
            Title = title,
         };


         v.RunParams = flowParams;
         v.Result.IsSuccess = true;
      }

   }


   public class TaskSubmitRequestHandler : TaskReviewRequestHandler
   {

      public override void Handle(WorkTask tk, ReviewRequestOption v)
      {
         var re = v.Result;
         var db = v.db;
         var pj = v.Project;
         var t = APDBDef.WorkTask;

         if (tk.TaskType == TaskKeys.TempTaskType)
         {
            base.Handle(tk, v);
            return;
         }

         if (tk.IsProjectTaskType && !pj.IsProcessStatus)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Project.NOT_IN_PROCESS;
            return;
         }

         if (tk.IsProjectTaskType && pj.IsCompleteStatus)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Project.HAS_COMPLETE;
            return;
         }

         //查找所有在执行状态的子任务，如果有子任务没有完成，则退出提醒
         var childs = db.WorkTaskDal.ConditionQuery(t.ParentId == tk.TaskId & t.TaskStatus == TaskKeys.ProcessStatus, null, null, null);
         var allComplete = childs.Count <= 0 || childs.All(k => k.IsCompleteStatus);
         if (!allComplete)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.CHILD_NOT_DONE;
            return;
         }


         v.Result = re;

         base.Handle(tk, v);
      }

   }


   public class TaskEditRequestHandler : TaskReviewRequestHandler
   {
      public override void Handle(WorkTask t, ReviewRequestOption v)
      {
         var re = v.Result;

         if (t.IsParent && !t.IsPlanTask)
         {
            re.IsSuccess = false;
            re.Msg = Errors.Task.PARENT_NOT_ALLOWED_CHANGE_DATE;
            return;
         }

         base.Handle(t, v);
      }
   }


   public class ReviewRequestOption
   {
      public Project Project { get; set; }

      public UserInfo User { get; set; }

      public Guid FlowId => ReviewKeys.FlowId;

      public Guid ReviewType { get; set; }

      public RunParams RunParams { get; set; }

      public Result Result { get; set; }

      public APDBDef db { get; set; }
   }


}
