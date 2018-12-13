using Business.Helper;
using System;
using System.ComponentModel.DataAnnotations;
using TheSite.Models;

namespace Business
{

   public partial class Indication
   {
      /// <summary>
      /// 如果该指标是自动计算，则绑定该算法id
      /// </summary>
      public Guid AlgorithmnId { get; set; }

      //public static Indication Create(Guid indicationId) => new Indication { IndicationId= indicationId };

      [Display(Name ="指标类型")]
      public string Type => IndicationKeys.GetIndicaitonTypeByValue(IndicationType);


      public Result Validate()
      {
         var result = new Result { IsSuccess = true, Msg = Success.Task.EDIT_SUCCESS };

         if (string.IsNullOrEmpty(IndicationName))
         {
            result.Msg = Errors.Indicaiton.NOT_ALLOWED_NAME_NULL;
            result.IsSuccess = false;
         }

         return result;
      }


      public override bool Equals(object obj)
      {
         var other = obj as Indication;

         if (other == null) return false;

         return IndicationId== other.IndicationId;
      }


      public bool IsInUse()
      {
         var eri = APDBDef.EvalResultItem;
         var evi = APDBDef.EvalIndication;

         return EvalResultItem.ConditionQueryCount(eri.IndicationId == IndicationId) > 0 ||
               EvalIndication.ConditionQueryCount(evi.IndicationId==IndicationId)>0;
      }

   }

}