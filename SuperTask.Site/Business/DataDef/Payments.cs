using Business.Helper;
using System;
using TheSite.Models;

namespace Business
{

   public partial class Payments
   {
      public Result Valiedate()
      {
         if (ProjectId.IsEmpty() || PayType.IsEmpty())
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.EDIT_FAIL };
         }

         return new Result { IsSuccess = true};
      }

      public bool IsProjectType => PayType == PaymentsKeys.ProjectPaymentsType; 
      public bool IsInternalVenderType => PayType == PaymentsKeys.InternalVenderPaymentsType;
      public bool IsDeliveryType => PayType == PaymentsKeys.CheckBeforeDeliveryType ||
                                    PayType == PaymentsKeys.BondType ||
                                    PayType == PaymentsKeys.GuaranteeType ||
                                    PayType == PaymentsKeys.NothingType;

   }

}