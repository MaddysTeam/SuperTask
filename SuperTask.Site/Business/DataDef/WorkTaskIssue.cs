using System;
using System.ComponentModel.DataAnnotations;


namespace Business
{

   public partial class WorkTaskIssue
   {
      [Display(Name = "任务名称")]
      public string TaskName { get; set; }

      [Display(Name = "项目名称")]
      public string ProjectName { get; set; }

      public Guid ProjectId { get; set; }
   }

}
