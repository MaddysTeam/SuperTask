using Business;
using Business.Config;
using Business.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheSite.EvalAnalysis;

namespace TheSite.Models
{

   public class PersonalReportModel
   {
      public Guid UserId { get; set; }
      public Guid ProjectId { get; set; }
      public Guid TaskType { get; set; } = TaskKeys.SelectAll;
      public string UserName { get; set; }
      public string ProjectName { get; set; }
      public double TotalTaskCount { get; set; }
      public double CompleteTaskCount { get; set; }
      public double ProcessCount { get; set; }
      public double PlanCount { get; set; }
      public double DelCount { get; set; }
      public double ReviewCount { get; set; }
      public double ProjectHours { get; set; }
      public double WorkHours { get; set; }
      public double EstimateWorkHours { get; set; }
      public double ReturnRatio { get; set; }
      public DateTime StartDate { get; set; } = ThisApp.StartDayPerMonth;
      public DateTime EndDate { get; set; } = ThisApp.EndDayPerMonth;
      public bool IsTotal { get; set; } = false;
   }

   public class EvalReportModel
   {
      public Guid EvalResultId { get; set; }
      public Guid TargetId { get; set; }
      public Guid TargetRoleId { get; set; }
      public Guid PeriodId { get; set; }
      public Guid TableId { get; set; }
      public string TargetRoleName { get; set; }
      public string TableName { get; set; }
      public string TargetName { get; set; }
      public string PeriodName { get; set; }
      public double Score { get; set; }
      public int EvalCount { get; set; }
      public double AdjustScore { get; set; }
   }

   public class EvalResultDetailsViewModel: EvalReportModel
   {
      public Guid AccesserRoleId { get; set; }
      public Guid AccesserId { get; set; }
      public Guid IndicationId { get; set; }
      public double Propertion { get; set; }
      public double FullScore  { get; set; }
      public string IndicationName { get; set; }
      public string IndicationDescription { get; set; }
      public bool IsShowOthersEvalResult { get; set; }

      public List<EvalTable> PeriodTables = new List<EvalTable>();
      public Dictionary<Guid, List<EvalResultItem>> TableResultItems = new Dictionary<Guid, List<EvalResultItem>>();
   }


   public class PersonalScore
   {
      public Guid UserId { get; set; }
      public string UserName { get; set; }
      public double Score { get; set; }
      public double Code { get; set; }
      public double SubValue { get; set; }
      public string SubType { get; set; }
      public string TaskName { get; set; }
      public string ProjectName { get; set; }
      public Guid ProjectId { get; set; }
      public Guid TaskId { get; set; }
   }

   public class PersonalScoreViewModel
   {
      public string UserId { get; set; }
      public string UserName { get; set; }
      public string Score { get; set; }
      public string TaskId { get; set; }
      public string TaskName { get; set; }
      public string ProjectId { get; set; }
      public string ProjectName { get; set; }
      public string SubValue { get; set; }
      public string SubType { get; set; }
      public string UnitScore { get; set; }
   }

   public class PersonalScoreExportViewModel
   {
      public string UserName { get; set; }
      public string ProjectName { get; set; }
      public string TaskName { get; set; }
      public string SubType { get; set; }
      public string UnitScore { get; set; }
      public string SubValue { get; set; }
      public string Score { get; set; }
   }


   public class PMScoreViewModel
   {
      public string UserName { get; set; }
      public double GoodStoneTaskCount { get; set; }
      public double GoodPlanTaskCount { get; set; }
      public double NegativeTaskCount1 { get; set; }
      public double NegativeTaskCount2 { get; set; }
   }


}
