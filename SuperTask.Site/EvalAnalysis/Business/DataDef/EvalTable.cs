using Business.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheSite.Models;

namespace Business
{

   public partial class EvalTable
   {

      public bool IsSelected { get; set; }

      public Result Validate()
      {
         var result = new Result { IsSuccess = true, Msg = Success.Task.EDIT_SUCCESS };

         if (string.IsNullOrEmpty(TableName))
         {
            result.Msg = Errors.EvalTable.NOT_ALLOWED_NAME_NULL;
            result.IsSuccess = false;
         }

         if (FullScore <= 0)
         {
            result.Msg = Errors.EvalTable.NOT_ALLOWED_SCORE_LESS_THAN_ZERO;
            result.IsSuccess = false;
         }

         return result;
      }

      public bool IsBuildDone => TableStatus == EvalTableKeys.DoneStatus;

      public bool IsInUse()
      {
         var er = APDBDef.EvalResult;

         return EvalResult.ConditionQueryCount(er.TableId==TableId)>0;
      }

   }

}