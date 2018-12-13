using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Business
{

   public partial class EvalPeriod
   {

      public static List<EvalPeriod> GetCurrentPeriod(APDBDef db)
      {
         var p = APDBDef.EvalPeriod;

         var current = DateTime.Now;

         return db.EvalPeriodDal.ConditionQuery(current <= p.AccessEndDate & current>=p.AccessBeginDate,p.BeginDate.Asc,null,null);
      }

   }

}