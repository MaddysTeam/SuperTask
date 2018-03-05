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

      public List<EvalPeriodTable> PeriodTables = new List<EvalPeriodTable>();
      public Dictionary<Guid, List<EvalResultItem>> TableResultItems = new Dictionary<Guid, List<EvalResultItem>>();
   }


}
