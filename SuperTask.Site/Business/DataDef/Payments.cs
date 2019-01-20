using Business.Helper;
using System;
using TheSite.Models;

namespace Business
{

   public partial class Payments
   {
      public Result Valiedate()
      {
         if (ProjectId.IsEmpty())
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.EDIT_FAIL };
         }
         else if (string.IsNullOrEmpty(PayName))
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.NOT_ALLOWED_NAME_NULL };
         }
         else if (PayType.IsEmpty())
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.NOT_ALLOWED_TYPE_NULL };
         }
         else if(Money<=0 && !IsDeliveryType)
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.NOT_ALLOWED_Money_ZERO };
         }
         else if (PayDate.IsEmpty())
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.NOT_ALLOWED_TIME_NULL };
         }
         else if (InvoiceDate.IsEmpty() && IsProjectType)
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.NOT_ALLOWED_TIME_NULL };
         }
         else if (PayType == PaymentsKeys.CheckBeforeDeliveryType)
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.NOT_ALLOWED_TYPE_WHEN_DELIVERY };
         }

         return new Result { IsSuccess = true };
      }

      public bool IsProjectType => PayType == PaymentsKeys.ProjectPaymentsType;
      public bool IsInternalVenderType => PayType == PaymentsKeys.InternalVenderPaymentsType;
      public bool IsDeliveryType => PayType == PaymentsKeys.CheckBeforeDeliveryType ||
                                    PayType == PaymentsKeys.BondType ||
                                    PayType == PaymentsKeys.GuaranteeType ||
                                    PayType == PaymentsKeys.NothingType;

   }

}