using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{

   public partial class Review
   {

      public string Sender { get; set; }

      public string Reciver { get; set; }

      public string TaskName { get; set; }

      public string ProjectName { get; set; }

      public string Type => ReviewKeys.GetTypeKeyByValue(ReviewType);

      public string DateRange { get; set; }

      public double TaskEsHours { get; set; }

      public double TaskHours { get; set; }

      //public string StatusName => ReviewKeys.GetStatusKeyByValue(Result);


      public string GetStatus(int val)
         => ReviewKeys.GetStatusKeyByValue(val);


      public static Dictionary<Guid, string> ReturnUrlsAfterReview => new Dictionary<Guid, string>
      {
         {ReviewKeys.ReviewTypeForPjChanged,"/Project/AfterEditReview" },
         {ReviewKeys.ReviewTypeForTkChanged,"/Task/AfterEditReview" },
         {ReviewKeys.ReviewTypeForTkSubmit,"/Task/AfterSubmitReview" },
         {ReviewKeys.TaskRequestFailed,"/Task/AfterReviewFail"},
         {ReviewKeys.ProjectRequestFailed,"/Project/AfterReviewFail"}
      };

      public static Dictionary<Guid, string> ReturnUrlsAfterSend => new Dictionary<Guid, string>
      {
         {ReviewKeys.ReviewTypeForPjChanged,"/Project/AfterEditReviewSend" },
         {ReviewKeys.ReviewTypeForTkChanged,"/Task/AfterEditReviewSend" },
         {ReviewKeys.ReviewTypeForTkSubmit,"/Task/AfterSubmitReviewSend" },
      };


      public static Dictionary<string, Dictionary<Guid, string>> ReviewStrategy => new Dictionary<string, Dictionary<Guid, string>>
      {
         {"completed", ReturnUrlsAfterReview},
         {"submit", ReturnUrlsAfterSend},
      };

   }

}
