using Business.Helper;
using System;
using System.Linq;
using TheSite.Models;

namespace Business
{

   public class TaskReviewRequestHandler : IHandler<WorkTask, ReviewRequestOption>
   {
      static APDBDef.ReviewTableDef rv = APDBDef.Review;

      public virtual void Handle(WorkTask ta, ReviewRequestOption v)
      {
         var user = v.User;
         var title = ReviewKeys.GetTypeKeyByValue(v.ReviewType);

         var isInReview = v.db.ReviewDal.ConditionQueryCount(rv.TaskId == ta.TaskId & rv.ReviewType == v.ReviewType & rv.Result == ReviewKeys.ResultWait) > 0;
         if (isInReview)
         {
            v.Result.Msg = Errors.Review.HAS_IN_REVIEW;
            v.Result.IsSuccess = false;
            return;
         }

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

         v.Review = review;
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

      public Result Result { get; set; }

      public APDBDef db { get; set; }

      public Review Review { get; set; }
   }


   public class ProjectReviewHandler : IHandler<Project, ReviewRequestOption>
   {
      static APDBDef.ReviewTableDef rv = APDBDef.Review;

      public void Handle(Project p, ReviewRequestOption v)
      {
         var isInReview = v.db.ReviewDal.ConditionQueryCount(rv.ProjectId == p.ProjectId & rv.ReviewType == v.ReviewType & rv.Result == ReviewKeys.ResultWait) > 0;
         if (isInReview)
         {
            v.Result.Msg = Errors.Review.HAS_IN_REVIEW;
            v.Result.IsSuccess = false;
            return;
         }

         var user = v.User;
         var reviewerId = p.ReviewerId;
         var title = ReviewKeys.GetTypeKeyByValue(v.ReviewType);
         var review = new Review
         {
            SenderID = user.UserId,
            Sender = user.UserName,
            ReceiverID = reviewerId,
            SendDate = DateTime.Now,
            ReviewDate = DateTime.Parse("1/1/1753").AddYears(100),
            ReviewType = v.ReviewType,
            Result = ReviewKeys.ResultWait,
            ProjectId = p.ProjectId,
            Title = title,
            ProjectName = p.ProjectName,
            DateRange = string.Format("{0}  至  {1}", p.StartDate, p.EndDate),
         };

         v.Review = review;
         v.Result.IsSuccess = true;
      }

   }


   public class ProjectStartRequestHandler: ProjectReviewHandler
   {

   }


   public class ProjectStoneTaskReviewHandler : IHandler<ProjectStoneTask, ReviewRequestOption>
   {
      static APDBDef.ReviewTableDef rv = APDBDef.Review;

      public void Handle(ProjectStoneTask pst, ReviewRequestOption v)
      {
         var isInReview = v.db.ReviewDal.ConditionQueryCount(rv.TaskId == pst.PstId & rv.Result == ReviewKeys.ResultWait) > 0;
         if (isInReview)
         {
            v.Result.Msg = Errors.Review.HAS_IN_REVIEW;
            v.Result.IsSuccess = false;
            return;
         }

         var project = v.Project;
         var user = v.User;
         var title = ReviewKeys.GetTypeKeyByValue(v.ReviewType);
         var reviewerId = pst.ReviewerID;
         var review = new Review
         {
            SenderID = user.UserId,
            Sender = user.UserName,
            ReceiverID = reviewerId,
            SendDate = DateTime.Now,
            ReviewDate = DateTime.Parse("1/1/1753").AddYears(100),
            ReviewType = v.ReviewType,
            Result = ReviewKeys.ResultWait,
            ProjectId = pst.ProjectId,
            TaskId = pst.PstId,
            Title = title,
            TaskName = pst.TaskName,
            ProjectName = project.ProjectName,
            //DateRange = string.Format("{0}  至  {1}", pst.StartDate, pst.EndDate)
         };

         v.Review = review;
      }
   }


   public class ProjectStoneTaskEditRequestHandler : ProjectStoneTaskReviewHandler { }


   public class ProejctStoneTaskSubmitRequestHandler : ProjectStoneTaskReviewHandler { }

}
