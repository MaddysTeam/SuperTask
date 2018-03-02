using Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheSite.EvalAnalysis;

namespace TheSite.Models
{

   public class EvalParams
   {
      public Guid TargetId { get; set; }
      public Guid AccessorId { get; set; }
      public Guid PeriodId { get; set; }
      public APDBDef db { get; set; }
      public Guid CurrentTableId { get; set; }
      public Guid AccessorRoleId { get; set; }
      public Guid TargetRoleId { get; set; }
      public Guid GroupId => EvalGroupConfig.DefaultGroupId.ToGuid(Guid.Empty);
      public string TableIds { get; set; }
   }

   public class AutoEvalParams : EvalParams
   {
     public Result Result { get; set; }
     public EvalIndication EvalIndication { get; set; }

      public AutoEvalParams()
      {
         AccessorRoleId=AccessorRoleId.IsEmpty() ? EvalConfig.AutoAccessorRoleId.ToGuid(Guid.Empty) : AccessorRoleId;
      }
   }

   public class SubjectEvalParams : EvalParams
   {
     
   }

   public class SubjectEvalResultParams: SubjectEvalParams
   {
   }

   public class AutoEvalResultParams: AutoEvalParams
   {
  
   }

}
