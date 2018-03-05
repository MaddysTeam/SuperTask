using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheSite.Models;

namespace Business
{

   public partial class WorkJournal
   {

      [Display(Name = "项目名称")]
      public string ProjectName { get; set; }

      [Display(Name = "累计已用工时")]
      public double TaskWorkHours { get; set; }

      [Display(Name = "任务累计得分")]
      public double SubTaskScore { get; set; }

      [Display(Name = "任务名称")]
      public string TaskName { get; set; }

      public Guid TaskType { get; set; }

      public Attachment Attachment { get; set; }

      public string SubTypeTitle { get; set; }


      public virtual bool HasUploadFile()
         => !string.IsNullOrEmpty(Attachment.RealName) && !string.IsNullOrEmpty(Attachment.Url);


      public void SetStatus(Guid status)
      {
         Status = status;
      }


      public Result Validate()
      {
         var message = string.Empty;
         var result = true;

         if (Progress == 100)
         {
            message = Errors.WorkJournal.NEED_PREVIEW;
            result = false;
         }
         if (Progress > 100 || Progress < 0)
         {
            message = Errors.WorkJournal.PROGRESS_OUTOFRANGE;
            result = false;
         }
         if (WorkHours < 0)
         {
            result = false;
            message = Errors.WorkJournal.DATE_OUTOFRANGE;
         }



         return new Result { IsSuccess = result, Msg = message };
      }


      public static JournalQuality CheckQuailty(List<WorkJournal> journals)
      {
         return CheckQuailtyDelegate(journals);
      }

      /// <summary>
      /// 检查日志质量，可以根据需求替换
      /// </summary>
      public static Func<List<WorkJournal>, JournalQuality> CheckQuailtyDelegate { get; set; }
      = (journals) =>
      {
         var recordHours = journals.Sum(j => j.WorkHours);
         var recordDayCount = journals.GroupBy(j => j.RecordDate.TodayStart()).Count();
         var result = JournalQuality.Bad;

         if (recordDayCount < 10)
            result= JournalQuality.Bad;
         else if (recordDayCount > 10 && recordDayCount <= 16)
            result= JournalQuality.Normal;
         else
            result= JournalQuality.Good;

         if (recordHours < 60)
            result = JournalQuality.Bad;
         else if (recordHours > 60 && recordHours < 100)
            result = JournalQuality.Normal;
         else if (recordDayCount >= 14 && recordHours > 100)
            result = JournalQuality.Good;
         else
            result = JournalQuality.Normal;

         return result;
      };

   }

}
