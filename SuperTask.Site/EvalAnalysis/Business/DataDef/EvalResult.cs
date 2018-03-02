using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class EvalResult
   {
    
      public List<EvalResultItem> Items { get; set; }

      public static List<EvalResult> GetTargetEvalResult(Guid accessorId,Guid accessorRoleId, Guid targetId, Guid targetRoleId,int evalType)
      {
         var er = APDBDef.EvalResult;
         return EvalResult.ConditionQuery(er.AccesserId == accessorId
                                                    & er.AccesserRoleId == accessorRoleId
                                                    & er.EvalType == evalType
                                                    & er.TargetId == targetId
                                                    & er.TargetRoleId ==targetRoleId,
                                                    null);
          
      }
   }

}