using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class EvalResult
   {
    
      public List<EvalResultItem> Items { get; set; }

      public static List<EvalResult> GetTargetEvalResult(Guid periodId,Guid accessorId, Guid targetId,Guid evalType)
      {
         var er = APDBDef.EvalResult;
         return EvalResult.ConditionQuery(er.PeriodId==periodId
                                                    &  er.AccesserId == accessorId
                                                    & er.EvalType == evalType
                                                    & er.TargetId == targetId,
                                                    null);
          
      }
   }

}