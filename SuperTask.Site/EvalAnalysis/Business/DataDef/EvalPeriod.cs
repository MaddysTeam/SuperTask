using Business.Helper;
using System;
using System.Collections.Generic;

namespace Business
{

   public partial class EvalPeriod
   {

      public static List<EvalPeriod> GetCurrentPeriod(APDBDef db)
      {
         var p = APDBDef.EvalPeriod;
         var current = DateTime.Now;
         var result= db.EvalPeriodDal.ConditionQuery(current <= p.AccessEndDate & current>=p.AccessBeginDate,p.BeginDate.Asc,null,null);
         if(result.Count<=0)
            throw new ApplicationException(Errors.Eval.NOT_ANY_PERIOD);

         return result;
      }

   }

}