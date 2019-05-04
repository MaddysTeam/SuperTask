﻿using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheSite.Models;

namespace Business
{

   public partial class Review
   {

      public string Sender { get; set; }

      public string Reciver { get; set; }

      [Display(Name = "任务名称")]
      public string TaskName { get; set; }

      [Display(Name ="项目名称")]
      public string ProjectName { get; set; }

      public string Type => ReviewKeys.GetTypeKeyByValue(ReviewType);

      [Display(Name = "时间范围")]
      public string DateRange { get; set; }

      public double TaskEsHours { get; set; }

      public double TaskHours { get; set; }


      public string GetStatus(int val)
         => ReviewKeys.GetStatusKeyByValue(val);


      public static Dictionary<Guid, string> ReturnUrlsAfterReview => new Dictionary<Guid, string>
      {
         {ReviewKeys.ReviewTypeForPjStart,"/Project/AfterProjectStartSubimitReview" },
         {ReviewKeys.ProjectRequestFailed,"/Project/AfterReviewFail"},

         {ReviewKeys.ReviewTypeForTkChanged,"/Task/AfterEditReview" },
         {ReviewKeys.ReviewTypeForTkSubmit,"/Task/AfterSubmitReview" },
         {ReviewKeys.TaskRequestFailed,"/Task/AfterReviewFail"},

         {ReviewKeys.ReviewTypeForStoneTaskChanged,"/ProjectStoneTask/AfterEditReview" },
         {ReviewKeys.ReviewTypeForStoneTaskSubmit,"/ProjectStoneTask/AfterSubmitReview" },
         {ReviewKeys.StoneTaskRequestFailed,"/ProjectStoneTask/AfterReviewFail"},
      };

      public static Dictionary<Guid, string> ReturnUrlsAfterSend => new Dictionary<Guid, string>
      {
         {ReviewKeys.ReviewTypeForPjStart,"/Project/AfterProjectStartReviewSend" },

         {ReviewKeys.ReviewTypeForTkChanged,"/Task/AfterEditReviewSend" },
         {ReviewKeys.ReviewTypeForTkSubmit,"/Task/AfterSubmitReviewSend" },

         {ReviewKeys.ReviewTypeForStoneTaskChanged,"/ProjectStoneTask/AfterEditReviewSend" },
         {ReviewKeys.ReviewTypeForStoneTaskSubmit,"/ProjectStoneTask/AfterSubmitReviewSend" },
      };


      public static Dictionary<string, Dictionary<Guid, string>> ReviewStrategy => new Dictionary<string, Dictionary<Guid, string>>
      {
         {"completed", ReturnUrlsAfterReview},
         {"submit", ReturnUrlsAfterSend},
      };


      public Result Validate()
      {
         var message = string.Empty;
         var result = true;

         if (ProjectId.IsEmpty())
         {
            message = Errors.Review.PROJECT_ID_CANNOT_BE_NULL;
            result = false;
         }
         else if (ReviewType == Guid.Empty)
         {
            message = Errors.Review.REVIEW_TYPE_CANNOT_BE_NULL;
            result = false;
         }
         else if (ReceiverID == Guid.Empty)
         {
            message = Errors.Review.RECEIVER_ID_CANNOT_BE_NULL;
            result = false;
         }

         return new Result { IsSuccess = result, Msg = message };
      }

   }

}
