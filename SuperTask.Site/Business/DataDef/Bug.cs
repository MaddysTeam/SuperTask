using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class Bug
   {

      public string Manager { get; set; }

      public string Creator { get; set; }

      [Display(Name = "项目")]
      public string ProjectName => Project.PrimaryGet(Projectid)?.ProjectName;

      [Display(Name = "浏览器")]
      public string BrowserName => BugKeys.GetSystemByValue(BrowserId) ?? string.Empty;

      [Display(Name = "操作系统")]
      public string SystemName => BugKeys.GetSystemByValue(SystemId) ?? string.Empty;

      public Attachment CurrentAttachment { get; set; }

      public string RelativeTaskIds { get; set; }

      public string ConfirmRemark { get; set; }

      public string ResolveRemark { get; set; }

      public string Level => BugKeys.GetLevelByValue(BugLevel);

      public string Type => BugKeys.GetTypeKeyByValue(BugType);

      public string Status => BugKeys.GetStatusKeyByValue(BugStatus);


      public List<TheSite.Models.OperationHistoryViewModel> OperationHistory { get; set; }

      public List<WorkTask> RelativeTasks { get; set; } = new List<WorkTask>();


      [Required]
      public override string BugName
      {
         get
         {
            return base.BugName;
         }

         set
         {
            base.BugName = value;
         }
      }

   }



}