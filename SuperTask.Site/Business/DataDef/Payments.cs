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
         else if (string.IsNullOrEmpty(PayName) &&(ParentId.IsEmpty())) // 只有父级元素款项名称必填
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.NOT_ALLOWED_NAME_NULL };
         }
         else if (PayType.IsEmpty())
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.NOT_ALLOWED_TYPE_NULL };
         }
         else if(Money<=0 && PayType!=PaymentsKeys.NothingType
            //&& !IsDeliveryType
            )
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
         else if(PayDate < InvoiceDate)
         {
            return new Result { IsSuccess = false, Msg = Errors.Payments.START_MUST_BE_EARLIER_THAN_END };
         }

         return new Result { IsSuccess = true };
      }

      public string Ratio { get; set; }

      public bool IsPayType=> PayType == PaymentsKeys.ProjectPaymentsType || PayType == PaymentsKeys.ProjectPaymentsType;

      public bool IsProjectType => PayType == PaymentsKeys.ProjectPaymentsType;
      public bool IsInternalVenderType => PayType == PaymentsKeys.InternalVenderPaymentsType;
      public bool IsDeliveryType => PayType == PaymentsKeys.CheckBeforeDeliveryType ||
                                    PayType == PaymentsKeys.BondType ||
                                    PayType == PaymentsKeys.GuaranteeType ||
                                    PayType == PaymentsKeys.NothingType;

   }

}