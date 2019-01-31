using Business.Helper;
using Business.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheSite.Models;

namespace Business
{

   public partial class Project
   {

      [Display(Name = "创建人")]
      public string Creator { get; set; }

      [Display(Name = "所属部门")]
      public string OrgName { get; set; }

      [Display(Name = "项目负责人")]
      public string Header { get; set; }

      [Display(Name = "项目经理")]
      public string Manager { get; set; }

      [Display(Name = "项目状态")]
      public string Status => ProjectKeys.GetStatusKeyById(ProjectStatus);

      [Display(Name = "项目类型")]
      public string Type => ProjectKeys.GetTypeKeyById(ProjectType);

      [Display(Name = "审核者")]
      public string Reviewer { get; set; }

      [Display(Name = "项目进度")]
      public double ProjectProgress { get; set; }

      public Attachment Attachment { get; set; }

      public bool IsPlanStatus => this.ProjectStatus == ProjectKeys.PlanStatus;

      public bool IsProcessStatus => this.ProjectStatus == ProjectKeys.ProcessStatus;

      public bool IsCompleteStatus => this.ProjectStatus == ProjectKeys.CompleteStatus;

      public bool IsEditStatus => ProjectStatus == ProjectKeys.EditStatus;

      public bool IsDelStatus => ProjectStatus == ProjectKeys.DeleteStatus;

      public bool IsReviewStatus => ProjectStatus == ProjectKeys.ReviewStatus;

      public List<Resource> Resources { get; set; }

      public void SetStatus(Guid status)
      {
         this.ProjectStatus = status;
      }


      public void Start()
      {
         SetStatus(ProjectKeys.ProcessStatus);
         RealStartDate = DateTime.Now;
      }


      public void Complete()
      {
         SetStatus(ProjectKeys.CompleteStatus);
         RealEndDate = DateTime.Now;
      }


      public Result Validate()
      {
         var message = string.Empty;
         var result = true;

         if (string.IsNullOrEmpty(ProjectName))
         {
            message = Errors.Project.NOT_ALLOWED_NAME_NULL;
            result = false;
         }
         else if (PMId.IsEmpty())
         {
            message = Errors.Project.NOT_ALLOWED_MANAGER_NULL;
            result = false;
         }
         else if (StartDate > EndDate && EndDate > DateTime.MinValue)
         {
            message = Errors.Project.NOT_ALLOWED_DATE_INVALIDATE_RANGE;
            result = false;
         }
         else if (SecurityScenario.SpecialCharChecker.HasSpecialChar(ProjectName))
         {
            message = Errors.Project.NOT_ALLOWED_SEPCIAL_CHAR;
            result = false;
         }
         else if(!string.IsNullOrEmpty(Code) && !string.IsNullOrEmpty(RealCode))
         {
            message = Errors.Project.NOT_ALLOWED_BOTH_CODE;
            result = false;
         }

         return new Result { IsSuccess = result, Msg = message };
      }


      public virtual string GenerateCode()
         => ProjectKeys.ProjectCodePrefix + (this.GetHashCode() ^ 33);

      public virtual bool HasUploadFile()
         => !string.IsNullOrEmpty(Attachment.RealName) && !string.IsNullOrEmpty(Attachment.Url);

      public virtual bool IsLeader(Guid userId)
         => PMId == userId || ManagerId == userId;


      public static void Initial(Project prj)
      {
         prj.ProjectId = prj.ProjectId.IsEmpty() ? Guid.NewGuid() : prj.ProjectId;
         prj.ProjectStatus = ProjectKeys.PlanStatus;
         prj.CreateDate = DateTime.Now;
         prj.StartDate = DateTime.Now.TodayStart();
         prj.EndDate = prj.StartDate.AddDays(ProjectKeys.DefaultDateRange)
                            .GetNextMondayIfIsWeekend();
         prj.Code = prj.GenerateCode();
         prj.FolderId = Guid.NewGuid();
         prj.PMId = ResourceKeys.TempBossId;
      }


      public override bool Equals(object obj)
       => IsEquals(obj as Project);


      public override int GetHashCode()
      {
         return base.GetHashCode();
      }


      protected virtual bool IsEquals(Project proj)
      {
         if (proj == null) return false;

         if (proj.ProjectName == ProjectName
            && proj.StartDate == StartDate
            && proj.EndDate == EndDate
            && proj.ProjectType == ProjectType
            && proj.ProjectStatus == ProjectStatus
            && proj.RateOfProgress == RateOfProgress
            && proj.PMId == PMId
            && proj.ReviewerId == ReviewerId
            && proj.ManagerId == ManagerId
            && proj.ProjectOwner == ProjectOwner
            && proj.Code == Code
            && proj.RealCode == RealCode
            && proj.ProcessName == ProcessName
            && proj.ProjectOwner == ProjectOwner
            && proj.ProjectExecutor == ProjectExecutor
            )
         {
            return true;
         }

         return false;
      }

   }

}
